using RUCP.Channels;
using RUCP.Cryptography;
using RUCP.Tools;
using RUCP.Transmitter;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RUCP
{
    public sealed class Client : IDisposable
    {
        /// <summary>Выступает в роли моста, между представлением клиента, на стороне клиента и сервере</summary>
        private IServer m_server;
        private IProfile m_profile;
        private IPEndPoint m_remoteAdress;
        private NetStream m_stream;
        private NetworkStatistic m_network = new NetworkStatistic();
        private QueueBuffer m_bufferQueue;
        private ReliableBuffer m_bufferReliable;
        private DiscardBuffer m_bufferDiscard;
        private Object m_locker = new Object();
        private TaskPipeline m_taskPipeline;
        public long ID { get; private set; }

       

        /// <summary>false - этот клиент подключен к серверу на прямую. true - это удаленный  клиент, подключен к серверу через сеть</summary>
        public bool isRemoteHost { get; private set; }
        internal RSA CryptographerRSA { get; private set; }
        internal AES CryptographerAES { get; private set; } 

        internal IServer Server => m_server;
        /// <summary>Адрес удаленного узла с которым соединён этот клиент</summary>
        public IPEndPoint RemoteAddress => m_remoteAdress;
        public NetStream Stream => m_stream;    
        public NetworkStatistic Statistic => m_network;    
        public IProfile Profile => m_profile;



        //Этот клиент создается сервером и не может быть повторно использован
        internal Client(IServer server, IPEndPoint adress)
        {
            isRemoteHost = false;
            m_server = server;
            m_taskPipeline = server.TaskPool.CreatePipeline(this);
            m_remoteAdress = adress;
            m_stream = new NetStream(this);

            CryptographerRSA = new RSA();
            CryptographerAES = new AES();

            ID = SocketInformer.GetID(adress);
            m_profile = server.CreateProfile();

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
            if (m_profile == null) { throw new Exception("Профиль для обработки пакетов не задан"); }
            if(options == null) options =new ServerOptions();

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

        public void SetHandler(Func<IProfile> getHandler)
        {
            m_profile = getHandler.Invoke();
        }

        internal bool isConnected() => m_network.Status == NetworkStatus.CONNECTED;

        internal void BufferTick()
        {
            m_bufferReliable?.Tick();
            m_bufferQueue?.Tick();
            m_bufferDiscard?.Tick();
        }
        internal void OpenConnection()
        {
            lock (m_locker)
            {
               // Console.WriteLine($"Open connection:{isRemoteHost}");
                if(m_network.Status != NetworkStatus.LISTENING) { throw new Exception("Error opening connection"); }
                m_network.Status = NetworkStatus.CONNECTED;
                m_profile.OpenConnection();
            }
           
        }
        /// <summary>
        /// Передает пакеты в Обрабатывающий класс
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

        /// <summary>
        /// Отсылает клиенту команду на отключения
        /// </summary>
        internal void SendDisconnectCMD()
        {
            Packet packet = Packet.Create();
            packet.TechnicalChannel = TechnicalChannel.Disconnect;
            packet.InitClient(this);
            packet.SendImmediately();
        }
        private void SendACK(ushort sequence, int channel)
        {
            Packet ack = Packet.Create();
            ack.InitClient(this);
            ack.TechnicalChannel = channel;
            ack.Sequence = sequence;
            ack.Send();
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
                    m_bufferReliable.Insert(packet);
                    return true;
                case Channel.Queue:
                    m_bufferQueue.Insert(packet);
                    return true;
                case Channel.Discard:
                    m_bufferDiscard.Insert(packet);
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

        /// <summary>
        /// Close connection to remote host
        /// </summary>
        public void Close()
        {
            try
            {
                CloseConnection(sendDisconnectCMD: true);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }


        }
        /// <summary>
        /// Removing a client from the list of connections and calling the CloseConnection method in the profile
        /// </summary>
        internal void CloseConnection(bool sendDisconnectCMD = true)
        {
            if (sendDisconnectCMD && NetworkStatus.LISTENING.HasFlag(m_network.Status))
            { SendDisconnectCMD(); }

            if (CloseIf(NetworkStatus.CONNECTED | NetworkStatus.LISTENING))
            { Dispose(); }
        }
        internal bool CloseIf(NetworkStatus status)
        {
            lock (m_locker)
            {
                if (status.HasFlag(m_network.Status))
                {
                //    if (isRemoteHost) Console.WriteLine($"status:{status}");
                    m_network.Status = NetworkStatus.CLOSED;

                    if (m_server.RemoveClient(this)) { m_profile?.CloseConnection(); }

                    return true;
                }
                return false;
            }
        }

        public void Send(Packet packet)
        {
            packet.InitClient(this);
            packet.Send();
        }
        public void Send(Packet packet, Channel channel)
        {
            packet.InitClient(this);
            packet.TechnicalChannel = (int)channel;
            packet.Send();
        }
        public void Dispose()
        {
            lock (m_locker)
            {
                CloseIf(NetworkStatus.CONNECTED | NetworkStatus.LISTENING);
                if (m_network.Status == NetworkStatus.CLOSED)
                {



                    // m_remoteAdress = null;
                    m_stream?.Dispose();
                    // m_stream = null;

                    m_bufferReliable?.Dispose();
                    m_bufferQueue?.Dispose();
                    m_bufferDiscard?.Dispose();

                    m_bufferReliable = null;
                    m_bufferQueue = null;
                    m_bufferDiscard = null;

                    CryptographerAES?.Dispose();
                    CryptographerRSA?.Dispose();

                    CryptographerAES = null;
                    CryptographerRSA = null;

                    m_server = null;
                }
            }
        }
    }
}
