/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Packets;
using RUCPs.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPs.BufferChannels
{
     public class Buffer
    {
		/// <summary>
		/// размер окна нумерации пакетов
		/// </summary>
		protected static readonly int numberingWindowSize = 65_000;

		//Буффер для хранения полеченных пакетов
		protected Packet[] receivedPackages;
        //Буффер для хранения отправленных пакетов
        protected Packet[] sentPackages;
        //Порядковый номер отправляемого пакета
        protected volatile int numberSent = 0;
		/// <summary>
		/// Ожидаемый порядковый номер получаемого пакета
		/// </summary>
		protected volatile int numberReceived = 0;


        internal Buffer(int size)
        {
            receivedPackages = new Packet[size];
            sentPackages = new Packet[size];
        }

		/// <summary>
		/// Подтверждение о принятии пакета клиентом
		/// </summary>
		/// <param name="number"></param>
		public void ConfirmAsk(int number)
		{
			lock (sentPackages)
			{
				int index = number % sentPackages.Length;
				if (sentPackages[index] != null && sentPackages[index].ReadNumber() == number)
				{
					sentPackages[index].ACK = true;
					sentPackages[index].CalculatePing();
					sentPackages[index] = null;
				}
			}
		}
		/// <summary>
		/// Вставка в буффер не подтвержденных пакетов
		/// </summary>
		internal void Insert(Packet packet)
		{
			lock (sentPackages)
			{
				packet.WriteNumber((ushort)numberSent);
				int index = numberSent % sentPackages.Length;
				//Если пакет в буффере еще не подтвержден и требует переотправки
				if (sentPackages[index] != null) throw new BufferOverflowException("send buffer overflow");

				sentPackages[index] = packet;

				numberSent = (numberSent + 1) % numberingWindowSize;
			}
		}

		internal void Dispose()
		{
			for (int i = 0; i < receivedPackages.Length; i++)
				 receivedPackages[i]?.Dispose();
		}
	}
}
