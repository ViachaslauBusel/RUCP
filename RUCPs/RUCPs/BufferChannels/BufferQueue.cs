/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Client;
using RUCPs.Logger;
using RUCPs.Packets;
using RUCPs.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPs.BufferChannels
{
	internal class BufferQueue:Buffer
    {
		private ClientSocket client;
		internal BufferQueue(ClientSocket client, int size) : base(size) 
		{
			this.client = client;
		}

		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal void Check(Packet packet)
		{
				int numberPacket = packet.ReadNumber();//Порядковый номер принятого пакета
				int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере
		

				lock (receivedPackages)
				{
				//Если пакет еще не был принят
				if (receivedPackages[index] == null
					// Если пакет в буфере  был отправлен после записаного в буфер
					|| NumberUtils.UshortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
					{
					receivedPackages[index]?.Dispose();
					receivedPackages[index] = packet;

						while (ConfirmPacket(numberPacket))//Если номер пакета совпадает с ожидаемым
						{
							client.HandlerPack(receivedPackages[index]);//Обрабатываем, и смотрим в перед есть ли еще не обработанные пакеты

							index = (index + 1) % receivedPackages.Length;//Порядковый номер следующего пакета в буфере
							if (receivedPackages[index] == null)
								break;
							numberPacket = receivedPackages[index].ReadNumber();
						}
					}

				}
		}

		private bool ConfirmPacket(int number)
		{

																		//	int compare = NumberUtils.ShortCompare(number, comingNumber);
			if (number == m_nextExpectedSequenceNumber)// Пакет пришел в нужном порядке
			{
				m_nextExpectedSequenceNumber = (m_nextExpectedSequenceNumber + 1) % NUMBERING_WINDOW_SIZE;

				return true;
			}
			return false;
		}
	}
}
