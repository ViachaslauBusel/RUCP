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
                    if (packet.ACK || !packet.Client.isConnected()) { packet.ForcedDispose(); continue; }

                    //If the waiting time for confirmation of receipt of the package by the client exceeds timeout, disconnect the client
                    if (packet.CalculatePing() > 100_000)
                    {
                        Console.WriteLine($"[{m_master.GetType()}]Disconect time:{packet.CalculatePing()}, cicle:{packet.m_sendCicle} \n" +
                            $"SentPackets:{packet.Client.Network.SentPackets}, ResentPackets:{packet.Client.Network.ResentPackets}");

                        packet.Client.CloseConnection();
                    //    packet.ForcedDispose();
                        continue;
                    }
                  //  Console.WriteLine("Recend packet");
                    m_master.Socket.SendTo(packet, packet.Client.RemoteAddress);
                    packet.Client.Network.ResentPackets++;
                   // Console.WriteLine($"пакет:[{packet.Sequence}]->переотправка");

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
