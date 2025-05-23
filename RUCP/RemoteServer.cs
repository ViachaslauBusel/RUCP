﻿using RUCP.ServerSide;
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
            if (!client.StartListening()) return;
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
        public BaseProfile CreateProfile()
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
            StopServices();

            return m_clients.RemoveClient(client);
        }

        private void StopServices()
        {
           
            m_work = false;
            m_resender?.Stop();
            m_cheking?.Stop();
            m_socket?.Close();
        }

        /// <summary>
        /// Sending a connection request to the server
        /// </summary>
        private void Connector(Client client)
        {
            Packet packet = Packet.Create();
            packet.TechnicalChannel = TechnicalChannel.Connection;
            packet.WriteFloat(Config.VESRSION);
            client.CryptographerRSA.WritePublicKey(packet);
            client.CryptographerRSA.Encrypt(packet);

                int max_cicle = 10;//5 сек ожидание подключения
                m_firstPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (client.Status == NetworkStatus.LISTENING) //Ожидаем подключение
            {
                if (--max_cicle < 0) { break; }
                //Send packet
                client.WriteInSocket(packet);

                Thread.Sleep(500); //Ожидание пакета "подтверждение подключение" от сервера
            }
            packet.Dispose();
            //Закрыть если сервер так и не ответил
            if (client.Status == NetworkStatus.LISTENING)
            {
                client.CloseConnection(DisconnectReason.ConnectionFailed);
            }
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

                    PacketHandler.Process(this, m_master, packet);

                    m_master.Stream?.ForceFlushToSocket();
                }
                catch (SocketException e)
                {
                    //e.ErrorCode == 10054 The remote host forcibly terminated the existing connection
                    if (e.ErrorCode == 10054)
                    {
                        //TODO Handle the exception thrown by the local socket when sending a packet to a remote closed socket
                        continue;
                    }
                    //if (e.ErrorCode != 10004 && e.ErrorCode != 4)

                    if (m_master.HandleException(e))
                    { m_master.CloseConnection(DisconnectReason.InnerException, false); }
                }
                catch (Exception e)
                {
                    if (m_master.HandleException(e))
                    { m_master.CloseConnection(DisconnectReason.UnhandledException, true); }
                }
            }
        }
            
    }
}
