/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Client;
using RUCPs.Debugger;
using RUCPs.Packets;
using RUCPs.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPs.BufferChannels
{
	internal class BufferDiscard: Buffer
    {
        private ClientSocket clientSocket;

        internal BufferDiscard(ClientSocket clientSocket, int size) : base(size)
		{
			this.clientSocket = clientSocket;
		}

		
		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal void Check(Packet packet)
		{
			try
			{

				int packetNumber = packet.ReadNumber();//Порядковый номер принятого пакета
				int bufferIndex = packetNumber % receivedPackages.Length;//Порядковый номер в буфере


				lock (receivedPackages)
				{
					//Если пакет еще не был принят
					if (receivedPackages[bufferIndex] == null
							// Или если принятый пакет был отправлен после чем пакет записанный в буффер
							|| NumberUtils.UshortCompare(packetNumber, receivedPackages[bufferIndex].ReadNumber()) > 0)
					{
						receivedPackages[bufferIndex]?.Dispose();
						receivedPackages[bufferIndex] = packet;
						//Discard >>
						int compare = NumberUtils.UshortCompare(packetNumber, m_nextExpectedSequenceNumber);
						if (compare >= 0)// Пакет пришел первым
						{
							m_nextExpectedSequenceNumber = packetNumber;
						}
						// Пакет пришел не первым, ищем пакеты с таким же типом, если они есть, отбрасываем этот пакет
						else
						{
							for (int x =(packetNumber + 1) % NUMBERING_WINDOW_SIZE; NumberUtils.UshortCompare(x, m_nextExpectedSequenceNumber) <= 0; x=(x+1)%NUMBERING_WINDOW_SIZE)//Перебор пакетов пришедших после
							{
								Packet pack_rc = receivedPackages[x % receivedPackages.Length];
								if (pack_rc == null) continue;
								//Если пакет совпадает по типу и был отправлен после этого пакета
								if (packet.ReadType() == pack_rc.ReadType() && NumberUtils.UshortCompare(pack_rc.ReadNumber(), packetNumber) > 0)
								{ return; }
							}
						}
						//Discard <<
						clientSocket.HandlerPack(packet);
					}
				}
			}
			catch (Exception e)
			{
				Server.CallException(e);
			}
		}
	}
}
