using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
    internal class ReliableBuffer : Buffer
    {
		private Client m_master;
		internal ReliableBuffer(Client client, int size) : base(size)
		{
			m_master = client;
		}


		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal void Check(Packet pack)
		{

			int numberPacket = pack.ReadNumber();//Порядковый номер принятого пакета
			int index = numberPacket % receivedPackages.Length;//Порядковый номер в буфере


			lock (receivedPackages)
			{
				//Если пакет еще не был принят
				if (receivedPackages[index] == null
						// Если принятый пакет был отправлен после чем пакет записанный в буффер
						|| NumberUtils.UshortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
				{
					receivedPackages[index]?.Dispose();
					receivedPackages[index] = pack;

					m_master.HandlerPack(pack);
				}

			}

		}
	}
}
