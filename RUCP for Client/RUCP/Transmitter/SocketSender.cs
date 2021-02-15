/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Cryptography;
using RUCP.Debugger;
using RUCP.Network;
using RUCP.Packets;
using RUCP.Transmitter;
using System;
using System.Net.Sockets;
using System.Threading;

namespace RUCP.Transmitter
{
    
    internal class SocketSender
    {

        protected ServerSocket server;
        private Resender resender;


        internal SocketSender(ServerSocket server)
        {
            this.server = server;
            resender = new Resender(this.server);
            resender.Start();
        }


        /// <summary>
        /// Отправка и вставка пакетов в очередь на переотправку
        /// </summary>
        internal void Send(Packet packet)
        {
           if(packet.isBlock) { Debug.Log("Package is blocked, sending is not possible", MsgType.ERROR); return; }
           if(packet.Encrypt) server.CryptographerAES.Encrypt(packet);
            switch (packet.Channel)
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

        internal void Stop()
        {
            resender?.Stop();
        }
    }
}
