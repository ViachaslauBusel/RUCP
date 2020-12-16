
using RUCP.Tools;
using RUCP.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using RUCP.Packets;

namespace RUCP.BufferChannels
{
	internal class BufferReliable : Buffer
    {
		internal BufferReliable(int size):base(size)
        {
            
        }


		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal bool Check(Packet pack)
		{

				int numberPacket = pack.ReadNumber();//Порядковый номер принятого пакета
				int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере


				lock (receivedPackages)
				{
					//Если пакет еще не был принят
					if (receivedPackages[index] == null
							// Если принятый пакет был отправлен после чем пакет записанный в буффер
							|| NumberUtils.ShortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
					{
				     	receivedPackages[index]?.Dispose();
						receivedPackages[index] = pack;

						return true;
					}
					return false;
				}
			
		}
	}
}
