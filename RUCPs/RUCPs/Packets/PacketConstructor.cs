/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCPs.Packets
{
	public enum DataSize
    {
		Normal,
		Large
    }
   public partial class Packet
    {
		private static ConcurrentBag<Packet> m_packetsBuffer = new ConcurrentBag<Packet>();
		private static ConcurrentBag<Packet> m_largePacketsBuffer = new ConcurrentBag<Packet>();

		private Packet(ClientSocket client, int channel, DataSize dataSize)
		{
			int size = dataSize switch
			{
				DataSize.Large => LARGE_DATA_SIZE,
				_ => DATA_SIZE
			};
			Data = new byte[size];
			Reset();
			this.Client = client;
			this.Channel = channel;
		
		}
		private Packet(DataSize dataSize)
		{
			int size = dataSize switch
			{
				DataSize.Large => LARGE_DATA_SIZE,
				_ => DATA_SIZE
			};
			Data = new byte[size];
			m_sendCicle = 1;
			Length = index = HEADER_SIZE;
		}
		private Packet(ClientSocket client, Packet copy_packet)
		{
			Data = new byte[copy_packet.Data.Length];
			this.Client = client;
			Array.Copy(copy_packet.Data, 0, this.Data, 0, copy_packet.Data.Length);

			index = copy_packet.index;
			Length = copy_packet.Length;
		}

		/// <summary>
		/// Creates a packet without a destination, with the channel through which it will be delivered
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static Packet Create(int channel, DataSize dataSize = DataSize.Normal) => Create(null, channel, dataSize);
		/// <summary>
		/// Creates a packet with the channel through which it will be delivered
		/// </summary>
		public static Packet Create(ClientSocket client, int channel, DataSize dataSize = DataSize.Normal)
		{
			var buffer = dataSize switch
			{
				DataSize.Large => m_largePacketsBuffer,
				_ => m_packetsBuffer
			};
			if (buffer.TryTake(out Packet packet))
			{
				packet.Reset();
				packet.Client = client;
				packet.Channel = channel;
				return packet;
			}
			return new Packet(client, channel, dataSize);
		}
		/// <summary>
		/// Creates a copy of the packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="copy_packet"></param>
		/// <returns></returns>
		public static Packet Create(ClientSocket client, Packet copy_packet)
		{
			if (m_packetsBuffer.TryTake(out Packet packet))
			{

				packet.Reset();
				packet.Client = client;
				Array.Copy(copy_packet.Data, 0, packet.Data, 0, copy_packet.Length);

				packet.index = copy_packet.index;
				packet.Length = copy_packet.Length;
				return packet;
			}
			return new Packet(client, copy_packet);
		}
		internal static Packet Create(DataSize dataSize = DataSize.Normal)
		{
			var buffer = dataSize switch
			{
				DataSize.Large => m_largePacketsBuffer,
				_ => m_packetsBuffer
			};
			if (buffer.TryTake(out Packet packet))
			{
				packet.Reset();
				packet.m_sendCicle = 1;
				return packet;
			}
			return new Packet(dataSize);
		}


		private void Reset()
        {
			address = null;
			Client = null;
			m_ack = false;
			m_sendCicle = 0;
			WriteType(0);
			Length = index = HEADER_SIZE;
		}


		/// <summary>
		/// Return the packet to the packet pool. Can't be used on the sent packet!!!
		/// </summary>
		public void Dispose()
		{
			m_packetsBuffer.Add(this);
		}
	}
}
