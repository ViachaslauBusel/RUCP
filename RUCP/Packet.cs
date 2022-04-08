using RUCP.Collections;
using RUCP.DATA;
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
	public class Packet : PacketData, IDelayed, IComparable, IComparable<Packet>
    {

		public Packet Next { get; set; } = null;
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
			if (m_sendCicle++ == 0) m_sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			m_resendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Client.Network.GetTimeoutInterval();
		}
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
		public int Channel
		{
			get => m_data[0] & 0b0111_1111;
			private set { m_data[0] = (byte)(value | (m_data[0] & 0b1000_0000)); }
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




		private Packet(Client client, int channel)
		{

			m_data = new byte[DATA_SIZE];
			Reset();
			this.Client = client;
			this.Channel = channel;

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
		public static Packet Create(int channel) => Create(null, channel);
		/// <summary>
		/// Creates a packet with the channel through which it will be delivered
		/// </summary>
		public static Packet Create(Client client, int channel)
		{
            if (PacketPool.TryTake(out Packet packet))
            {
                packet.Client = client;
                packet.Channel = channel;
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
            if (PacketPool.TryTake(out Packet packet))
            {
                packet.Copy(client, copy_packet);
                return packet;
            }
            return new Packet().Copy(client, copy_packet);
		}
		internal static Packet Create()
		{
            if (PacketPool.TryTake(out Packet packet))
            {
                return packet;
            }
            return new Packet();
		}


		public void Reset()
		{
			Client = null;
			m_ack = false;
			m_sendCicle = 0;
			WriteType(0);
			m_realLength = m_index = HEADER_SIZE;
		}

		public void Send()
		{


				if (Client == null)
				{
					throw new Exception("The packet cannot be sent, the client is not specified");
				}
				if (m_sendCicle != 0)
				{
					throw new Exception("Packet is blocked, sending is not possible");
				}

	
			bool dispose = false;

				if (Encrypt) Client.CryptographerAES.Encrypt(this);

				//Вставка в буфер отправленных пакетов для дальнейшего подтверждения об успешной доставки пакета
				if (Client.InsertBuffer(this))
				{ Client.Server.Resender.Add(this); }//Record for re-sending
				else dispose = true;

				Client.Server.Socket.SendTo(this, Client.RemoteAdress);

				if (dispose) Dispose();
		
		}
		/// <summary>
		/// Return the packet to the packet pool. Can't be used on the sent packet!!!
		/// </summary>
		public void Dispose()//TODO если два раза вызвать на одном и том же пакете, то возникнет баг совместного использование одного пакета
		{
            Reset();
            PacketPool.Insert(this);
        }
	}
}
