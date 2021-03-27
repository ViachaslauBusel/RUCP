using RUCPc.Debugger;
using RUCPc.Network;
using RUCPc.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCPc.Transmitter
{
    internal class SocketConnector
    {
        protected ServerSocket server;
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
            Thread th = new Thread(() => Connector());
            
            th.Start();
        }

        /// <summary>
        /// Отпровляем запрос серверу на подключение
        /// </summary>
        private void Connector()
        {
            Packet packet = Packet.Create(Channel.Connection);
            packet.WriteFloat(ServerSocket.version);
            server.CryptographerRSA.WritePublicKey(packet);
            server.CryptographerRSA.Encrypt(packet);

            lock (server.lockStatus)
            {
                    int max_cicle = 20;//10 сек ожидание подключения
                    firstPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    while (server.NetworkStatus == NetworkStatus.LISTENING) //Ожидаем подключение
                    {
                        if (--max_cicle < 0) { server.Close(); return; }
                        //Отпровляем пакет
                        server.Socket.Send(packet);

                        Monitor.Wait(server.lockStatus, 500); //Ожидание пакета "подтверждение подключение" от сервера
                    }
            }
        }

        /// <summary>
        /// Прием ответа от сервера на открытие подключение
        /// </summary>
        internal void OpenConnection()
        {
            lock (server.lockStatus)
            {
                if (server.NetworkStatus == NetworkStatus.LISTENING)
                {
                    server.NetworkStatus = NetworkStatus.СONNECTED;
                    server.NetworkInfo.Ping = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - firstPing);
                  //  Debug.Log($"first ping:{server.NetworkInfo.Ping}");
                }
            }
        }


    }
}
