using NUnit.Framework;
using RUCP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nTests
{
    public class PacketCreator
    {
       // [Test]
       // public void CreateNew() 
       // {
       ////     ConcurrentBag<double> vs = new ConcurrentBag<double>();
       //     Func<double> action = () =>
       //     {
       //         Stopwatch stopwatch = Stopwatch.StartNew();
       //         for(int i = 0; i < 1000; i++)
       //         {
       //             Packet packet = Packet.Create(null, Channel.Reliable);
       //         }
       //         stopwatch.Stop();
       //         return stopwatch.Elapsed.TotalMilliseconds;
       //     };

       //     for(int i = 0;i < 10000; i++)
       //     {
       //         Task[] tasks = new Task[6];
       //         tasks[0] = Task.Factory.StartNew(() => action());
       //         tasks[1] = Task.Factory.StartNew(() => action());
       //         tasks[2] = Task.Factory.StartNew(() => action());
       //         tasks[3] = Task.Factory.StartNew(() => action());
       //         tasks[4] = Task.Factory.StartNew(() => action());
       //         tasks[5] = Task.Factory.StartNew(() => action());
       //         Task.WaitAll(tasks);
       //         GC.Collect();
       //         GC.WaitForPendingFinalizers();
       //     }
       //    // Console.WriteLine($"Result:{vs.Sum() / (double)vs.Count}");
       // }

       // [Test]
       // public void CreateFromPacketPool()
       // {
       //    // ConcurrentBag<double> vs = new ConcurrentBag<double>();
       //     Func<double> action = () =>
       //     {
       //         Stopwatch stopwatch = Stopwatch.StartNew();
       //         for (int i = 0; i < 1000; i++)
       //         {
       //             Packet packet = Packet.Create(null, Channel.Reliable);
       //             packet.Dispose();
       //         }
       //         stopwatch.Stop();
       //         return stopwatch.Elapsed.TotalMilliseconds;
       //     };

       //     for (int i = 0; i < 10000; i++)
       //     {
       //         Task[] tasks = new Task[6];
       //         tasks[0] = Task.Factory.StartNew(() => action());
       //         tasks[1] = Task.Factory.StartNew(() => action());
       //         tasks[2] = Task.Factory.StartNew(() => action());
       //         tasks[3] = Task.Factory.StartNew(() => action());
       //         tasks[4] = Task.Factory.StartNew(() => action());
       //         tasks[5] = Task.Factory.StartNew(() => action());
       //         Task.WaitAll(tasks);
       //         GC.Collect();
       //         GC.WaitForPendingFinalizers();
       //     }
       //  //   Console.WriteLine($"Result:{vs.Sum() / (double)vs.Count}");
       // }

       // [Test]
       // public void CreateFromPool()
       // {
           
       //  ////   ConcurrentBag<double> vs = new ConcurrentBag<double>();
       //  //   Packet headPacket = null;
       //  //   Object locker = new object();
       //  //   Func<double> action = () =>
       //  //   {
       //  //       Stopwatch stopwatch = Stopwatch.StartNew();
       //  //       for (int i = 0; i < 1000; i++)
       //  //       {
       //  //           Packet packet = null;
       //  //           lock (locker)
       //  //           {
       //  //               packet = headPacket;
       //  //               if (packet != null) headPacket = headPacket.Next;
       //  //           }
       //  //           if(packet == null) packet = Packet.Create(Channel.Reliable);
       //  //           lock (locker)
       //  //           {
       //  //               packet.Next = headPacket;
       //  //               headPacket = packet;
       //  //               headPacket.Reset();
       //  //           }
       //  //       }
       //  //       stopwatch.Stop();
       //  //       return stopwatch.Elapsed.TotalMilliseconds;
       //  //   };

       //  //   for (int i = 0; i < 10000; i++)
       //  //   {
       //  //       Task[] tasks = new Task[6];
       //  //       tasks[0] = Task.Factory.StartNew(() => action());
       //  //       tasks[1] = Task.Factory.StartNew(() => action());
       //  //       tasks[2] = Task.Factory.StartNew(() => action());
       //  //       tasks[3] = Task.Factory.StartNew(() => action());
       //  //       tasks[4] = Task.Factory.StartNew(() => action());
       //  //       tasks[5] = Task.Factory.StartNew(() => action());
       //  //       Task.WaitAll(tasks);
       //  //       GC.Collect();
       //  //       GC.WaitForPendingFinalizers();
       //  //   }
       //    // Console.WriteLine($"Result:{vs.Sum() / (double)vs.Count}");
       // }
    }
}
