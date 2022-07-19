using RUCP.DATA;
using RUCP.Transmitter;
using System;

namespace RUCP
{
    public sealed partial class Packet : PacketData
    {


		private volatile int m_sendCicle = 0;
		private volatile bool m_ack = false;
		/// <summary>Время отправки пакета</summary>
		private long m_sendTime;
		private long m_resendTime;


        /// <summary>Время повторной отправки пакета при неудачной попытке доставки</summary>
        internal long ResendTime => m_resendTime;
		internal long SendTime => m_sendTime;
		internal bool ACK { get => m_ack; set => m_ack = value; }


		/// <summary>
		/// Records the time the packet was sent. And the time to resend the packet when the delivery attempt fails
		/// </summary>
		internal void WriteSendTime(int timeout)
		{
			if (m_sendCicle++ == 0) { m_sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }

			m_resendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + timeout * m_sendCicle;
		}


        /// <summary>
        /// Time elapsed since the packet was sent
        /// </summary>
        /// <returns></returns>
        internal int CalculatePing()
		{
			return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m_sendTime);
		}
		internal long GetDelay()
		{
			return ResendTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public bool Encrypt
		{
			get => false;// (m_data[0] & 0b1000_0000) == 0b1000_0000;
			set { }// { if (value) m_data[0] |= 0b1000_0000; else m_data[0] &= 0b0111_1111; }
		}
		/// <summary>
		/// Возврощает канал по которому будет\был передан пакет
		/// </summary>
		public Channel Channel => (Channel)TechnicalChannel;

		internal int TechnicalChannel 
		{
			get => (m_data[0] & 0b0111_1111);
		    set { m_data[0] = (byte)(value | (m_data[0] & 0b1000_0000)); }
		}

		/// <summary>
		/// The sequence number of the packet being sent
		/// </summary>
		/// <param name="sequence"></param>
		internal unsafe ushort Sequence
        {
            set
            {
				fixed (byte* d = m_data)
				{ Buffer.MemoryCopy(&value, d + 3, 2, 2); }
			}
            get => BitConverter.ToUInt16(m_data, 3);
        }
		/// <summary>
		/// Writes the opcode to the header
		/// </summary>
		public unsafe short OpCode
        {
			get => BitConverter.ToInt16(m_data, 1);
            set
            {
				fixed (byte* d = m_data)
				{ Buffer.MemoryCopy(&value, d + 1, 2, 2); }
			}
		}


	

		public int CompareTo(object obj) => CompareTo((Packet)obj);
		public int CompareTo(Packet packet)
		{
			if (ResendTime > packet.ResendTime) return 1;
			return -1;
		}




	


		public void Reset()
		{
			m_dataAcces = DataAccess.Write;
			m_ack = false;
			m_sendCicle = 0;
			OpCode = 0;
			TechnicalChannel = 0;
			Sequence = 0;
			m_realLength = m_index = HEADER_SIZE;
		}

	

		
	}
}
