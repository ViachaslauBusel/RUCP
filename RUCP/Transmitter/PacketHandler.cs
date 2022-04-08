using RUCP.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Transmitter
{
    internal class PacketHandler
    {
		internal static void Process(IServer server, Packet packet)
		{

			try
			{

			
				Client client = packet.Client;  //Получаем оброботчик(Сокет) конкретного клиента

				//Если связь с клиентом не установлена 
				if (!client.isConnected())
				{
					if (client.isRemoteHost)
					{
						//Прием ответа от сервера на открытие подключение
						if (packet.Channel == Channel.Connection)
						{ client.OpenConnection(); }
					}
					else
					{
						//и клиент хочет ее установить
						if (packet.Channel == Channel.Connection)
						{
							if (packet.Encrypt)
							{ client.CryptographerRSA.Decrypt(packet); }
							float versionClient = packet.ReadFloat();
							if (versionClient < Config.MIN_SUPPORTED_VERSION) { return; }

							if (server.Connect(client))
							{

								client.OpenConnection();//If the connection was successful


								client.CryptographerRSA.SetPublicKey(packet);

								//отпровляем подтверждение клиенту
								Packet confirmPacket = Packet.Create(client, Channel.Connection);
								client.CryptographerAES.WriteKey(confirmPacket);
								client.CryptographerRSA.Encrypt(confirmPacket);
								confirmPacket.Send();

							}
						}
						else//Если получен пакет без установленной связи, отправить этому клиенту команду на отключения
						{
							client.Disconnect();
						}
					}
					packet.Dispose();
					return;
				}

				//Package processing
				switch (packet.Channel)
				{

					case Channel.ReliableACK://Confirmation of acceptance of the package by the other side
						client.ConfirmReliableACK(packet.Sequence);
						packet.Dispose();
						break;
					case Channel.QueueACK://Confirmation of acceptance of the package by the other side
						client.ConfirmQueueACK(packet.Sequence);
						packet.Dispose();
						break;
					case Channel.DiscardACK://Confirmation of acceptance of the package by the other side
						client.ConfirmDiscardACK(packet.Sequence);
						packet.Dispose();
						break;


					case Channel.Connection:

						if (!client.isRemoteHost)
						{
							//send confirmation of successful connection to the client
							Packet confirmPacket = Packet.Create(client, Channel.Connection);
							client.CryptographerAES.WriteKey(confirmPacket);
							client.CryptographerRSA.Encrypt(confirmPacket);
							confirmPacket.Send();
						}
						packet.Dispose();
						break;

					case Channel.Disconnect:
						//	System.Console.WriteLine("client Disconnect");
						client.CloseConnection(false);
						packet.Dispose();
						break;

					case Channel.Reliable:
						client.ProcessReliable(packet);
						break;
					case Channel.Queue:
						client.ProcessQueue(packet);
						break;
					case Channel.Discard:
						client.ProcessDiscard(packet);
						break;

					case Channel.Unreliable:
						//Обработка пакета
						client.HandlerPack(packet);
						break;
				}

			}
			catch (Exception e)
			{
				server.CallException(e);
			}
			//	}

		}
	}
}
