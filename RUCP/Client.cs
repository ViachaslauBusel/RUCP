using RUCP.Channels;
using RUCP.Cryptography;
using RUCP.Tools;
using RUCP.Transmitter;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP
{
    public class Client
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
        internal RSA CryptographerRSA { get; set; } = new RSA();
        internal AES CryptographerAES { get; set; } = new AES();

        internal IServer Server => m_server;
        /// <summary>Адрес удаленного узла с которым соединён этот клиент</summary>
        public IPEndPoint RemoteAddress => m_remoteAdress;
        public NetStream Stream => m_stream;    
        public NetworkStatistic Statistic => m_network;    
        public IProfile Profile => m_profile;



        internal Client(IServer server, IPEndPoint adress)
        {
            isRemoteHost = false;
            m_server = server;
            m_taskPipeline = server.TaskPool.CreatePipeline();
            m_remoteAdress = adress;
            m_stream = new NetStream(this);
           
            ID = SocketInformer.GetID(adress);
            m_profile = server.CreateProfile();

            m_bufferReliable = new ReliableBuffer(this, 512);
            m_bufferQueue = new QueueBuffer(this, 512);
            m_bufferDiscard = new DiscardBuffer(this, 512);
        }
        public Client()
        {
            isRemoteHost = true;
            m_stream = new NetStream(this);
            m_bufferReliable = new ReliableBuffer(this, 512);
            m_bufferQueue = new QueueBuffer(this, 512);
            m_bufferDiscard = new DiscardBuffer(this, 512);
        }

        public void ConnectTo(string adress, int port, bool networkEmulator = false)
        {
            if(m_server != null) { throw new Exception("The client is already connected to the remote host"); }
            if (m_profile == null) { throw new Exception("Профиль для обработки пакетов не задан"); }

            m_remoteAdress = new IPEndPoint(IPAddress.Parse(adress), port);
            m_server = new RemoteServer(this, m_remoteAdress, networkEmulator);
         
            ID = SocketInformer.GetID(m_remoteAdress);
        }

        public void SetHandler(Func<IProfile> getHandler)
        {
            m_profile = getHandler.Invoke();
        }

        internal bool isConnected() => m_network.Status == NetworkStatus.СONNECTED;

        internal void OpenConnection()
        {
            lock (m_locker)
            {
               // Console.WriteLine($"Open connection:{isRemoteHost}");
                if(m_network.Status != NetworkStatus.LISTENING) { throw new Exception("Error opening connection"); }
                m_network.Status = NetworkStatus.СONNECTED;
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

        public void Close()
        {
            CloseConnection(sendDisconnectCMD: true);
        }
        /// <summary>
        /// Removing a client from the list of connections and calling the CloseConnection method in the profile
        /// </summary>
        internal void CloseConnection(bool sendDisconnectCMD = true)
        {
            if (sendDisconnectCMD)
            { SendDisconnectCMD(); }
           
            CloseIf(NetworkStatus.СONNECTED);
        }
        internal void CloseIf(NetworkStatus status)
        {
            lock (m_locker)
            {
                if (m_network.Status == status)
                {
                    m_server.RemoveClient(this);
                    if (m_network.Status == NetworkStatus.СONNECTED) { m_profile.CloseConnection(); }

                    m_network.Status = NetworkStatus.CLOSED;

                    m_bufferReliable.Dispose();
                    m_bufferQueue.Dispose();
                    m_bufferDiscard.Dispose();

                    CryptographerAES.Dispose();
                    CryptographerRSA.Dispose();
                }
            }
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
    }
}
