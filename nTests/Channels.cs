using NUnit.Framework;
using RUCP;
using RUCP.Channels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace nTests
{
    public class Channels
    {
        class ServerProfile : BaseProfile
        {

            private volatile int m_totalReadPacket = 0;


            public override void ChannelRead(Packet pack)
            {

                if (pack.OpCode == 1)
                {
                    m_totalReadPacket++;
                    Packet packet = Packet.Create(Channel.Reliable);
                    packet.OpCode = 1;
                    packet.WriteInt(pack.ReadInt());
                    PushPacket(Client, packet);

                }
                //   ThreadCount.TryAdd(Thread.CurrentThread.ManagedThreadId, 0);
                // Console.WriteLine($"peer:{pack.Client.ID}, th:{Thread.CurrentThread.ManagedThreadId}");
                pack.Dispose();
            }

            public override void CheckingConnection()
            {
            
            }


            public override void CloseConnection(DisconnectReason reason)
            {

                    Console.WriteLine($"Server -> m_totalReadPacket:{m_totalReadPacket}, {Client.Statistic.ToString()}");
                    Console.WriteLine($"Server -> Close reason:{reason}");
            }

            public override bool HandleException(Exception exception)
            {
                Console.WriteLine($"Server: Exception caught:{exception}");
                return true;
            }

            public override void OpenConnection()
            {
                Console.WriteLine($"Owner:{Client != null}");
            }
        }


        private Server m_server;
        private Client[] m_clients = new Client[2];

        private bool ForeachClients(Predicate<Client> predicate)
        {
            for (int i = 0; i < m_clients.Length; i++)
            {
                if (!predicate.Invoke(m_clients[i])) { return false; }
            }
            return true;
        }
        private static void PushPacket(Client client, Packet packet)
       {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                try
                {

                    client.Send(packet);
                    return;
                }
                catch (BufferOverflowException er)
                {

                    if (sw.Elapsed.TotalMilliseconds > 5_000)
                    {

                        Console.WriteLine($"[{(client.isRemoteHost ? "Client":"Server")}]Не удалось отправить пакет");
                        throw new Exception("End");
                    }
                    //  Console.WriteLine("BufferOverflowException");
                    Thread.Sleep(3);
                }

            }
            //    try
            //    {
            //        packet.Send();
            //    }
            //    catch (BufferOverflowException e)
            //    {
            //     //   if (packet.Client.isRemoteHost) Thread.Sleep(1);
            //       // Task.Factory.StartNew(() =>
            //     //   {

            //     //   });
            //    }

        }
        [SetUp]
        public void SetUp()
        {
            m_server?.Stop();
            m_server = new Server(3232);
            m_server.SetHandler(() => new ServerProfile());
            m_server.throwingExceptions += (e) => Console.Error.WriteLine(e);
            m_server.Start(new ServerOptions()
            {
                SendTimeout = 5_000,
                MaxParallelism = 16,
                DisconnectTimeout = 6_000
            });

            for (int i = 0; i < m_clients.Length; i++)
            {
                m_clients[i] = new Client();
                m_clients[i].SetHandler(() => new UnityProfile());
                m_clients[i].ConnectTo("127.0.0.1", 3232, new ServerOptions()
                {
                    SendTimeout=5_000
                });
            }
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed.TotalMilliseconds < 3_000.0)
            {
                if (ForeachClients((c) => c.Status == NetworkStatus.CONNECTED))
                {
  
                    return;
                }
            }
            Array.ForEach(m_clients, (c) => Console.WriteLine($"Connection status: {c.Status}"));
            Assert.Fail();
        }
        [Test]
        public void TestConnection()
        {
            Assert.True(ForeachClients((c) => c.Status == NetworkStatus.CONNECTED));
            Assert.True(ForeachClients((c) => c.Profile.Client != null));
            Assert.True(ForeachClients((c) => { c.Close(); return true; }));
            m_server.Stop();
        }
        [Test]
        public void TestReliable_1()
        {
            int numberTest = 6_250;
            int testSum = Enumerable.Range(0, numberTest).Sum();


            Stopwatch timer = Stopwatch.StartNew();
          //  int sleepJ = 10;
            for (int j = 0; j < numberTest; j++)
            {
                int dataJ = j;
                // Parallel.ForEach(m_clients, (c) =>
                foreach (var c in m_clients)
                {
                    Packet packet = Packet.Create(Channel.Reliable);
                    packet.OpCode = 1;
                    packet.WriteInt(dataJ);
                    PushPacket(c, packet);
                }//);   
                //if (dataJ % sleepJ == 0)
                //{
                //    Thread.Sleep(1);
                //    sleepJ += 10;
                //}
;            }
            foreach (var c in m_clients) { c.Stream.ForceFlushToSocket(); }
                double sendTime = timer.Elapsed.TotalMilliseconds;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //Подождать покуда все отправленные пакеты не будут перенаправленны обратно
                if (ForeachClients((c) => ((UnityProfile)c.Profile).AvailablePackets == numberTest))
                {
                   // Console.WriteLine("SUC");
                    //Пересчитать суму всех принятых чисел, должно совпадать с суммой отправляемых
                    if (!ForeachClients((c) =>
                    {
                        UnityProfile unityProfile = (UnityProfile)c.Profile;
                        int sum = 0;
                        unityProfile.Handlers.RegisterHandler(1, (Packet) => sum += Packet.ReadInt());
                        unityProfile.ProcessPacket(numberTest);
                        return sum == testSum;
                    }))
                    {
                        Assert.Fail();
                        return;
                    }
                    break;
                }
                else if (stopwatch.Elapsed.TotalMilliseconds > 5_000.0)
                {
                    //foreach(Client c in m_clients)
                    //{
                    //    if (((UnityProfile)c.Profile).AvailablePackets != numberTest)
                    //    {
                    //        Console.WriteLine($"было отправлено:{c.Statistic.SentPackets}, переотправлено:{c.Statistic.ResentPackets}");
                    //        //  Console.WriteLine($"AvailablePackets:{((UnityProfile)c.Profile).AvailablePackets} ClientOnline:{c.Network.Status}");
                    //    }
                    //}
                    m_server.Stop();
                    Assert.Fail();
                    return;
                }
            }
            timer.Stop();
            Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}ms");

            int sumResentPackets = 0;
            ForeachClients((c) =>
            {
                sumResentPackets += c.Statistic.ResentPackets;
                  
                return true;
            });
            m_server.Stop();
            Assert.True(ForeachClients((c) => { c.Close(); return true; }));
            Assert.True(ForeachClients((c) => { c.Dispose(); return true; }));

            Assert.Pass();
        }
        [Test]
        public void TestReliable_2()
        {
            if(m_clients.Length == 0) { Assert.Fail(); return; }

            int numberTest = 1_000_000;// _000;
            int testSum = 0;
            for(int i=0; i<numberTest; i++)
            {
                testSum += i;
            }
           Stopwatch timer =  Stopwatch.StartNew();
            try {
                for (int j = 0; j < numberTest; j++)
                {

                    Packet packet = Packet.Create(Channel.Reliable);
                    packet.OpCode = 1;
                    packet.WriteInt(j);
                    PushPacket(m_clients[0], packet);

                  
                }
            }
            catch
            {
                Console.WriteLine($"Client -> {m_clients[0].Statistic.ToString()}");
                m_server.Stop();
                Assert.Fail();
            }
          //  m_clients[0].Stream.Flush();
            double sendTime = timer.Elapsed.TotalMilliseconds; 
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //Подождать покуда все отправленные пакеты не будет перенаправленны обратно
                if (((UnityProfile)m_clients[0].Profile).AvailablePackets == numberTest)
                {
                  //  Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets} numberTest:{numberTest}");
                    //Пересчитать суму всех принятых чисел, должно совпадать с суммой отправляемых

                        UnityProfile unityProfile = (UnityProfile)m_clients[0].Profile;
                        int sum = 0;
                        unityProfile.Handlers.RegisterHandler(1, (Packet) => sum += Packet.ReadInt());
                        unityProfile.ProcessPacket(numberTest);
                 //   Console.WriteLine($"test:{testSum} sum:{sum}");
                    if (sum == testSum)
                    {
                        Console.WriteLine($"Verification of transmitted data was successful -> {sum}");
                        break;
                    }
                    Assert.Fail();
                    return;
                }
                else if (stopwatch.Elapsed.TotalMilliseconds > 5_000.0)
                {
                 //   m_server.Stop();
                    Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets}");
                    Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}");
                    Console.WriteLine($"Client -> {m_clients[0].Statistic.ToString()}");

                    m_server.Stop();
                    Assert.Fail();
                    return;
                }
            }
            timer.Stop();

            Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}");
            Console.WriteLine($"Client -> {m_clients[0].Statistic.ToString()}");

            m_server.Stop();

            Assert.Pass();
        }
        [Test]
        public void TestQueue_1()
        {
            if (m_clients.Length == 0) { Assert.Fail(); return; }

            int numberTest = 1_000;// _000;

            Stopwatch timer = Stopwatch.StartNew();
            for (int j = 0; j < numberTest; j++)
            {
                while ((j - ((UnityProfile)m_clients[0].Profile).AvailablePackets) > 200) { Thread.Sleep(1); }
                Packet packet = Packet.Create(Channel.Queue);
                packet.OpCode = 1;
                packet.WriteInt(j);
                PushPacket(m_clients[0], packet);
            }
            double sendTime = timer.Elapsed.TotalMilliseconds;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //Подождать покуда все отправленные пакеты не будет перенаправленны обратно
                if (((UnityProfile)m_clients[0].Profile).AvailablePackets == numberTest)
                {
                    //  Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets} numberTest:{numberTest}");
                    //Пересчитать суму всех принятых чисел, должно совпадать с суммой отправляемых

                    UnityProfile unityProfile = (UnityProfile)m_clients[0].Profile;
                    int prevNum = -1;
                    bool rise = true;
                    unityProfile.Handlers.RegisterHandler(1, (Packet) =>
                    {
                        if (!rise) return;
                        int num = Packet.ReadInt();
                        rise = (num - prevNum) == 1;
                        prevNum = num;
                    });
                    unityProfile.ProcessPacket(numberTest);
                    //   Console.WriteLine($"test:{testSum} sum:{sum}");
                    if (rise)
                    {
                        break;
                    }
                    Assert.Fail();
                    return;
                }
                else if (stopwatch.Elapsed.TotalMilliseconds > 40_000.0)
                {
                    Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets}");
                    Assert.Fail();
                    return;
                }
            }
            timer.Stop();
            Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}");

            Assert.Pass();
        }

        [Test]
        public void TestDiscard_1()
        {
            if (m_clients.Length == 0) { Assert.Fail(); return; }

            int numberTest = 1_000_000;

            Stopwatch timer = Stopwatch.StartNew();
            for (int j = 0; j <= numberTest; j++)
            {
                Packet packet = Packet.Create(Channel.Discard);
                packet.OpCode = 1;
                packet.WriteInt(j);
                PushPacket(m_clients[0], packet);
            }
            double sendTime = timer.Elapsed.TotalMilliseconds;
            Stopwatch stopwatch = Stopwatch.StartNew();
    
                //Подождать покуда все отправленные пакеты не будет перенаправленны обратно
             
                    //  Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets} numberTest:{numberTest}");
                    //Пересчитать суму всех принятых чисел, должно совпадать с суммой отправляемых

                    UnityProfile unityProfile = (UnityProfile)m_clients[0].Profile;
                    int prevNum = -1;
                    bool rise = true;
                    unityProfile.Handlers.RegisterHandler(1, (Packet) =>
                    {
                        if (!rise) return;
                        int num = Packet.ReadInt();
                        rise = (num - prevNum) > 0;
                        prevNum = num;
                    });
            while (true)
            {
                unityProfile.ProcessPacket(numberTest);
                //   Console.WriteLine($"test:{testSum} sum:{sum}");
                if (prevNum == numberTest || !rise)
                {
                    if (rise)
                    {
                        break;
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
                if (stopwatch.Elapsed.TotalMilliseconds > 10_000.0)
                {
                    Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets}");
                    Assert.Fail();
                    return;
                }
            }
               
            
            timer.Stop();
            Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}");

            Assert.Pass();
        }
    }
}