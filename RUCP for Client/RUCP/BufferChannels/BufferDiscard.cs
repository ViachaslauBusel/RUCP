/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Network;
using RUCP.Packets;
using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.BufferChannels
{
    class BufferDiscard: Buffer
    {
		public BufferDiscard(int size) : base(size)
		{

		}


		

		public bool Check(Packet pack)
		{
			try
			{

				int numberPacket = pack.ReadNumber();//Порядковый номер принятого пакета
				int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере


					//Если пакет еще не был принят
					if (receivedPackages[index] == null
							// Если принятый пакет был отправлен позже чем пакет записанный в буффер
							|| NumberUtils.UshortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
					{
						receivedPackages[index] = pack;
						//Discard >>
						int compare = NumberUtils.UshortCompare(numberPacket, numberReceived);
						if (compare >= 0)// Пакет пришел первым
						{
							numberReceived = numberPacket;
						}
						// Пакет пришел не первым, ищем пакеты с таким же типом, если они есть, отбрасываем этот пакет
						else 
						{
							for (int x = (numberPacket + 1) % numberingWindowSize; NumberUtils.UshortCompare(x, numberReceived) <= 0; x = (x+1)%numberingWindowSize)//Перебор пакетов пришедших после
							{
								Packet pack_rc = receivedPackages[x % receivedPackages.Length];
							if (pack_rc == null) continue;
							//Если пакет совпадает по типу и был отправлен после этого пакета
							if (pack.ReadType() == pack_rc.ReadType() && NumberUtils.UshortCompare(pack_rc.ReadNumber(), numberPacket) > 0)
							 return false; 
							}
						}
						//Discard <<
						return true;
					}
					return false;
				
			}
			catch (Exception e)
			{
				Debug.LogError(GetType().Name, e.ToString(), e.StackTrace);
				return false;
			}
		}
	}
}
