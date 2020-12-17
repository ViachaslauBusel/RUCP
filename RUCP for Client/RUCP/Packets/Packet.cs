/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Collections;
using RUCP.Network;
using System;
using System.Net;

namespace RUCP.Packets
{
    public partial class Packet : PacketData, IDelayed, IComparable
    {
		/// <summary>
		/// Длина заголовка пакета
		/// </summary>
		private static readonly int headerLength = 5;
		internal long sendTime = 0;//Время отправки
		private long resendTime = 0;//Время повторной отправки пакета при неудачной попытке доставки
		internal volatile int sendCicle = 0;//При отправке или получении пакета, пакет блокируется для невозможности повторной отправки
		private volatile bool ack = false;


        public long ResendTime => resendTime;
		public bool isBlock => sendCicle != 0;

		/// <summary>
		/// Записывает время отправки/переотправки
		/// </summary>
		public void WriteSendTime(long timeOut)
		{
			if (sendCicle == 1) sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			resendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (timeOut * sendCicle);
		}
		public long CalculatePing()
		{
			return (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sendTime);
		}

		public bool isAck()
		{
			return ack;
		}

		/***
		 * Задает метку доставки пакета.
		 * Задается автоматически при получении пакета от клиента с подтверждением доставки
		 * @param ack
		 */
		public void setAck(bool ack)
		{
			//Debug.Log($"подтверждение полученно через: {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sendTime}");
			this.ack = ack;
		}


		/***
		 * Возврощает канал по которому будет\был передан пакет
		 * @return
		 */
		public int ReadChannel()
		{
			return (int)Data[0];
		}

		public bool isChannel(int channel)
		{
			return Data[0] == channel;
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
			return resendTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		}

        public int CompareTo(object obj)
        {
			if (resendTime > ((Packet)obj).resendTime) return 1;
			return -1;
		}
    }
}
