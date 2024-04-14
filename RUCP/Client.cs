using RUCP.Channels;
using RUCP.Cryptography;
using RUCP.DATA;
using RUCP.Tools;
using RUCP.Transmitter;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace RUCP
{
    public sealed class Client : IDisposable
    {
        /// <summary>Acts as a bridge between remote client and server client</summary>
        private IServer _server;
        private BaseProfile _profile;
        private IPEndPoint _remoteAdress;
        private NetStream _stream;
        private NetworkStatus _status = NetworkStatus.CLOSED;
        private NetworkStatistic _statistic = new NetworkStatistic();
        private QueueBuffer _bufferQueue;
        private ReliableBuffer _bufferReliable;
        private DiscardBuffer _bufferDiscard;
        private Object _locker = new Object();
        private TaskPipeline _taskPipeline;
        private Packet _disconnectPacket;
      



      
        internal RSA CryptographerRSA { get; private set; }
        internal AES CryptographerAES { get; private set; } 
        internal IServer Server => _server;


        public long ID { get; private set; }
        /// <summary>
        /// false - this client is directly connected to the server(server client). true - this is a remote client connected to the server via the network(remote client)
        /// </summary>
        public bool isRemoteHost { get; private set; }


        /// <summary>The address of the remote host to which this client is connected</summary>
        public IPEndPoint RemoteAddress => _remoteAdress;
        public NetStream Stream => _stream;    
        public NetworkStatistic Statistic => _statistic;
        public NetworkStatus Status => _status;
        public BaseProfile Profile => _profile;


        public Packet GetDisconnectPacket()
        {
            lock (_locker)
            {
                if (_disconnectPacket == null)
                {
                    _disconnectPacket = Packet.Create();
                    _disconnectPacket.TechnicalChannel = TechnicalChannel.Disconnect;
                }
                return _disconnectPacket;
            }
        }





        //This client is created by the server and cannot be reused
        internal Client(IServer server, IPEndPoint adress)
        {
            _status = NetworkStatus.LISTENING;
            isRemoteHost = false;
            _server = server;
            _taskPipeline = server.TaskPool.CreatePipeline(this);
            _remoteAdress = adress;
            _stream = new NetStream(this);

            CryptographerRSA = new RSA();
            CryptographerAES = new AES();

            ID = SocketInformer.GetID(adress);
            SetHandler(() => server.CreateProfile());

            _bufferReliable = new ReliableBuffer(this, 512);
            _bufferQueue = new QueueBuffer(this, 512);
            _bufferDiscard = new DiscardBuffer(this, 512);
        }
        public Client()
        {
            isRemoteHost = true;
           
        }

        public void ConnectTo(string address, int port, ServerOptions options = null)
        {
            if(_server != null) { throw new Exception("The client is already connected to the remote host"); }
            if (_profile == null) { throw new Exception("Client profile not set"); }
            if(options == null) options = new ServerOptions();

            CryptographerRSA = new RSA();
            CryptographerAES = new AES();

            _stream = new NetStream(this);
            _bufferReliable = new ReliableBuffer(this, 512);
            _bufferQueue = new QueueBuffer(this, 512);
            _bufferDiscard = new DiscardBuffer(this, 512);

            _remoteAdress = new IPEndPoint(IPAddress.Parse(address), port);
            _server = new RemoteServer(this, _remoteAdress, options);
         
            ID = SocketInformer.GetID(_remoteAdress);
        }

        public void SetHandler(Func<BaseProfile> getHandler)
        {
            _profile = getHandler.Invoke();
            _profile.TechnicalInit(this);
        }

        internal bool isConnected() => _status == NetworkStatus.CONNECTED;

        internal void BufferTick()
        {
            _bufferReliable?.Tick();
            _bufferQueue?.Tick();
            _bufferDiscard?.Tick();
        }


        /// <summary>
        /// Passes packets to the Handling method
        /// </summary>
        internal void HandlerPack(Packet packet)
        {
            if (packet.Encrypt) CryptographerAES.Decrypt(packet);
            _profile.ChannelRead(packet);
         
        }
        internal void checkingConnection()
        {
            _profile.CheckingConnection();
        }

       

        internal void InsertTask(Action act)
        {
            _taskPipeline.Insert(new Task(act));
        }

        private void SendACK(ushort sequence, int channel)
        {
            Packet ack = Packet.Create();
            ack.TechnicalChannel = channel;
            ack.Sequence = sequence;
            Stream.Write(ack);
            ack.Dispose();
        }
        //Подтверждение о принятии пакета клиентом
        internal void ConfirmReliableACK(int sequence) { _bufferReliable.ConfirmAsk(sequence); }
        internal void ConfirmQueueACK(int sequence) { _bufferQueue.ConfirmAsk(sequence); }
        internal void ConfirmDiscardACK(int sequence) { _bufferDiscard.ConfirmAsk(sequence); }

        /// <summary>
        /// Вставка в буффер не подтвержденных пакетов
        /// </summary>
        internal bool InsertBuffer(Packet packet)
        {
            switch (packet.Channel)
            {
                case Channel.Reliable:
                    _bufferReliable?.Insert(packet);
                    return true;
                case Channel.Queue:
                    _bufferQueue?.Insert(packet);
                    return true;
                case Channel.Discard:
                    _bufferDiscard?.Insert(packet);
                    return true;

                default: return false;
            }
        }

        internal bool HandleException(Exception e) => _profile.HandleException(e);

        //Обработка пакетов
        internal void ProcessReliable(Packet packet)
        {
            ushort sequence = packet.Sequence;
            if (_bufferReliable.Check(packet))
            {
                //Отправка ACK>>
                SendACK(sequence, TechnicalChannel.ReliableACK);
                //Отправка ACK<<
            }

        }
        internal void ProcessQueue(Packet packet)
        {
            ushort sequence = packet.Sequence;
            if (_bufferQueue.Check(packet))
            {
                //Отправка ACK>>
                SendACK(sequence, TechnicalChannel.QueueACK);
                //Отправка ACK<<
            }
        }
        internal void ProcessDiscard(Packet packet)
        {
            ushort sequence = packet.Sequence;
            if (_bufferDiscard.Check(packet))
            {
                //Отправка ACK>>
                SendACK(sequence, TechnicalChannel.DiscardACK);
                //Отправка ACK<<
            }
        }


        public void Send(Packet packet, short opCode, Channel channel)
        {
            packet.OpCode = opCode;
            packet.TechnicalChannel = (int)channel;
            Send(packet);
        }
        public void Send(Packet packet, Channel channel)
        {
            packet.TechnicalChannel = (int)channel;
            Send(packet);
        }
        public NetStream Send(Packet packet)
        {
            try
            {
                if (packet.DataAccess != DataAccess.Write)
                {
                    _profile?.HandleException(new Exception($"Packet is blocked, sending is not possible"));
                }
                if (Status != NetworkStatus.CONNECTED)
                {
                    _profile?.HandleException(new Exception($"Attempt to write to a closed socket"));
                }

                Packet keepPacket = Packet.Create(packet);

                if (keepPacket.Encrypt) CryptographerAES.Encrypt(keepPacket);

                //Pasting into the buffer of sent packets for further confirmation of the successful delivery of the packet
                if (InsertBuffer(keepPacket))
                {
                    Statistic.SentPackets++; ;
                }

                Stream?.Write(keepPacket);
            }
            catch
            {
                Close();
            }
            return Stream;
        }
        internal void WriteInSocket(Packet packet)
        {
            Server?.Socket?.SendTo(packet, RemoteAddress);
        }


        #region Connection status management
        /// <summary>
        /// Close connection to remote host
        /// </summary>
        public async Task AsyncClose()
        {
            StartClosingCycle();

            int maxAttempts = 1000;
            while (_status != NetworkStatus.CLOSED && maxAttempts-- > 0)
            {
                await Task.Delay(5);
            }
        }
        /// <summary>
        /// Close connection to remote host
        /// </summary>
        public void Close()
        {
            StartClosingCycle();
        }

        internal bool StartListening()
        {
            lock (_locker)
            {
                if ((NetworkStatus.CLOSED).HasFlag(_status))
                {
                    _status = NetworkStatus.LISTENING;
                    return true;
                }
            }
                return false;
        }
        internal bool TryOpenConnection()
        {
            lock (_locker)
            {
                if ((NetworkStatus.LISTENING).HasFlag(_status))
                {
                    _status = NetworkStatus.CONNECTED;

                    if (_server.AddClient(this))
                    {
                        _profile.OpenConnection();
                    }
                    else
                    {
                        _status = NetworkStatus.CLOSED;
                    }
                }
            }
            return true;
        }
        internal void StartClosingCycle()
        {
            lock (_locker)
            {
                if ((NetworkStatus.LISTENING | NetworkStatus.CONNECTED).HasFlag(_status))
                {
                    _status = NetworkStatus.CLOSE_WAIT;

                    Packet packet = GetDisconnectPacket();
                    WriteInSocket(packet);
                    packet.WriteSendTime(Statistic.GetTimeoutInterval());
                }
            }
        }
        /// <summary>
        /// Closes the socket for sending/receiving. Stops all services for a client
        /// </summary>
        internal void CloseConnection(DisconnectReason reason, bool sendNotifications = false)
        {
            lock (_locker)
            {
                Console.WriteLine($"CloseConnection:{_status}");
                try
                {
                    if ((NetworkStatus.LISTENING | NetworkStatus.CONNECTED | NetworkStatus.CLOSE_WAIT).HasFlag(_status))
                    {

                        _status = NetworkStatus.CLOSED;
                        try
                        {
                            if (sendNotifications)
                            {
                                WriteInSocket(GetDisconnectPacket());
                            }
                        }
                        catch { }
                    }
                    //Provides single call conditions
                    if (_server.RemoveClient(this))
                    {
                        Console.WriteLine($"CloseConnection:RemoveClient");
                        _profile?.CloseConnection(reason);
                    }
                    else
                    {
                        Console.WriteLine($"CloseConnection:RemoveClient failed");
                    }
                }catch (Exception e)
                {
                   Console.WriteLine($"CloseConnection:{e.Message}");
                }
            }
        }
        
        


      
      

      

        #endregion
        public void Dispose()
        {
            //lock (_locker)
            //{
            //    CloseIf(NetworkStatus.CONNECTED | NetworkStatus.LISTENING);
            //    if (_status == NetworkStatus.CLOSED)
            //    {



            //        // _remoteAdress = null;
            //        _stream?.Dispose();
            //        // _stream = null;

            //        _bufferReliable?.Dispose();
            //        _bufferQueue?.Dispose();
            //        _bufferDiscard?.Dispose();

            //        _bufferReliable = null;
            //        _bufferQueue = null;
            //        _bufferDiscard = null;

            //        CryptographerAES?.Dispose();
            //        CryptographerRSA?.Dispose();

            //        CryptographerAES = null;
            //        CryptographerRSA = null;

            //        _server = null;
            //    }
            //}
        }
    }
}
