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
        private IServer m_server;
        private BaseProfile m_profile;
        private IPEndPoint m_remoteAdress;
        private NetStream m_stream;
        private NetworkStatus m_status = NetworkStatus.CLOSED;
        private NetworkStatistic m_statistic = new NetworkStatistic();
        private QueueBuffer m_bufferQueue;
        private ReliableBuffer m_bufferReliable;
        private DiscardBuffer m_bufferDiscard;
        private Object m_locker = new Object();
        private TaskPipeline m_taskPipeline;
        private Packet m_disconnectPacket;
      



      
        internal RSA CryptographerRSA { get; private set; }
        internal AES CryptographerAES { get; private set; } 
        internal IServer Server => m_server;


        public long ID { get; private set; }
        /// <summary>
        /// false - this client is directly connected to the server(server client). true - this is a remote client connected to the server via the network(remote client)
        /// </summary>
        public bool isRemoteHost { get; private set; }


        /// <summary>The address of the remote host to which this client is connected</summary>
        public IPEndPoint RemoteAddress => m_remoteAdress;
        public NetStream Stream => m_stream;    
        public NetworkStatistic Statistic => m_statistic;
        public NetworkStatus Status => m_status;
        public BaseProfile Profile => m_profile;


        public Packet GetDisconnectPacket()
        {
            lock (m_locker)
            {
                if (m_disconnectPacket == null)
                {
                    m_disconnectPacket = Packet.Create();
                    m_disconnectPacket.TechnicalChannel = TechnicalChannel.Disconnect;
                }
                return m_disconnectPacket;
            }
        }





        //This client is created by the server and cannot be reused
        internal Client(IServer server, IPEndPoint adress)
        {
            m_status = NetworkStatus.LISTENING;
            isRemoteHost = false;
            m_server = server;
            m_taskPipeline = server.TaskPool.CreatePipeline(this);
            m_remoteAdress = adress;
            m_stream = new NetStream(this);

            CryptographerRSA = new RSA();
            CryptographerAES = new AES();

            ID = SocketInformer.GetID(adress);
            SetHandler(() => server.CreateProfile());

            m_bufferReliable = new ReliableBuffer(this, 512);
            m_bufferQueue = new QueueBuffer(this, 512);
            m_bufferDiscard = new DiscardBuffer(this, 512);
        }
        public Client()
        {
            isRemoteHost = true;
           
        }

        public void ConnectTo(string address, int port, ServerOptions options = null)
        {
            if(m_server != null) { throw new Exception("The client is already connected to the remote host"); }
            if (m_profile == null) { throw new Exception("Client profile not set"); }
            if(options == null) options = new ServerOptions();

            CryptographerRSA = new RSA();
            CryptographerAES = new AES();

            m_stream = new NetStream(this);
            m_bufferReliable = new ReliableBuffer(this, 512);
            m_bufferQueue = new QueueBuffer(this, 512);
            m_bufferDiscard = new DiscardBuffer(this, 512);

            m_remoteAdress = new IPEndPoint(IPAddress.Parse(address), port);
            m_server = new RemoteServer(this, m_remoteAdress, options);
         
            ID = SocketInformer.GetID(m_remoteAdress);
        }

        public void SetHandler(Func<BaseProfile> getHandler)
        {
            m_profile = getHandler.Invoke();
            m_profile.TechnicalInit(this);
        }

        internal bool isConnected() => m_status == NetworkStatus.CONNECTED;

        internal void BufferTick()
        {
            m_bufferReliable?.Tick();
            m_bufferQueue?.Tick();
            m_bufferDiscard?.Tick();
        }


        /// <summary>
        /// Passes packets to the Handling method
        /// </summary>
        internal void HandlerPack(Packet packet)
        {
            if (packet.Encrypt) CryptographerAES.Decrypt(packet);
            m_profile.ChannelRead(packet);
         
        }
        internal void checkingConnection()
        {
            m_profile.CheckingConnection();
        }

       

        internal void InsertTask(Action act)
        {
            m_taskPipeline.Insert(new Task(act));
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
        internal void ConfirmReliableACK(int sequence) { m_bufferReliable.ConfirmAsk(sequence); }
        internal void ConfirmQueueACK(int sequence) { m_bufferQueue.ConfirmAsk(sequence); }
        internal void ConfirmDiscardACK(int sequence) { m_bufferDiscard.ConfirmAsk(sequence); }

        /// <summary>
        /// Вставка в буффер не подтвержденных пакетов
        /// </summary>
        internal bool InsertBuffer(Packet packet)
        {
            switch (packet.Channel)
            {
                case Channel.Reliable:
                    m_bufferReliable?.Insert(packet);
                    return true;
                case Channel.Queue:
                    m_bufferQueue?.Insert(packet);
                    return true;
                case Channel.Discard:
                    m_bufferDiscard?.Insert(packet);
                    return true;

                default: return false;
            }
        }

        internal bool HandleException(Exception e) => m_profile.HandleException(e);

        //Обработка пакетов
        internal void ProcessReliable(Packet packet)
        {
            ushort sequence = packet.Sequence;
            if (m_bufferReliable.Check(packet))
            {
                //Отправка ACK>>
                SendACK(sequence, TechnicalChannel.ReliableACK);
                //Отправка ACK<<
            }

        }
        internal void ProcessQueue(Packet packet)
        {
            ushort sequence = packet.Sequence;
            if (m_bufferQueue.Check(packet))
            {
                //Отправка ACK>>
                SendACK(sequence, TechnicalChannel.QueueACK);
                //Отправка ACK<<
            }
        }
        internal void ProcessDiscard(Packet packet)
        {
            ushort sequence = packet.Sequence;
            if (m_bufferDiscard.Check(packet))
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
            if (packet.DataAccess != DataAccess.Write)
            {
                throw new Exception($"Packet is blocked, sending is not possible");
            }
            if(Status != NetworkStatus.CONNECTED)
            {
                throw new Exception($"Attempt to write to a closed socket");
            }

            Packet keepPacket = Packet.Create(packet);

            if (keepPacket.Encrypt) CryptographerAES.Encrypt(keepPacket);

            //Pasting into the buffer of sent packets for further confirmation of the successful delivery of the packet
            if (InsertBuffer(keepPacket))
            {
                Statistic.SentPackets++;;
            }


            Stream?.Write(keepPacket);


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

            while (m_status != NetworkStatus.CLOSED)
            {
                await Task.Delay(1);
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
            lock (m_locker)
            {
                if ((NetworkStatus.CLOSED).HasFlag(m_status))
                {
                    m_status = NetworkStatus.LISTENING;
                    return true;
                }
            }
                return false;
        }
        internal bool TryOpenConnection()
        {
            lock (m_locker)
            {
                if ((NetworkStatus.LISTENING).HasFlag(m_status))
                {
                    m_status = NetworkStatus.CONNECTED;

                    if (m_server.AddClient(this))
                    {
                        m_profile.OpenConnection();
                    }
                    else
                    {
                        m_status = NetworkStatus.CLOSED;
                    }
                }
            }
            return true;
        }
        internal void StartClosingCycle()
        {
            lock (m_locker)
            {
                if ((NetworkStatus.LISTENING | NetworkStatus.CONNECTED).HasFlag(m_status))
                {
                    m_status = NetworkStatus.CLOSE_WAIT;

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
            lock (m_locker)
            {
                if ((NetworkStatus.LISTENING | NetworkStatus.CONNECTED | NetworkStatus.CLOSE_WAIT).HasFlag(m_status))
                {

                    m_status = NetworkStatus.CLOSED;
                    try
                    {
                        if (sendNotifications)
                        {
                            WriteInSocket(GetDisconnectPacket());
                        }
                    } catch { }
                    //Provides single call conditions
                    if (m_server.RemoveClient(this))
                    {
                        m_profile?.CloseConnection(reason);
                    }
                }
            }
        }
        
        


      
      

      

        #endregion
        public void Dispose()
        {
            //lock (m_locker)
            //{
            //    CloseIf(NetworkStatus.CONNECTED | NetworkStatus.LISTENING);
            //    if (m_status == NetworkStatus.CLOSED)
            //    {



            //        // m_remoteAdress = null;
            //        m_stream?.Dispose();
            //        // m_stream = null;

            //        m_bufferReliable?.Dispose();
            //        m_bufferQueue?.Dispose();
            //        m_bufferDiscard?.Dispose();

            //        m_bufferReliable = null;
            //        m_bufferQueue = null;
            //        m_bufferDiscard = null;

            //        CryptographerAES?.Dispose();
            //        CryptographerRSA?.Dispose();

            //        CryptographerAES = null;
            //        CryptographerRSA = null;

            //        m_server = null;
            //    }
            //}
        }
    }
}
