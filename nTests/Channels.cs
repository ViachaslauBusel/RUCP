using NUnit.Framework;
using RUCP;
using RUCP.Channels;
using RUCP.Transmitter;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace nTests
{
    public class Channels
    {
        class ServerProfile : IProfile
        {
            private volatile int m_totalReadPacket = 0;
            private volatile int m_totalTypePacket = 0;
            public void ChannelRead(Packet pack)
            {
                m_totalReadPacket++;
                if (pack.ReadType() == 1)
                {
                    m_totalTypePacket++;
                    Packet packet = Packet.Create(pack.Client, Channel.Reliable);
                    packet.WriteType(1);
                    packet.WriteInt(pack.ReadInt());
                PushPacket(packet);
                   
                }
                pack.Dispose();
            }

            public void CheckingConnection()
            {

            }

            public void CloseConnection()
            {
                Console.WriteLine($"m_totalReadPacket:{m_totalReadPacket} type:{m_totalTypePacket}");
            }

            public void OpenConnection()
            {
              //  Console.WriteLine("OpenConnection");
            }
        }


        private Server m_server;
        private Client[] m_clients = new Client[8];

        private bool ForeachClients(Predicate<Client> predicate)
        {
            for (int i = 0; i < m_clients.Length; i++)
            {
                if (!predicate.Invoke(m_clients[i])) { return false; }
            }
            return true;
        }
        private static void PushPacket(Packet packet)
        {
            Stopwatch sw = Stopwatch.StartNew();    
            while (sw.Elapsed.TotalMilliseconds < 5_000)
            {
                try
                {
                    packet.Send();
                    return;
                }
                catch (BufferOverflowException)
                {
                   //  Console.WriteLine("BufferOverflowException");
                    Thread.Sleep(1);
                }
                catch (Exception e){ Console.Error.WriteLine($"UnknownException;{e}"); Assert.Fail(); return; }
            }
            throw new Exception("BufferOverflowException");
        }
        [SetUp]
        public void SetUp()
        {
            m_server?.Stop();
            m_server = new Server(3232, networkEmulator: true);
            m_server.SetHandler(() => new ServerProfile());
            m_server.Start();

            for (int i = 0; i < m_clients.Length; i++)
            {
                m_clients[i] = new Client();
                m_clients[i].SetHandler(() => new UnityProfile());
                m_clients[i].ConnectTo("127.0.0.1", 3232, networkEmulator: false);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed.TotalMilliseconds < 3_000.0)
            {
                if (ForeachClients((c) => c.Network.Status == NetworkStatus.СONNECTED))
                {
  
                    return;
                }
            }
            Assert.Fail();
        }
        [Test]
        public void TestConnection()
        {
            Assert.True(ForeachClients((c) => c.Network.Status == NetworkStatus.СONNECTED));
        }
        [Test]
        public void TestReliable_1()
        {
            int numberTest = 50_000;
            int testSum = Enumerable.Range(0, numberTest).Sum();



            Stopwatch timer = Stopwatch.StartNew();
            for (int j = 0; j < numberTest; j++)
            {
                int dataJ = j;
                // Parallel.ForEach(m_clients, (c) =>
                foreach (var c in m_clients)
                {
                    Packet packet = Packet.Create(c, Channel.Reliable);
                    packet.WriteType(1);
                    packet.WriteInt(dataJ);
                    PushPacket(packet);
                }//);   
            }
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
                else if (stopwatch.Elapsed.TotalMilliseconds > 50_000.0)
                {
                    ForeachClients((c) =>
                    {
                        if (((UnityProfile)c.Profile).AvailablePackets != numberTest)
                        {
                            Console.WriteLine($"было отправлено:{c.Network.SentPackets}, переотправлено:{c.Network.ResentPackets}");
                          //  Console.WriteLine($"AvailablePackets:{((UnityProfile)c.Profile).AvailablePackets} ClientOnline:{c.Network.Status}");
                        }
                        return true;
                    });
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
                sumResentPackets += c.Network.ResentPackets;
                  
                return true;
            });
            Console.WriteLine($"ResentPackets:{sumResentPackets}");
            //ver. 0.010
            //- 23070,5893ms
            //- 23741,4702ms
            //- 23305,5446ms
            Assert.Pass();
        }
        [Test]
        public void TestReliable_2()
        {
            if(m_clients.Length == 0) { Assert.Fail(); return; }

            int numberTest = 1_000_000;
            int testSum = 0;
            for(int i=0; i<numberTest; i++)
            {
                testSum += i;
            }
           Stopwatch timer =  Stopwatch.StartNew();
            for (int j = 0; j < numberTest; j++)
            {
             
                Packet packet = Packet.Create(m_clients[0], Channel.Reliable);
                packet.WriteType(1);
                packet.WriteInt(j);
                PushPacket(packet);
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
                        int sum = 0;
                        unityProfile.Handlers.RegisterHandler(1, (Packet) => sum += Packet.ReadInt());
                        unityProfile.ProcessPacket(numberTest);
                 //   Console.WriteLine($"test:{testSum} sum:{sum}");
                    if (sum == testSum)
                    {
                        break;
                    }
                    Assert.Fail();
                    return;
                }
                else if (stopwatch.Elapsed.TotalMilliseconds > 20_000.0)
                {
                 //   m_server.Stop();
                    Console.WriteLine($"AvailablePackets:{((UnityProfile)m_clients[0].Profile).AvailablePackets}");
                    Assert.Fail();
                    return;
                }
            }
            timer.Stop();
            Thread.Sleep(5_000);
            Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}");

            Assert.Pass();
        }
        [Test]
        public void TestQueue_1()
        {
            if (m_clients.Length == 0) { Assert.Fail(); return; }

            int numberTest = 1_000_000;

            Stopwatch timer = Stopwatch.StartNew();
            for (int j = 0; j < numberTest; j++)
            {
                while ((j - ((UnityProfile)m_clients[0].Profile).AvailablePackets) > 200) { Thread.Sleep(1); }
                Packet packet = Packet.Create(m_clients[0], Channel.Queue);
                packet.WriteType(1);
                packet.WriteInt(j);
                PushPacket(packet);
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
                Packet packet = Packet.Create(m_clients[0], Channel.Discard);
                packet.WriteType(1);
                packet.WriteInt(j);
                PushPacket(packet);
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