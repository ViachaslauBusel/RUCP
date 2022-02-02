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

        private BlockingQueue<Packet> m_elements = new BlockingQueue<Packet>();
        private Client m_serverSocket;
        private Thread m_thread;
        private volatile bool m_work = true;

        internal Resender(Client serverSocket)
        {
            this.m_serverSocket = serverSocket;
        }

        internal void Add(Packet packet)
        {
            int timeout = m_serverSocket.NetworkInfo.GetTimeoutInterval();
            packet.WriteSendTime(timeout);
         //   Debug.Log($"Время следующей отправки пакета:{packet.ReadNumber()} через:{timeout}");
   
            m_elements.Add(packet);
        }

        internal void Start()
        {
            m_thread = new Thread(new ThreadStart(Run)) { IsBackground = true };
            m_thread.Start();
        }
        private void Run()
        {
            Packet packet = null;
            while (m_work)
            {
                try
                {
                    packet = m_elements.Take();
                   

                    if (packet == null) continue;
                    //Если первый пакеет в очереди подтвержден удаляем его из очереди и переходим к следуюещему
                    if (packet.ACK) continue;

                    //Если время ожидания подтверждения получения пакета сервером превышает 6 сек, отключаемся от сервера
                    if (packet.CalculatePing() > 6000)
                    {
                        Debug.Log($"Lost connection, remote node does not respond for: {packet.CalculatePing()}ms", MsgType.ERROR);
                        m_serverSocket.Close();
                        continue;
                    }


                    m_serverSocket.NetworkInfo.ResentPackets++;
                     //    Debug.Log($"Переотправка пакета:{packet.ReadNumber()}, time:{packet.CalculatePing()}");
                    m_serverSocket.Socket?.Send(packet);
                    Add(packet); //Запись на переотправку

                }
                catch (Exception e)
                {
                    Debug.Log($"Resender:{e}", MsgType.ERROR);
                }

            }
            //   Debug.Log("Resender has completed its work");
        }

        internal void Stop()
        {
            m_work = false;
            m_elements.isBlocking = false;
        }
    }
}
