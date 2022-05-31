using RUCP.Transmitter;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RUCP
{
    /// <summary>
    /// Служит для соединения с удаленным сервером
    /// </summary>
    internal class RemoteServer : IServer
    {
        private Client m_master;
        private ISocket m_socket;
        private Resender m_resender;
        private CheckingConnections m_cheking;
        public ISocket Socket => m_socket;

        public Resender Resender => m_resender;

        internal RemoteServer(Client client, IPEndPoint iPEndPoint, bool networkEmulator = false)
        {
            m_socket = networkEmulator ? NetworkEmulator.CreateNetworkEmulatorSocket() : UDPSocket.CreateSocket();
            m_socket.Connect(iPEndPoint);

            m_master = client;
            //Запуск потока переотправки потеряных пакетов
            m_resender = Resender.Start(this);
            //Запуск потока проверки соединения
            m_cheking = CheckingConnections.Start(this);

            m_cheking.InsertClient(client);

            //Запуск потока считывание датаграмм
            Thread server_th = new Thread(() => Listener());
            server_th.IsBackground = true;
            server_th.Start();

            Thread th = new Thread(() => Connector(client));
            th.Start();
        }

        public void CallException(Exception exception)
        {

        }

        public bool Connect(Client client)
        {
          
            return true;
        }

        public IProfile CreateProfile()
        {
          return null;
        }

        public bool Disconnect(Client client)
        {
            return true;
        }


        /// <summary>
        /// Отпровляем запрос серверу на подключение
        /// </summary>
        private void Connector(Client client)
        {
            Packet packet = Packet.Create();
            packet.TechnicalChannel = TechnicalChannel.Connection;
            packet.WriteFloat(Config.VESRSION);
            client.CryptographerRSA.WritePublicKey(packet);
            client.CryptographerRSA.Encrypt(packet);

                int max_cicle = 20;//10 сек ожидание подключения
                long firstPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                while (client.Network.Status == NetworkStatus.LISTENING) //Ожидаем подключение
                {
                    if (--max_cicle < 0) {  return; }
                    //Отпровляем пакет
                    Socket.Send(packet);

                Thread.Sleep(500); //Ожидание пакета "подтверждение подключение" от сервера
                }
                //Закрыть если сервер так и не ответил
            client.CloseIf(NetworkStatus.LISTENING);
        }


        private void Listener()
        {
            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {            //Создание нового пакета для хранение данных
                    Packet packet = Packet.Create();

                    //Считывание датаграм
                    int receiveBytes = m_socket.ReceiveFrom(packet.Data, ref senderRemote);
                    packet.InitData(receiveBytes);
                    packet.InitClient(m_master);

                    PacketHandler.Process(this, packet);


                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10004)
                        break;
                    if (e.ErrorCode == 10054)
                        continue;
                    CallException(e);
                }
                catch (Exception e)
                {
                    CallException(e);
                }
            }
        }
            
    }
}
