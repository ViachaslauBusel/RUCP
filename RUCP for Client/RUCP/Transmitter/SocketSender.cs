/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Network;
using RUCP.Packets;
using RUCP.Transmitter;
using System;
using System.Net.Sockets;
using System.Threading;

namespace RUCP.Transmitter
{
    
    class SocketSender
    {

        protected ServerSocket server;
        private Resender resender;

        public SocketSender(ServerSocket _client)
        {
            this.server = _client;
        }


        public void Stop()
        {
            server.NetworkStatus = NetworkStatus.CLOSED;
            resender?.Stop();
        }



        /// <summary>
        /// Создает новый поток для подключения и переотправки потерянных пакетов
        /// </summary>
        public void InitializedSender(Packet open_packet = null)
        {
            if (open_packet == null)
            {
                open_packet =  Packet.Create(Channel.Connection); //Пакет с первым байтом индификатором подключения 
            }
            else if (!open_packet.isChannel(Channel.Connection)) throw new SocketException();

            server.NetworkStatus = NetworkStatus.LISTENING;//Устанавливаем состояние в подключение

           new Thread(() => Connector(open_packet)).Start();
        }

        /// <summary>
        /// Отпровляем запрос серверу на подключение
        /// </summary>
        private void Connector(Packet open_packet)
        {

            if (server.NetworkStatus == NetworkStatus.СONNECTED) return;//Если состояние подключенно возврат

            

            int max_cicle = 20;//10 сек ожидание подключения
            while (server.NetworkStatus == NetworkStatus.LISTENING) //Ожидаем подключение
            {
                   
                    //Отпровляем пакет
                    server.Socket.Send(open_packet);

             
                    Thread.Sleep(500); //Ожидание пакета "подтверждение подключение" от сервера
                    max_cicle--;
                if (max_cicle < 0) server.NetworkStatus = NetworkStatus.CLOSED;

            }
        }

        /// <summary>
        /// Прием ответа от сервера на открытие подключение
        /// </summary>
        internal void OpenConnection()
        {
            resender = new Resender(server);
            resender.Start();
            if (server.NetworkStatus == NetworkStatus.LISTENING)
                server.NetworkStatus = NetworkStatus.СONNECTED;
        }


        /// <summary>
        /// Отправка и вставка пакетов в очередь на переотправку
        /// </summary>
        internal void Send(Packet packet)
        {
           if(packet.sendCicle != 0) { Debug.Log("Пакет заблокирован, отправка невозможна"); return; }

            switch (packet.ReadChannel())
            {
                case Channel.Reliable:
                    
                    server.bufferReliable.Insert(packet);
                    server.NetworkInfo.Send++;
                    resender.Add(packet);//Запись на переотправку при потери пакетов
                    break;
                case Channel.Queue:
                   
                    server.bufferQueue.Insert(packet);
                    server.NetworkInfo.Send++;
                    resender.Add(packet);//Запись на переотправку при потери пакетов
                    break;
                case Channel.Discard:
                   
                    server.bufferDiscard.Insert(packet);
                    server.NetworkInfo.Send++;
                    resender.Add(packet);//Запись на переотправку при потери пакетов
                    break;
            }
            //Отпровляем пакет
                server.Socket.Send(packet);
        }
    }
}
