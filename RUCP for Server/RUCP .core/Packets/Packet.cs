/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Client;
using RUCP.Collections;
using RUCP.Tools;
using System;
using System.Net;

namespace RUCP.Packets
{
    public partial class Packet : PacketData, IDelayed, IComparable
    {
		/// <summary>
		/// Длина заголовка пакета
		/// </summary>
		private const int headerLength = 5;
		public long sendTime { get; private set; } = 0;//Время отправки
		public long ResendTime { get; private set; } = 0;//Время повторной отправки пакета при неудачной попытке доставки
		internal int sendCicle = 0;//При отправке или получении пакета, пакет блокируется для невозможности повторной отправки
		private bool ack = false;

		public ClientSocket Client { get; private set; }
		internal IPEndPoint address;


		/// <summary>
		/// Записывает время отправки/переотправки
		/// </summary>
		internal void WriteSendTime()
		{
			if (sendCicle == 1) sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			ResendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Client.GetTimeout() * sendCicle;
		}
		internal void CalculatePing()
		{
			Client.Ping = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sendTime);
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
		internal void setAck(bool ack)
		{
			this.ack = ack;
		}

		internal ClientSocket BindClient()
		{
			//Получаем клиента по адрессу и порту от которого пришла датаграмма
			Client = ClientList.GetClient(SocketInformer.GetID(address)) ?? new ClientSocket(address, Server.ProfileCreate());
			return Client;
		}

		/***
		 * Возврощает канал по которому будет\был передан пакет
		 * @return
		 */
		public int ReadChannel()
		{
			return Data[0];
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
			return ResendTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

        public int CompareTo(object obj)
        {
			if (ResendTime > ((Packet)obj).ResendTime) return 1;
			return -1;
		}
    }
}
