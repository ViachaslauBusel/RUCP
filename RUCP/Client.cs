using RUCP.Channels;
using RUCP.Cryptography;
using RUCP.Tools;
using RUCP.Transmitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RUCP
{
    public class Client
    {
        /// <summary>Выступает в роли моста, между представлением клиента, на стороне клиента и сервере</summary>
        private IServer m_server;
        private IProfile m_profile;
        private IPEndPoint m_remoteAdress;
        private NetworkInfo m_network = new NetworkInfo();
        private QueueBuffer m_bufferQueue;
        private ReliableBuffer m_bufferReliable;
        private DiscardBuffer m_bufferDiscard;
        private Object m_locker = new Object();

        public long ID { get; private set; }

       

        /// <summary>false - этот клиент подключен к серверу на прямую. true - это удаленный  клиент, подключен к серверу через сеть</summary>
        internal bool isRemoteHost { get; private set; }
        internal RSA CryptographerRSA { get; set; } = new RSA();
        internal AES CryptographerAES { get; set; } = new AES();

        internal IServer Server => m_server;
        /// <summary>Адрес удаленного узла с которым соединён этот клиент</summary>
        public IPEndPoint RemoteAdress => m_remoteAdress;
        public NetworkInfo Network => m_network;    
        public IProfile Profile => m_profile;



        internal Client(IServer server, IPEndPoint adress)
        {
            isRemoteHost = false;
            m_server = server;
            m_remoteAdress = adress;
           
            ID = SocketInformer.GetID(adress);
            m_profile = server.CreateProfile();

            m_bufferReliable = new ReliableBuffer(this, 512);
            m_bufferQueue = new QueueBuffer(this, 512);
            m_bufferDiscard = new DiscardBuffer(this, 512);
        }
        public Client()
        {
            isRemoteHost = true;
            m_profile = new LocalProfile();
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

        public void SetProfile(IProfile profile)
        {
            m_profile = profile;
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
            CloseConnection(true);
        }
        /// <summary>
        /// Removing a client from the list of connections and calling the CloseConnection method in the profile
        /// </summary>
        public void CloseConnection(bool disconnect = true)
        {
            if (disconnect)
                Disconnect();
            CloseIf(NetworkStatus.СONNECTED);
        }
        internal void CloseIf(NetworkStatus status)
        {
            lock (m_locker)
            {
                if (m_network.Status == status)
                {
                    m_server.Disconnect(this);
                    m_profile.CloseConnection();

                    m_network.Status = NetworkStatus.CLOSED;

                    m_bufferReliable.Dispose();
                    m_bufferQueue.Dispose();
                    m_bufferDiscard.Dispose();

                    CryptographerAES.Dispose();
                    CryptographerRSA.Dispose();
                }
            }
        }
        /// <summary>
		/// Отсылает клиенту команду на отключения
		/// </summary>
		internal void Disconnect()
        {
            Packet.Create(this, Channel.Disconnect).Send();
        }

        //Подтверждение о принятии пакета клиентом
        internal void ConfirmReliableACK(int number) { m_bufferReliable.ConfirmAsk(number); }
        internal void ConfirmQueueACK(int number) { m_bufferQueue.ConfirmAsk(number); }
        internal void ConfirmDiscardACK(int number) { m_bufferDiscard.ConfirmAsk(number); }

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
        private void SendACK(Packet packet, int channel)
        {
            Packet packet1 = Packet.Create(packet.Client, channel);
            packet1.Sequence = packet.Sequence;
            packet1.Send();
        }
        //Обработка пакетов
        internal void ProcessReliable(Packet packet)
        {

            if (m_bufferReliable.Check(packet))
            {
                //Отправка ACK>>
                SendACK(packet, Channel.ReliableACK);
                //Отправка ACK<<
            }

        }
        internal void ProcessQueue(Packet packet)
        {
            
            if (m_bufferQueue.Check(packet))
            {
                //Отправка ACK>>
                SendACK(packet, Channel.QueueACK);
                //Отправка ACK<<
            }
        }
        internal void ProcessDiscard(Packet packet)
        {
           
            if (m_bufferDiscard.Check(packet))
            {
                //Отправка ACK>>
                SendACK(packet, Channel.DiscardACK);
                //Отправка ACK<<
            }
        }
    }
}
