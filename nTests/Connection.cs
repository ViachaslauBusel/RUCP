using NUnit.Framework;
using RUCP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nTests
{
    public class Connection
    {
        [Test]
        public void WithoutAnswer()
        {
            Client client = new Client();
            client.SetHandler(() => new UnityProfile());
            client.ConnectTo("127.0.0.1", 3232);

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < 10_000 && client.Status == NetworkStatus.LISTENING)
            {
                Thread.Sleep(1); 
            }

            Console.WriteLine($"Connection status:{client.Status}");
            Assert.True(client.Status == NetworkStatus.CLOSED);
        }

        [Test]
        public void CloseCheck()
        {
            Server server = new Server(3232);
            server.SetHandler(() => new UnityProfile());
            server.Start();

            Client client = new Client();
            client.SetHandler(() => new UnityProfile());
            client.ConnectTo("127.0.0.1", 3232);

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < 10_000 && client.Status == NetworkStatus.LISTENING)
            {
                Thread.Sleep(1);
            }
            Assert.True(client.Status == NetworkStatus.CONNECTED);


            client.Close();
            stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < 10_000 && client.Status != NetworkStatus.CLOSED)
            {
                Thread.Sleep(1);
            }

            Console.WriteLine($"Connection status:{client.Status}");
            Assert.True(client.Status == NetworkStatus.CLOSED);
            Assert.True(server.ClientsCount == 0);
        }

        [Test]
        public void CloseCheckByServer()
        {
            Server server = new Server(3232);
            server.SetHandler(() => new UnityProfile());
            server.Start();

            Client[] client = new Client[100];
            for(int i=0; i<client.Length; i++)
            {
                client[i] = new Client();
                client[i].SetHandler(() => new UnityProfile());
                client[i].ConnectTo("127.0.0.1", 3232);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < 10_000 && client.Any(c => c.Status != NetworkStatus.CONNECTED))
            {
                Thread.Sleep(1);
            }
            Assert.True(client.All(c => c.Status == NetworkStatus.CONNECTED));


            server.Stop();
            stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < 10_000 && client.Any(c => c.Status != NetworkStatus.CLOSED))
            {
                Thread.Sleep(1);
            }

            Assert.True(client.All(c => c.Status == NetworkStatus.CLOSED));
            Assert.True(server.ClientsCount == 0);
        }
    }
}
