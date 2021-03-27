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

				int numberPacket = packet.ReadNumber();//Порядковый номер принятого пакета
				int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере


				lock (receivedPackages)
				{
					//Если пакет еще не был принят
					if (receivedPackages[index] == null
							// Если принятый пакет был отправлен после чем пакет записанный в буффер
							|| NumberUtils.UshortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
					{
						receivedPackages[index]?.Dispose();
						receivedPackages[index] = packet;
						//Discard >>
						int compare = NumberUtils.UshortCompare(numberPacket, numberReceived);
						if (compare >= 0)// Пакет пришел первым
						{
							numberReceived = numberPacket;
						}
						// Пакет пришел не первым, ищем пакеты с таким же типом, если они есть, отбрасываем этот пакет
						else
						{
							for (int x =(numberPacket + 1) % numberingWindowSize; NumberUtils.UshortCompare(x, numberReceived) <= 0; x=(x+1)%numberingWindowSize)//Перебор пакетов пришедших после
							{
								Packet pack_rc = receivedPackages[x % receivedPackages.Length];
								if (pack_rc == null) continue;
								//Если пакет совпадает по типу и был отправлен после этого пакета
								if (packet.ReadType() == pack_rc.ReadType() && NumberUtils.UshortCompare(pack_rc.ReadNumber(), numberPacket) > 0)
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
				Debug.Log(e);
			}
		}
	}
}
