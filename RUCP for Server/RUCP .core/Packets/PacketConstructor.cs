using RUCP.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCP.Packets
{
   public partial class Packet
    {
		private static ConcurrentBag<Packet> packets = new ConcurrentBag<Packet>();
		private void Reset()
        {
			address = null;
			ack = false;
			sendCicle = 0;
			WriteType(0);
			Length = index = headerLength;
		}
		public void Dispose()
        {
			packets.Add(this);
        }
		public static Packet Create(int channel) => Create(null, channel);
		public static Packet Create(ClientSocket client, int channel)
		{
			if (packets.TryTake(out Packet packet))
			{
				packet.Reset();
				packet.Client = client;
				packet.Data[0] = (byte)channel;
				return packet;
			}
			return new Packet(client, channel);
		}
		private Packet(ClientSocket client, int channel)
		{

			this.Client = client;
			Data[0] = (byte)channel;
			Reset();
		}


		public static Packet Create(ClientSocket client, Packet copy_packet)
		{
			if (packets.TryTake(out Packet packet))
			{
				packet.Reset();
				packet.Client = client;
				Array.Copy(copy_packet.Data, 0, packet.Data, 0, copy_packet.Length);
				return packet;
			}
			return new Packet(client, copy_packet);
		}
		private Packet(ClientSocket client, Packet packet)
		{

			this.Client = client;
			Array.Copy(packet.Data, 0, this.Data, 0, packet.Data.Length);

			index = packet.index;
			Length = packet.Length;
		}

		internal static Packet Create()
		{
			if (packets.TryTake(out Packet packet))
			{
				packet.Reset();
				packet.sendCicle = 1;
				packet.index = headerLength;
				return packet;
			}
			return new Packet();
		}
		private Packet()
		{
			sendCicle = 1;
			index = headerLength;
		}
	}
}
