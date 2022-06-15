using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
   
    internal class CheckingConnections
    {
        private class Verification
        {
            public long time;
            public Client client;
        }

        private const long TIME_CHECK = 10_000;
        private IServer m_master;
        private  BlockingCollection<Verification> m_list_checking = new BlockingCollection<Verification>(new ConcurrentQueue<Verification>());

        private CheckingConnections(IServer server) { m_master = server; }
       
        internal static CheckingConnections Start(IServer server)
        {

            CheckingConnections checking = new CheckingConnections(server);
            new Thread(() => checking.Run()) { IsBackground = true };
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


            while (true)
            {
                try
                {
                    Verification verification = m_list_checking.Take();

                    long sleepTime = (verification.time + TIME_CHECK) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (sleepTime > 0) Thread.Sleep((int)sleepTime);

                    if (verification.client.isConnected())
                    {
                        //  System.Console.WriteLine("Checking Connection: " + client.Address.ToString() + " time: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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
            Packet pack = Packet.Create(client, Channel.Reliable);
            pack.WriteType(0);
            pack.SendImmediately();
        }
    }
}
