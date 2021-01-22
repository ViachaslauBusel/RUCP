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



            new Thread(() => Connector(packet)).Start();

        }

        /// <summary>
        /// Отпровляем запрос серверу на подключение
        /// </summary>
        private void Connector(Packet open_packet)
        {
            lock (connectionLock)
            {
                if (server.NetworkStatus != NetworkStatus.CLOSED) return;//Если соеденение не в закрытом состоянии возврат

                server.NetworkStatus = NetworkStatus.LISTENING;//Устанавливаем состояние в подключение


                int max_cicle = 20;//10 сек ожидание подключения
                while (server.NetworkStatus == NetworkStatus.LISTENING) //Ожидаем подключение
                {

                    //Отпровляем пакет
                    server.Socket.Send(open_packet);


                    Monitor.Wait(connectionLock, 500); //Ожидание пакета "подтверждение подключение" от сервера
                    max_cicle--;
                    if (max_cicle < 0) server.NetworkStatus = NetworkStatus.CLOSED;

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
                    server.NetworkStatus = NetworkStatus.СONNECTED;
            }
        }

        internal void Stop()
        {
            lock (connectionLock)
            {
                server.NetworkStatus = NetworkStatus.CLOSED;
            }
        }
    }
}
