using RUCP.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
    internal sealed class Resender
    {
        private IServer m_master;
        private Thread m_thread;
        private volatile bool m_work = true;
     //   private BlockingQueue<Packet> m_elements = new BlockingQueue<Packet>();

        private Resender(IServer server)
        {
            m_master = server;
        }

        internal void Add(Packet packet)
        {
       //     packet.WriteSendTime(); //Время следующей переотправки пакета
         //   m_elements.Add(packet);
        }

        internal static Resender Start(IServer server)
        {
            Resender resender = new Resender(server);
            resender.m_thread = new Thread(() => resender.Run()) { IsBackground = true };
            resender.m_thread.Start();
            return resender;
        }

        internal void Run()
        {
            while (m_work)
            {
                try
                {
                    foreach (Client c in m_master.ClientList)
                    {
                      
                        if (c.Status == NetworkStatus.CLOSE_WAIT)
                        {
                            Packet disconnectCMD = c.GetDisconnectPacket();
                            //Отсылаем пакет с запросом на отключения каждые н сек в течении н секунд

                            //Время ожидания ответа от удаленого узла истекло
                            if ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - disconnectCMD.SendTime) > 3_000)
                            {
                                c.CloseConnection(DisconnectReason.TimeoutExpired);
                            }
                            else if (disconnectCMD.ResendTime >= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                            {
                                Console.WriteLine("Send disconnect CMD");
                                c.WriteInSocket(disconnectCMD);
                                disconnectCMD.WriteSendTime(c.Statistic.GetTimeoutInterval());
                            }
                        }

                        c.BufferTick();
                        c.Stream?.Flush();//TODO disconnect

                    }
                    Thread.Sleep(1);
                  //  Packet packet = m_elements.Take();


                  //  //If the first packet in the queue is confirmed or the client is disconnected, remove it from the queue and go to the next
                  //  if (packet.ACK || !packet.Client.isConnected()) { packet.ForcedDispose(); continue; }

                  //  //If the waiting time for confirmation of receipt of the package by the client exceeds timeout, disconnect the client
                  //  if (packet.CalculatePing() > m_master.Options.DisconnectTimeout)
                  //  {
                  //      Console.WriteLine($"[{m_master.GetType()}]Disconect time:{packet.CalculatePing()}, cicle:{packet.m_sendCicle} timeout:{m_master.Options.DisconnectTimeout} \n" +
                  //          $"SentPackets:{packet.Client.Statistic.SentPackets}, ResentPackets:{packet.Client.Statistic.ResentPackets}");

                  //      packet.Client.CloseConnection();
                  //  //    packet.ForcedDispose();
                  //      continue;
                  //  }
                  //  //if (packet.ResendTime != DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) Console.WriteLine($"Время повторной отправки не совпадает, разница:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.ResendTime}");
                  // packet.Client.Stream.Write(packet);
                  ////  m_master.Socket.SendTo(packet, packet.Client.RemoteAddress);

                   
                  
                  //  packet.Client.Statistic.ResentPackets++;
                  // // Console.WriteLine($"пакет:[{packet.Sequence}]->переотправка");

                  //  Add(packet); //Запись на переотправку

                }
                catch (Exception e)
                {
                   if(m_work) m_master.CallException(e);
                }

            }
        }

        internal void Stop()
        {
            m_work = false;
          //  m_elements.Dispose();
            m_thread.Join();
          //  Console.WriteLine("resender stop");
        }
    }
}
