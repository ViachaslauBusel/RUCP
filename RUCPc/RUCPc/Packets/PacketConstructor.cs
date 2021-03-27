/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCPc.Packets
{
   public partial class Packet
    {

		public static Packet Create(int channel) => new Packet(channel);

		private Packet(int channel)
		{
			Data = new byte[1500];
			Data[0] = (byte)channel;
			Length = index = headerLength;
		}

		public static Packet Create(Packet copy_packet) => new Packet(copy_packet);

		private Packet(Packet copy_packet)
		{
			Data = new byte[copy_packet.Length];
			Array.Copy(copy_packet.Data, 0, Data, 0, copy_packet.Length);

			index = copy_packet.index;
			Length = copy_packet.Length;
		}

		internal static Packet Create(byte[] data, int bytesReceived)
		{
			if (data == null || data.Length < Packet.headerLength) return null;
		 return	new Packet(data, bytesReceived);
		}
		private Packet(byte[] data, int bytesReceived)
		{
			sendCicle = 1;
			this.Data = data;
			//Получаем данные
			Length = bytesReceived;

			index = headerLength;
		}
	}
}
