using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
   
    internal sealed class CheckingConnections
    {
        private class Verification
        {
            public long time;
            public Client client;
        }

        private const long TIME_CHECK = 60_000;
        private IServer m_master;
        private volatile bool m_work = true;
        private Thread m_thread;
        private  BlockingCollection<Verification> m_list_checking = new BlockingCollection<Verification>(new ConcurrentQueue<Verification>());
        private object m_lock = new object();

        private CheckingConnections(IServer server) { m_master = server; }
       
        internal static CheckingConnections Start(IServer server)
        {

            CheckingConnections checking = new CheckingConnections(server);
            checking.m_thread = new Thread(() => checking.Run()) { IsBackground = true };
            checking.m_thread.Start();
            return checking;
        }
        internal void InsertClient(Client client)
        {
            m_list_checking.Add(new Verification()
            {
                client = client,
                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        internal void Run()
        {


            while (m_work)
            {
                try
                {
                    Verification verification = m_list_checking.Take();

                    long sleepTime = (verification.time + TIME_CHECK) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (sleepTime > 0)
                    {
                        lock (m_lock) { Monitor.Wait(m_lock, (int)sleepTime); }
                    }

                    if (verification.client.isConnected())
                    {
                      //  System.Console.WriteLine("Checking Connection: " + verification.client.isRemoteHost + "onlline:"+ verification.client.Statistic.Status + " time: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        verification.client.checkingConnection();//Можно использвать для сохранение прогресса в БД
                        CheckingConnection(verification.client);//Отправка пакета для проверки соеденения
                        verification.time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();//Время последней проверки
                        InsertClient(verification.client);//Вставка клиента в конец очереди для повторной проверки
                    }

                }
                catch (Exception e)
                {
                    m_master.CallException(e);
                }
            }
        }

        private void CheckingConnection(Client client)
        {
            Packet pack = Packet.Create(Channel.Reliable);
            pack.OpCode = 0;
            client.Send(pack);
        }

        internal void Stop()
        {
            m_work = false;
            m_list_checking.Dispose();
            lock (m_lock) { Monitor.PulseAll(m_lock); }
            m_thread.Join();
            //Console.WriteLine("CheckingConnection stop");
        }
    }
}
