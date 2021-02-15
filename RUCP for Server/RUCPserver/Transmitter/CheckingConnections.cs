/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Client;
using RUCP.Debugger;
using RUCP.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
    internal class CheckingConnections
    {
        private static CheckingConnections instance = null;
        private readonly long time_check = 1 * 60_000;
        private static ConcurrentQueue<ClientSocket> list_checking = new ConcurrentQueue<ClientSocket>();


        internal static void InsertClient(ClientSocket client)
        {
            list_checking.Enqueue(client);
        }

        internal static void Start()
        {
            if(instance == null)
            {
                lock (list_checking)
                {
                    instance = new CheckingConnections();
                    new Thread(new ThreadStart(instance.Run)) { IsBackground = true };
                }
            }
        }

        internal void Run()
        {
            ClientSocket client = null;
            long sleep;

            while (true)
            {
                try
                {
                    if (client != null && client.isConnected())
                    {
                        System.Console.WriteLine("Checking Connection: " + client.Address.ToString() + " time: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        client.checkingConnection();//Можно использвать для сохранение прогресса в БД
                        CheckingConnection(client);//Отправка пакета для проверки соеденения
                        client.CheckingTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();//Время последней проверки
                        list_checking.Enqueue(client);//Вставка клиента в конец очереди для повторной проверки

                        client = null;

                    }
                    sleep = time_check;
                    //Поиск следующего клиента для проверки
                    while (list_checking.TryDequeue(out client))
                    {
                        if (!client.isConnected())
                        {
                            client = null;
                            continue;
                        }

                        sleep = (client.CheckingTime + time_check) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        break;
                    }

                    if (sleep > 0) Thread.Sleep((int)sleep);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        private void CheckingConnection(ClientSocket client)
        {
            Packet pack = Packet.Create(client, Channel.Reliable);
            pack.WriteType(0);
            pack.Send();
        }
    }
}
