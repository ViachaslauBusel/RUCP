using NUnit.Framework;
using RUCPs.Packets;
using System.Collections.Generic;

namespace RUCP_Test
{
    class Packets
    {
        /*    [Test]
       public void CreatePacketNoPool()
       {
           Queue<Packet> collection = new Queue<Packet>();
           for(int i=0; i<200; i++)
           {
               collection.Enqueue(new Packet(null, Channel.Reliable));
           }

           for(int cicle = 0; cicle< 100_000; cicle++)
           {
               for (int i = 0; i < 200; i++)
               {
                   collection.Enqueue(new Packet(null, Channel.Reliable));
                   collection.Dequeue();
               }
           }
           Assert.Pass();
       }*/

        [Test]
        public void CreatePacketPool()
        {
            Queue<Packet> collection = new Queue<Packet>();
            for (int i = 0; i < 200; i++)
            {
                collection.Enqueue(Packet.Create(null, Channel.Reliable));
            }

            for (int cicle = 0; cicle < 100_000; cicle++)
            {
                for (int i = 0; i < 200; i++)
                {
                    collection.Enqueue(Packet.Create(null, Channel.Reliable));
                    collection.Dequeue().Dispose();
                }
            }

            Assert.Pass();
        }
        [Test]
        public void ChannelRW_Test()
        {
            /*  Packet packet = Packet.Create(Channel.Connection);
              Assert.IsTrue(packet.isChannel(Channel.Connection));
              Assert.IsTrue(packet.Encrypt == false);
              packet.Encrypt = true;
              Assert.IsTrue(packet.isChannel(Channel.Connection));
              Assert.IsTrue(packet.Encrypt == true);*/

            Packet packet = Packet.Create(Channel.Reliable);
            Assert.IsTrue(packet.isChannel(Channel.Reliable));
            Assert.IsTrue(packet.Encrypt == false);
            packet.Encrypt = true;
            Assert.IsTrue(packet.isChannel(Channel.Reliable));
            Assert.IsTrue(packet.Encrypt == true);

            packet = Packet.Create(Channel.Queue);
            Assert.IsTrue(packet.isChannel(Channel.Queue));
            Assert.IsTrue(packet.Encrypt == false);
            packet.Encrypt = true;
            Assert.IsTrue(packet.isChannel(Channel.Queue));
            Assert.IsTrue(packet.Encrypt == true);

            packet = Packet.Create(Channel.Discard);
            Assert.IsTrue(packet.isChannel(Channel.Discard));
            Assert.IsTrue(packet.Encrypt == false);
            packet.Encrypt = true;
            Assert.IsTrue(packet.isChannel(Channel.Discard));
            Assert.IsTrue(packet.Encrypt == true);

            packet = Packet.Create(Channel.Unreliable);
            Assert.IsTrue(packet.isChannel(Channel.Unreliable));
            Assert.IsTrue(packet.Encrypt == false);
            packet.Encrypt = true;
            Assert.IsTrue(packet.isChannel(Channel.Unreliable));
            Assert.IsTrue(packet.Encrypt == true);
        }
    }
}
