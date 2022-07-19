using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public partial class Packet
    {
		private Packet(Channel channel)
		{

			m_data = new byte[DATA_SIZE];
			Reset();

			this.TechnicalChannel = (int)channel;

		}
		private Packet()
		{
			m_data = new byte[DATA_SIZE];
			Reset();
		}
		private Packet Copy(Packet copy_packet)
		{
			m_data = new byte[copy_packet.Data.Length];
			;
			Array.Copy(copy_packet.Data, 0, this.Data, 0, copy_packet.Data.Length);

			m_index = copy_packet.m_index;
			m_realLength = copy_packet.m_realLength;
			return this;
		}


		/// <summary>
		/// Creates a packet with the channel through which it will be delivered
		/// </summary>
		public static Packet Create(Channel channel)
		{
			if (TryTakeFromPool(out Packet packet))
			{
				packet.TechnicalChannel = (int)channel;
				return packet;
			}
			return new Packet(channel);
		}
		/// <summary>
		/// Creates a copy of the packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="copy_packet"></param>
		/// <returns></returns>
		public static Packet Create(Packet copy_packet)
		{
			if (TryTakeFromPool(out Packet packet))
			{
				packet.Copy(copy_packet);
				return packet;
			}
			return new Packet().Copy(copy_packet);
		}
		internal static Packet Create()
		{
			if (TryTakeFromPool(out Packet packet))
			{
				return packet;
			}
			return new Packet();
		}
	}
}
