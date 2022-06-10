using NUnit.Framework;
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
        public void TestTaskPipeline()
        {
            Task task = Task.Run(() => Console.WriteLine($"task_1"));

            task.Wait();

            Thread.Sleep(1000);

            task.ContinueWith((t) => Console.WriteLine($"task_2"));
        }
    }
}
