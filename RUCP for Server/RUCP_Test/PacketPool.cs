using NUnit.Framework;
using RUCP;
using RUCP.Packets;
using System.Collections.Generic;

namespace RUCP_Test
{
    public class PacketPool
    {
        [SetUp]
        public void Setup()
        {
        }

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
    }
}