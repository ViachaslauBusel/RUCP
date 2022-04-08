using RUCP.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
    internal class Resender
    {
        private IServer m_master;
        private BlockingQueue<Packet> m_elements = new BlockingQueue<Packet>();

        private Resender(IServer server)
        {
            m_master = server;
        }

        internal void Add(Packet packet)
        {
            packet.WriteSendTime(); //Время следующей переотправки пакета
            m_elements.Add(packet);
        }

        internal static Resender Start(IServer server)
        {
            Resender resender = new Resender(server);
            new Thread(() => resender.Run()) { IsBackground = true }.Start();
            return resender;
        }

        internal void Run()
        {
            while (true)
            {
                try
                {
                    Packet packet = m_elements.Take();


                    //If the first packet in the queue is confirmed or the client is disconnected, remove it from the queue and go to the next
                    if (packet.ACK || !packet.Client.isConnected()) { packet.Dispose(); continue; }

                    //If the waiting time for confirmation of receipt of the package by the client exceeds 6 seconds, disconnect the client
                    if (packet.CalculatePing() > 10_000)
                    {
                        Console.WriteLine($"[{m_master.GetType()}]Disconect:{packet.m_sendCicle}");
                        //  Log.Warn($"Lost connection, remote node does not respond for: {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.SendTime}ms " +
                        //           $"timeout:{packet.Client.GetTimeout()}ms");

                        packet.Client.CloseConnection();
                        packet.Dispose();
                        continue;
                    }

                    m_master.Socket.SendTo(packet, packet.Client.RemoteAdress);
                    Add(packet); //Запись на переотправку

                }
                catch (Exception e)
                {
                    m_master.CallException(e);
                }

            }
        }
    }
}
