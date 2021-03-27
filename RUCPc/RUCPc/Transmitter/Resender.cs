/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.Collections;
using RUCPc.Debugger;
using RUCPc.Network;
using RUCPc.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCPc.Transmitter
{
    internal class Resender
    {

        private BlockingQueue<Packet> elements = new BlockingQueue<Packet>();
        private ServerSocket serverSocket;
        private Thread thread;
        private volatile bool work = true;

        internal Resender(ServerSocket serverSocket)
        {
            this.serverSocket = serverSocket;
        }

        internal void Add(Packet packet)
        {
            packet.WriteSendTime(serverSocket.NetworkInfo.GetTimeout());
   
            elements.Add(packet);
        }

        internal void Start()
        {
            thread = new Thread(new ThreadStart(Run)) { IsBackground = true };
            thread.Start();
        }
        private void Run()
        {
            Packet packet = null;
            while (work)
            {
                try
                {
                    packet = elements.Take();

                    if (packet == null) continue;
                    //Если первый пакеет в очереди подтвержден удаляем его из очереди и переходим к следуюещему
                    if (packet.ACK) continue;

                    //Если количество попыток переотправки пакета превышает 16, отключаем клиента
                    if (packet.SendCicle > 40)
                    {
                        Debug.Log($"Lost connection, remote node does not respond for: {packet.CalculatePing()}ms", MsgType.ERROR);
                        serverSocket.Close();
                        continue;
                    }


                    serverSocket.NetworkInfo.Resend++;
                    //     Debug.Log("Переотправка пакета");
                    serverSocket.Socket.Send(packet);
                    Add(packet); //Запись на переотправку

                }
                catch (Exception e)
                {
                    Debug.Log($"Resender:{e}");
                }

            }
        //   Debug.Log("Resender has completed its work");
        }

        internal void Stop()
        {
            work = false;
            elements.isBlocking = false;
        }
    }
}
