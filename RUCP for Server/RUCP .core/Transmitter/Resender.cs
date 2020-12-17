/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Collections;
using RUCP.Debugger;
using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
    internal class Resender
    {
        private static Resender instance = null;

        private static DelayQueue<Packet> elements = new DelayQueue<Packet>();


        internal static void Add(Packet packet)
        {
            packet.sendCicle++;
            packet.WriteSendTime(); //Время следующей переотправки пакета
            elements.Add(packet);
        }

        internal static void Start()
        {
            if (instance == null)
            {
                lock (elements)
                {
                    instance = new Resender();
                    new Thread(() => instance.Run()) { IsBackground = false }.Start();
                }
            }
        }
        internal void Run()
        {
            while (true)
            {
                try
                {
                    Packet packet = elements.Take();

                    //Если первый пакеет в очереди подтвержден удаляем его из очереди и переходим к следуюещему
                    if (packet.isAck() || !packet.Client.isConnected()) { packet.Dispose(); continue; }//Или клиент отключен

                    //Если количество попыток переотправки пакета превышает 20, отключаем клиента
                    if (packet.sendCicle > 20)
                    {
                        Console.WriteLine(packet.Client.ID + ": Client Close Connection: " + packet.Client.Address + " time: " + DateTimeOffset.UtcNow);
                        Console.WriteLine("Packet is closed, type: " + packet.ReadType() + " channel: "+ packet.ReadChannel() +" number: " + packet.ReadNumber());
                        Console.WriteLine("Время прошедшее с момента первой отправки пакета: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.sendTime) 
                            +" timeOut: "+packet.Client.GetTimeout() 
                            + " ping: "+packet.Client.Ping);
                        packet.Client.CloseConnection();
                        packet.Dispose();
                        continue;
                    }

                    UdpSocket.Send(packet);
                    Resender.Add(packet); //Запись на переотправку

                }
                catch (Exception e)
                {
                    Debug.logError(GetType().Name, e.Message, e.StackTrace);
                }

            }
        }
    }
}
