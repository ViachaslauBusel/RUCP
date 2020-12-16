using RUCP.Client;
using RUCP.Debugger;
using RUCP.Packets;
using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.BufferChannels
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
					|| NumberUtils.ShortCompare(numberPacket, receivedPackages[index].ReadNumber()) > 0)
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
			if (number == numberReceived)// Пакет пришел в нужном порядке
			{
				numberReceived = (numberReceived + 1) % numberingWindowSize;

				return true;
			}
			return false;
		}
	}
}
