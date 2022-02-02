/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Client;
using RUCPs.Collections;
using RUCPs.Tools;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace RUCPs.Packets
{
    public partial class Packet : PacketData, IDelayed, IComparable
    {
		/// <summary>
		/// Длина заголовка пакета
		/// 1 байт - 1 бит флаг зашифровано ли содержимое пакета. Остальные байты канал пакета
		/// 2-3 байт - Тип пакета
		/// 4-5 байт - Порядковый номер пакета
		/// </summary>
		internal const int HEADER_SIZE = 5;

		private volatile int m_sendCicle = 0;
		private volatile bool m_ack = false;

		internal long SendTime { get; private set; } = 0;//Время отправки
		public long ResendTime { get; private set; } = 0;//Время повторной отправки пакета при неудачной попытке доставки

	
		public bool isBlock => m_sendCicle != 0;

		
		internal bool ACK { get => m_ack; set { m_ack = value; } }

		public ClientSocket Client { get; private set; }
		internal IPEndPoint address;


		/// <summary>
		/// Записывает время отправки/переотправки
		/// </summary>
		internal void WriteSendTime()
		{
			if (m_sendCicle++ == 0) SendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			ResendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Client.GetTimeoutInterval();
		}
		internal int CalculatePing()
		{
		 	return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - SendTime);
		}


		internal ClientSocket BindClient()
		{
			//Получаем клиента по адрессу и порту от которого пришла датаграмма
			Client = ClientList.GetClient(SocketInformer.GetID(address)) ?? new ClientSocket(address, Server.ProfileCreate());
			return Client;
		}
		public bool Encrypt
        {
            get => (Data[0] & 0b1000_0000) == 0b1000_0000;
			set { if (value) Data[0] |= 0b1000_0000; else Data[0] &= 0b0111_1111; }
        }
        /***
		 * Возврощает канал по которому будет\был передан пакет
		 * @return
		 */
        public int Channel
		{
			get => Data[0] & 0b0111_1111;
            private set { Data[0] = (byte)value; }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool isChannel(int channel)
		{
			return Channel == channel;
		}

		/***
		 * Задает порядковый номер отпровляемого пакета.
		 *
		 */
		unsafe internal void WriteNumber(ushort number)
		{
			fixed (byte* d = Data)
			{ Buffer.MemoryCopy(&number, d + 3, 2, 2); }
		}

		/// <summary>
		/// Возврощает порядковый номер отпровленного пакета
		/// </summary>
		internal int ReadNumber()
		{
			return BitConverter.ToUInt16(Data, 3);
		}



		/// <summary>
		/// Записывает тип пакета в заголовок
		/// </summary>
		unsafe public void WriteType(short type)
		{
			fixed (byte* d = Data)
			{ Buffer.MemoryCopy(&type, d + 1, 2, 2); }
		}
		public int ReadType()
		{
			return BitConverter.ToInt16(Data, 1);
		}

		public long GetDelay()
        {
			return ResendTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

        public int CompareTo(object obj)
        {
			if (ResendTime > ((Packet)obj).ResendTime) return 1;
			return -1;
		}
    }
}
