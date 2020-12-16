using RUCP.Collections;
using RUCP.Network;
using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
    internal class Resender
    {

        private DelayQueue<Packet> elements = new DelayQueue<Packet>();
        private ServerSocket serverSocket;
        private Thread thread;

        internal Resender(ServerSocket serverSocket)
        {
            this.serverSocket = serverSocket;
        }

        internal void Add(Packet packet)
        {
            packet.sendCicle++;
            packet.WriteSendTime(serverSocket.NetworkInfo.GetTimeout());
            elements.Add(packet);
        }

        internal void Start()
        {
            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }
        internal void Run()
        {
            Packet packet = null;
            while (true)
            {
                try
                {
                    packet = elements.Take();

                    //Если первый пакеет в очереди подтвержден удаляем его из очереди и переходим к следуюещему
                    if (packet.isAck()) continue;
                   
                    //Если количество попыток переотправки пакета превышает 16, отключаем клиента
                    if (packet.sendCicle > 40)
                    {
                        Debug.Log("Close Connection time: " + DateTimeOffset.UtcNow);
                        Debug.Log("Время прошедшее с момента первой отправки пакета: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.sendTime));
                        serverSocket.Close();
                        continue;
                    }
                   // Debug.Log($"переотправка через: {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.sendTime}");
                    serverSocket.NetworkInfo.Resend++;
               //     Debug.Log("Переотправка пакета");
                    serverSocket.Socket.Send(packet);
                    Add(packet); //Запись на переотправку

                }
                catch(ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log($"sendCicle: {packet.sendCicle} timeOut: {serverSocket.NetworkInfo.GetTimeout()} delay:{packet.GetDelay()}");
                 Debug.logError(GetType().Name, e.Message, e.StackTrace);
                }

            }
        }

        internal void Stop()
        {
            thread.Abort();
        }
    }
}
