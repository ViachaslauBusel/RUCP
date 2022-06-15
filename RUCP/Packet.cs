using RUCP.Collections;
using RUCP.DATA;
using RUCP.Transmitter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
	//public enum DataSize
	//{
	//	Normal,
	//	Large
	//}
	public partial class Packet : PacketData, IDelayed, IComparable, IComparable<Packet>
    {


		internal volatile int m_sendCicle = 0;
		private volatile bool m_ack = false;
		/// <summary>Время отправки пакета</summary>
		private long m_sendTime;
		/// <summary>Время повторной отправки пакета при неудачной попытке доставки</summary>
		private long m_resendTime;




        /// <summary>Время повторной отправки пакета при неудачной попытке доставки</summary>
        public long ResendTime => m_resendTime;
		public bool isBlock => m_sendCicle != 0;
		internal bool ACK { get => m_ack; set => m_ack = value; }

		public Client Client { get; private set; }


		public void InitClient(Client client) { Client = client; }
		/// <summary>
		/// Записывает время отправки/переотправки
		/// </summary>
		internal void WriteSendTime()
		{
			if (m_sendCicle++ == 0) { m_sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }

			m_resendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Client.Statistic.GetTimeoutInterval() * m_sendCicle;
		}


        /// <summary>
        /// Time elapsed since the packet was sent
        /// </summary>
        /// <returns></returns>
        internal int CalculatePing()
		{
			return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m_sendTime);
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
		/// Порядковый номер отпровляемого пакета.
		/// </summary>
		/// <param name="sequence"></param>
		unsafe internal ushort Sequence
        {
            set
            {
				fixed (byte* d = m_data)
				{ Buffer.MemoryCopy(&value, d + 3, 2, 2); }
			}
            get => BitConverter.ToUInt16(m_data, 3);
        }

      




        /// <summary>
        /// Записывает тип пакета в заголовок
        /// </summary>
        unsafe public void WriteType(short type)
		{
			fixed (byte* d = m_data)
			{ Buffer.MemoryCopy(&type, d + 1, 2, 2); }
		}
		public int ReadType()
		{
			return BitConverter.ToInt16(m_data, 1);
		}

		public long GetDelay()
		{
			return ResendTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public int CompareTo(object obj) => CompareTo((Packet)obj);
		public int CompareTo(Packet packet)
		{
			if (ResendTime > packet.ResendTime) return 1;
			return -1;
		}




		private Packet(Client client, Channel channel)
		{

			m_data = new byte[DATA_SIZE];
			Reset();
			this.Client = client;
			this.TechnicalChannel = (int)channel;

		}
		private Packet()
		{
			m_data = new byte[DATA_SIZE];
			Reset();
		}
		private Packet Copy(Client client, Packet copy_packet)
		{
			m_data = new byte[copy_packet.Data.Length];
			this.Client = client;
			Array.Copy(copy_packet.Data, 0, this.Data, 0, copy_packet.Data.Length);

			m_index = copy_packet.m_index;
			m_realLength = copy_packet.m_realLength;
			return this;
		}

		/// <summary>
		/// Creates a packet without a destination, with the channel through which it will be delivered
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static Packet Create(Channel channel) => Create(null, channel);
		/// <summary>
		/// Creates a packet with the channel through which it will be delivered
		/// </summary>
		public static Packet Create(Client client, Channel channel)
		{
            if (TryTakeFromPool(out Packet packet))
            {
                packet.Client = client;
                packet.TechnicalChannel = (int)channel;
                return packet;
            }
            return new Packet(client, channel);
		}
		/// <summary>
		/// Creates a copy of the packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="copy_packet"></param>
		/// <returns></returns>
		public static Packet Create(Client client, Packet copy_packet)
		{
            if (TryTakeFromPool(out Packet packet))
            {
                packet.Copy(client, copy_packet);
                return packet;
            }
            return new Packet().Copy(client, copy_packet);
		}
		internal static Packet Create()
		{
            if (TryTakeFromPool(out Packet packet))
            {
                return packet;
            }
            return new Packet();
		}


		public void Reset()
		{
			m_dataAccess = Access.Write;
			Client = null;
			m_ack = false;
			m_sendCicle = 0;
			WriteType(0);
			TechnicalChannel = 0;
			Sequence = 0;
			m_realLength = m_index = HEADER_SIZE;
		}

		internal void SendImmediately()
		{
			if (Client == null)
			{
				throw new Exception("The packet cannot be sent, the client is not specified");
			}
			if (m_sendCicle != 0 || m_dataAccess == Access.Lock)
			{
				throw new Exception($"Packet is blocked, sending is not possible, m_sendCicle:{m_sendCicle}, m_dataAccess:{m_dataAccess}");
			}



			bool dispose = false;

			if (Encrypt) Client.CryptographerAES.Encrypt(this);

			//Вставка в буфер отправленных пакетов для дальнейшего подтверждения об успешной доставки пакета
			if (Client.InsertBuffer(this))
			{
				//Record for re-sending
				Client.Server.Resender.Add(this);
				m_dataAccess = Access.Lock;
			}
			else { dispose = true; }

			Client.Server.Socket.SendTo(this, Client.RemoteAddress);


			if (dispose) Dispose();

		}

		public NetStream Send()
		{
			if (Client == null)
			{
				throw new Exception("The packet cannot be sent, the client is not specified");
			}

			if (m_sendCicle != 0 || m_dataAccess == Access.Lock)
			{
				throw new Exception($"Packet is blocked, sending is not possible");
			}

			bool dispose = false;

			if (Encrypt) Client.CryptographerAES.Encrypt(this);

			//Вставка в буфер отправленных пакетов для дальнейшего подтверждения об успешной доставке пакета
			if (Client.InsertBuffer(this))
			{
				//Record for re-sending
				Client.Server.Resender.Add(this);
				m_dataAccess = Access.Lock;
			}
			else { dispose = true; }

			Client.Stream.Write(this);

			NetStream stream = Client.Stream; 
			if (dispose) Dispose();
			return stream;
		}
		
	}
}
