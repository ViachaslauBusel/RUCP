using NUnit.Framework;
using RUCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nTests
{
    internal class SomeTest
    {

        [Test]
        public void TestFlags()
        {
            Assert.True((NetworkStatus.LISTENING | NetworkStatus.CONNECTED).HasFlag(NetworkStatus.LISTENING));
            Assert.True((NetworkStatus.LISTENING | NetworkStatus.CONNECTED).HasFlag(NetworkStatus.CONNECTED));
            Assert.False((NetworkStatus.LISTENING | NetworkStatus.CONNECTED).HasFlag(NetworkStatus.CLOSED));
        }
        [Test]
        public void TestTaskPipeline()
        {
            Task task = Task.Run(() => Console.WriteLine($"task_1"));

            task.Wait();

            Thread.Sleep(1000);

            task.ContinueWith((t) => Console.WriteLine($"task_2"));
        }

        [Test]
        public void AsyncTest()
        {
            TestAsyncBody();
            Console.WriteLine("1");
            Thread.Sleep(1000);
            Console.WriteLine("2");
        }

        private async void TestAsyncBody()
        {
            await PrintAsync();   // вызов асинхронного метода
            Console.WriteLine("Некоторые действия в методе Main");


            void Print()
            {
                Thread.Sleep(3);     // имитация продолжительной работы
                Console.WriteLine("Hello METANIT.COM");
            }

            // определение асинхронного метода
            async Task PrintAsync()
            {
                Console.WriteLine("Начало метода PrintAsync"); // выполняется синхронно
                await Task.Run(() => Print());                // выполняется асинхронно
                Console.WriteLine("Конец метода PrintAsync");
            }
        }
    }
}
