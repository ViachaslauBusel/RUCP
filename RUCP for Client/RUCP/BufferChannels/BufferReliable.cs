
using System;
using System.Collections.Generic;
using System.Text;
using RUCP.Packets;
using RUCP.Tools;

namespace RUCP.BufferChannels
{
    class BufferReliable : Buffer
    {
        public BufferReliable(int size):base(size)
        {
            
        }



		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		public bool Check(Packet pack)
		{

				int numberPacket = pack.ReadNumber();//Порядковый номер принятого пакета
				int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере


					//Если пакет еще не был принят
					if (receivedPackages[index] == null
							// Если принятый пакет был отправлен позже чем пакет записанный в буффер
							|| NumberUtils.ShortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
					{
						receivedPackages[index] = pack;

						return true;
					}
					return false;
		}
	}
}
