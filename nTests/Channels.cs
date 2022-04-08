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
            public void ChannelRead(Packet pack)
            {
                if (pack.ReadType() == 1)
                {
                    Packet packet = Packet.Create(pack.Client, Channel.Reliable);
                    packet.WriteType(1);
                    packet.WriteInt(pack.ReadInt());
                    packet.Send();
                }
            }

            public void CheckingConnection()
            {

            }

            public void CloseConnection()
            {

            }

            public void OpenConnection()
            {

            }
        }


        private Server m_server;
        private Client[] m_clients = new Client[512];

        private bool InterrogateClients(Predicate<Client> predicate)
        {
            for (int i = 0; i < m_clients.Length; i++)
            {
                if (!predicate.Invoke(m_clients[i])) { return false; }
            }
            return true;
        }
        [SetUp]
        public void SetUp()
        {
            m_server = new Server(3232, networkEmulator: true);
            m_server.SetHandler(() => new ServerProfile());
            m_server.Start();

            for (int i = 0; i < m_clients.Length; i++)
            {
                m_clients[i] = new Client();
                m_clients[i].SetProfile(new UnityProfile());
                m_clients[i].ConnectTo("127.0.0.1", 3232, networkEmulator: true);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed.TotalMilliseconds < 3_000.0)
            {
                if (InterrogateClients((c) => c.Network.Status == NetworkStatus.СONNECTED))
                {
  
                    return;
                }
            }
            Assert.Fail();
        }
        [Test]
        public void TestConnection()
        {
            Assert.True(InterrogateClients((c) => c.Network.Status == NetworkStatus.СONNECTED));
        }
        [Test]
        public void TestReliable_1()
        {
            int numberTest = 550;
            int testSum = Enumerable.Range(0, numberTest).Sum();


            Action<Client, int> action = (c, j) =>
             {
                 Packet packet = Packet.Create(c, Channel.Reliable);
                 packet.WriteType(1);
                 packet.WriteInt(j);
                 packet.Send();
             };

            Stopwatch timer = Stopwatch.StartNew();
            for (int j = 0; j < numberTest; j++)
            {
                Parallel.ForEach(m_clients, (c) => action(c, j));   
            }
            double sendTime = timer.Elapsed.TotalMilliseconds;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //Подождать покуда все отправленные пакеты не будут перенаправленны обратно
                if (InterrogateClients((c) => ((UnityProfile)c.Profile).AvailablePackets == numberTest))
                {
                   // Console.WriteLine("SUC");
                    //Пересчитать суму всех принятых чисел, должно совпадать с суммой отправляемых
                    if (!InterrogateClients((c) =>
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
                if (stopwatch.Elapsed.TotalMilliseconds > 10_000.0)
                {
                    InterrogateClients((c) =>
                    {
                        if (((UnityProfile)c.Profile).AvailablePackets != numberTest)
                        {
                            Console.WriteLine($"AvailablePackets:{((UnityProfile)c.Profile).AvailablePackets} ClientOnline:{c.Network.Status}");
                        }
                        return true;
                    });
                    Assert.Fail();
                    return;
                }
            }
            timer.Stop();
            Console.WriteLine($"Send time:{sendTime}. Total time:{timer.Elapsed.TotalMilliseconds}ms");

            int sumResentPackets = 0;
            InterrogateClients((c) =>
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
                while (true)
                {
                    try
                    {
                        packet.Send();
                        break;
                    }
                    catch (BufferOverflowException)
                    {
                        Thread.Sleep(1);
                    }
                    catch { Assert.Fail(); return; }
                }
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
                if (stopwatch.Elapsed.TotalMilliseconds > 20_000.0)
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
                packet.Send();
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
                if (stopwatch.Elapsed.TotalMilliseconds > 20_000.0)
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
                while (true)
                {
                    try
                    {
                        packet.Send();
                        break;
                    }
                    catch (BufferOverflowException)
                    {
                        Thread.Sleep(1);
                    }
                    catch { Assert.Fail(); return; }
                }
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