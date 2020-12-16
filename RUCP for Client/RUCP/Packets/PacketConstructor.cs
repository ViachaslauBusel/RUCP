
using RUCP.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCP.Packets
{
   public partial class Packet
    {
		public Packet(int channel, int length = 1500)
		{
			data = new byte[length];
			data[0] = (byte)channel;

			Length = index = headerLength;
		}
		public Packet(Packet packet)
		{
			data = new byte[packet.data.Length];
			Array.Copy(packet.data, 0, this.data, 0, packet.data.Length);
		//	length = -1;

			index = packet.index;
			Length = packet.Length;
		}
		internal Packet(byte[] data, int bytesReceived)
		{
			sendCicle = 1;
			this.data = data;
			//Получаем данные
			Length = bytesReceived;

			index = headerLength;
		}
	}
}
