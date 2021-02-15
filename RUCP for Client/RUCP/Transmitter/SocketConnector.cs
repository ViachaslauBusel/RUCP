using RUCP.Debugger;
using RUCP.Network;
using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP.Transmitter
{
    internal class SocketConnector
    {
        protected ServerSocket server;
        private Object connectionLock = new object();
        private long firstPing;


        internal SocketConnector(ServerSocket server)
        {
            this.server = server;
        }
        /// <summary>
        /// Создает новый поток для подключения и переотправки потерянных пакетов
        /// </summary>
        internal void Connect()
        {
            Packet packet = Packet.Create(Channel.Connection);

            packet.WriteFloat(ServerSocket.version);
            server.CryptographerRSA.WritePublicKey(packet);
            server.CryptographerRSA.Encrypt(packet);

            lock (connectionLock)
            {
                if (server.NetworkStatus != NetworkStatus.CLOSED) return;//Если соеденение не в закрытом состоянии возврат

                server.NetworkStatus = NetworkStatus.LISTENING;//Устанавливаем состояние в подключение
            }
                new Thread(() => Connector(packet)).Start();
            
        }

        /// <summary>
        /// Отпровляем запрос серверу на подключение
        /// </summary>
        private void Connector(Packet open_packet)
        {
            lock (connectionLock)
            {

                int max_cicle = 20;//10 сек ожидание подключения
                while (server.NetworkStatus == NetworkStatus.LISTENING) //Ожидаем подключение
                {
                    firstPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    //Отпровляем пакет
                    server.Socket.Send(open_packet);


                    Monitor.Wait(connectionLock, 500); //Ожидание пакета "подтверждение подключение" от сервера
                    if (--max_cicle < 0) server.NetworkStatus = NetworkStatus.CLOSED;

                }
            }
        }

        /// <summary>
        /// Прием ответа от сервера на открытие подключение
        /// </summary>
        internal void OpenConnection()
        {
            lock (connectionLock)
            {
                if (server.NetworkStatus == NetworkStatus.LISTENING)
                {
                    server.NetworkStatus = NetworkStatus.СONNECTED;
                    server.NetworkInfo.Ping = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - firstPing);
                    Debug.Log("Connection established");
                }
            }
        }

        internal void Stop()
        {
            lock (connectionLock)
            {
                if(server.NetworkStatus == NetworkStatus.СONNECTED)
                    Debug.Log("Closing the connection");

                server.NetworkStatus = NetworkStatus.CLOSED;
                
            }
        }
    }
}
