using RUCP.ServerSide;
using RUCP.Transmitter;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP
{
    /// <summary>
    /// Служит для соединения с удаленным сервером
    /// </summary>
    internal sealed class RemoteServer : IServer
    {
        private Client m_master;
        private ISocket m_socket;
        private Resender m_resender;
        private CheckingConnections m_cheking;
        private volatile bool m_work = true;
        private long m_firstPing = -1;
        private ServerOptions m_options;
        private Thread m_server_th;
        private ClientList m_clients;

        public ISocket Socket => m_socket;

        public Resender Resender => m_resender;

        public TaskPool TaskPool => throw new NotImplementedException();

        public ServerOptions Options => m_options;

        public ClientList ClientList => m_clients;

        internal RemoteServer(Client client, IPEndPoint iPEndPoint, ServerOptions options)
        {
            m_options = options;
            m_socket =  UDPSocket.CreateSocket();
            m_socket.Connect(iPEndPoint);

            m_clients = new ClientList(this);
           

            m_master = client;
            //Запуск потока переотправки потеряных пакетов
            m_resender = Resender.Start(this);
            //Запуск потока проверки соединения
            m_cheking = CheckingConnections.Start(this);

            m_cheking.InsertClient(client);

            //Запуск потока считывание датаграмм
            m_server_th = new Thread(() => Listener());
            m_server_th.IsBackground = true;
            m_server_th.Start();

            Thread th = new Thread(() => Connector(client));
            th.Start();

        }
        public IProfile CreateProfile()
        {
            return null;
        }

        public void CallException(Exception exception)
        {
          //  m_master.HandleException(exception);
            Console.Error.WriteLine($"RemoteServer: {exception.ToString()}");
        }

        //Вызывается после получения ответа от сервера
        public bool AddClient(Client client)
        {
            if (m_clients.AddClient(client))
            {
                int ping = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m_firstPing);

                client.Statistic.InitPing(ping + 15);
                return true;
            }
            return false;
        }

      
        public bool RemoveClient(Client client)
        {
            if (m_clients.RemoveClient(client))
            {
                m_work = false;
                Stop();
                return true;
            }
            return false;
        }

        private async void Stop()
        {
            await Task.Run(() => m_resender?.Stop());
            await Task.Run(() => m_cheking?.Stop());
            await Task.Run(() => m_socket?.Close());
        }

        /// <summary>
        /// Отпровляем запрос серверу на подключение
        /// </summary>
        private void Connector(Client client)
        {
            Packet packet = Packet.Create();
            packet.InitClient(client);
            packet.TechnicalChannel = TechnicalChannel.Connection;
            packet.WriteFloat(Config.VESRSION);
            client.CryptographerRSA.WritePublicKey(packet);
            client.CryptographerRSA.Encrypt(packet);

                int max_cicle = 20;//10 сек ожидание подключения
                m_firstPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (client.Statistic.Status == NetworkStatus.LISTENING) //Ожидаем подключение
            {
                if (--max_cicle < 0) { break; }
                //Отпровляем пакет
                Socket.Send(packet);

                Thread.Sleep(500); //Ожидание пакета "подтверждение подключение" от сервера
            }
            packet.Dispose();
            //Закрыть если сервер так и не ответил
            client.CloseIf(NetworkStatus.LISTENING);
        }


        private void Listener()
        {
            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            while (m_work)
            {
                try
                {          
                    //Создание нового пакета для хранение данных
                    Packet packet = Packet.Create();

                    //Считывание датаграм
                    int receiveBytes = m_socket.ReceiveFrom(packet.Data, ref senderRemote);
                    packet.InitData(receiveBytes);
                    packet.InitClient(m_master);

                //    Console.WriteLine($"remote host receive packet.ch{packet.TechnicalChannel}");

                    PacketHandler.Process(this, packet);

                    m_master.Stream?.Flush();

                   
                }
                //catch (SocketException e)
                //{
                //    //if (e.ErrorCode == 10004)
                //    //    break;
                //    //if (e.ErrorCode == 10054)
                //    //    continue;
                //    if (m_work)
                //    {
                //        // CallException(e);
                //        if (m_master.HandleException(e))
                //        { m_master.Close(); }
                //    //    CallException(new Exception($"Client:{m_master.ID} was disconnected due to an unhandled exception"));
                //    }

                //}
                catch (SocketException e)
                {
                    //e.ErrorCode == 10054 The remote host forcibly terminated the existing connection
                    if (e.ErrorCode == 10054)
                    {
                        //TODO Handle the exception thrown by the local socket when sending a packet to a remote closed socket
                        //CallException(new Exception($"The client:{remoteSender} was disconnected. Error code:{e.ErrorCode}"));
                        continue;
                    }
                    //if (e.ErrorCode != 10004 && e.ErrorCode != 4)

                    if (m_master.HandleException(e))
                    { m_master.Close(); }
                }
                catch (Exception e)
                {
                    if (m_work)
                    {
                        //CallException(e);
                        if (m_master.HandleException(e))
                        { m_master.Close(); }
                        //CallException(new Exception($"Client:{m_master.ID} was disconnected due to an unhandled exception"));
                    }
                }
            }
        }
            
    }
}
