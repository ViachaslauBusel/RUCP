/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.Packets;
using RUCPc.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPc.BufferChannels
{
    class BufferQueue:Buffer
    {
		private Client server;
		public BufferQueue(Client server, int size) : base(size) 
		{
			this.server = server;
		}



		public void Check(Packet pack)
		{
				int numberPacket = pack.ReadNumber();//Порядковый номер принятого пакета
				int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере


					//Если пакет еще не был принят
					if (receivedPackages[index] == null
					// Если пакет в буфере  был отправлен позже записаного в буфер
					|| NumberUtils.UshortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
					{
						receivedPackages[index] = pack;

						while (ConfirmPacket(numberPacket))//Если номер пакета совпадает с ожидаемым
						{
							server.AddPipeline(receivedPackages[index]);//Обрабатываем, и смотрим в перед есть ли еще не обработанные пакеты

							index = (index + 1) % receivedPackages.Length;//Порядковый номер следующего пакета в буфере
							if (receivedPackages[index] == null)
								break;
							numberPacket = receivedPackages[index].ReadNumber();
						}
					}

				
		}

		private bool ConfirmPacket(int number)
		{
																		//	int compare = NumberUtils.ShortCompare(number, comingNumber);
			if (number == m_nextExpectedSequenceNumber)// Пакет пришел в нужном порядке
			{
				m_nextExpectedSequenceNumber = (m_nextExpectedSequenceNumber + 1) % numberingWindowSize;

				return true;
			}
			return false;
		}
	}
}
