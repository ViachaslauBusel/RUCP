using RUCP.BufferChannels;
using RUCP.Handler;
using RUCP.Network;
using RUCP.Packets;
using RUCP.Transmitter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RUCP
{
    public class ServerSocket
    {
        internal UdpSocket Socket{ get; private set; }
        public NetworkInfo NetworkInfo { get; private set; }
        public ServerInfo ServerkInfo { get; private set; }
        private volatile NetworkStatus networkStatus = NetworkStatus.CLOSED;
        public NetworkStatus NetworkStatus { get => networkStatus; internal set { 
                networkStatus = value;
                status?.Invoke(value);
            } }
        public event Action<NetworkStatus> status;
        internal SocketSender SocketSender { get; private set; }
        internal SocketListener SocketListener { get; private set; }
        private ConcurrentQueue<Packet> pipeline = new ConcurrentQueue<Packet>();


        internal BufferReliable bufferReliable { get; private set; }
        internal BufferQueue bufferQueue { get; private set; }
        internal BufferDiscard bufferDiscard { get; private set; }

        private int remotePort;
        private IPAddress remoteIP;

        public ServerSocket(string address, int port)
        {
            // Получаем данные, необходимые для соединения

            //удаленный IP-адрес
            remoteIP = IPAddress.Parse(address);

            remotePort = port;

            

            
        }

        internal  void AddPipeline(Packet nw)
        {
            ServerkInfo.proccesed++;
                pipeline.Enqueue(nw);
        }

      /*  private  Packet Receive()
        {
                    

            return packet;
        }*/

        public void ProcessPacket(int count)
        {
            for (int i = 0; i < count; i++)
            {
              //  pipeline.TryDequeue(out Packet packet);
                if (pipeline.TryDequeue(out Packet packet))
                    HandlersStorage.GetHandler(packet.ReadType())(packet);
                else return;
            }
        }
        

        /// <summary>
        /// Подключение к серверу
        /// </summary>
        public void Connection(Packet nw = null)
        {
             if(NetworkStatus == NetworkStatus.СONNECTED) { Debug.Log("соединение уже установлено"); return; }
            // Создаем endPoint по информации об удаленном хосте
            Socket = new UdpSocket(new IPEndPoint(remoteIP, remotePort));

            NetworkInfo = new NetworkInfo();
            ServerkInfo = new ServerInfo();

            bufferReliable = new BufferReliable(500);
            bufferQueue = new BufferQueue(this, 500);
            bufferDiscard = new BufferDiscard(500);


            //Создаем обьект для отправки пакетов
            SocketSender = new SocketSender(this);
            //Запускаем поток слушатель для прием пакетов
            SocketListener = new SocketListener(this);

            SocketSender.InitializedSender(nw);// Подключение к серверу
        }


        public void Close()
        {
            Debug.Log("Закрытие соединение");
            Packet packet = new Packet(Channel.Disconnect);
            SocketSender?.Send(packet);


            SocketSender?.Stop();
            SocketSender = null;

            SocketListener?.Stop();
            SocketListener = null;

            Socket?.Close();
            Socket = null;
        }




        public bool IsConnected()
        {
            return (NetworkStatus == NetworkStatus.СONNECTED);
        }

        /// <summary>
        /// Отпровляет данные по сети
        /// </summary>
        public void Send(Packet packet)
        {
                    SocketSender?.Send(packet);
        }

    }

}
