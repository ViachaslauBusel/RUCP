/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.BufferChannels
{
    class Buffer
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
        //Ожидаемый порядковый номер получаемого пакета
        protected volatile int numberReceived = 0;


        public Buffer(int size)
        {
            receivedPackages = new Packet[size];
            sentPackages = new Packet[size];
        }

        /// <summary>
		/// Вставка в буффер не подтвержденных пакетов
		/// </summary>
		/// <param name="packet"></param>
		public void Insert(Packet packet)
        {
            lock (sentPackages)
            {
                packet.WriteNumber((ushort)numberSent);
                int index = numberSent % sentPackages.Length;
                //Если пакет в буффере еще не подтвержден и требует переотправки
                if (sentPackages[index] != null) throw new BufferOverflowException("sent buffer overflow");

                sentPackages[index] = packet;

                numberSent = (numberSent + 1) % numberingWindowSize;
            }
        }

        /// <summary>
		/// Подтверждение о принятии пакета клиентом
		/// </summary>
		public int ConfirmAsk(int number)
        {
            int ping = 0;
            lock (sentPackages)
            {
                int index = number % sentPackages.Length;
                if (sentPackages[index] != null && sentPackages[index].ReadNumber() == number)
                {
                    sentPackages[index].setAck(true);
                    ping = (int)sentPackages[index].CalculatePing();
                    sentPackages[index] = null;

                }
            }
            return ping;
        }
    }
}
