namespace RUCP.Transmitter
{
    internal static class PacketHandler
    {
		internal static void Process(IServer server, Packet packet)
		{

			
				Client client = packet.Client;  //Получаем оброботчик(Сокет) конкретного клиента


			//Если связь с клиентом не установлена 
			if (!client.isConnected())
			{
				if (client.isRemoteHost)
				{
					//Прием ответа от сервера на открытие подключение
					if (packet.TechnicalChannel == TechnicalChannel.Connection && server.AddClient(client))
					{ client.OpenConnection(); }
				}
				else
				{
					//и клиент хочет ее установить
					if (packet.TechnicalChannel == TechnicalChannel.Connection)
					{
						if (packet.Encrypt)
						{ client.CryptographerRSA.Decrypt(packet); }
						float versionClient = packet.ReadFloat();
						if (versionClient < Config.MIN_SUPPORTED_VERSION) { return; }

						if (server.AddClient(client))
						{
							client.OpenConnection();//If the connection was successful


							client.CryptographerRSA.SetPublicKey(packet);

							//отпровляем подтверждение клиенту
							Packet confirmPacket = Packet.Create();
							confirmPacket.InitClient(client);
							confirmPacket.TechnicalChannel = TechnicalChannel.Connection;
							client.CryptographerAES.WriteKey(confirmPacket);
							client.CryptographerRSA.Encrypt(confirmPacket);
							confirmPacket.SendImmediately();

						}
					}
					else//Если получен пакет без установленной связи, отправить этому клиенту команду на отключения
					{
						client.SendDisconnectCMD();
					}
				}
				packet.Dispose();
				return;
			}

				//Package processing
				switch (packet.TechnicalChannel)
				{

					case TechnicalChannel.ReliableACK://Confirmation of acceptance of the package by the other side
						client.ConfirmReliableACK(packet.Sequence);
						packet.Dispose();
						break;
					case TechnicalChannel.QueueACK://Confirmation of acceptance of the package by the other side
						client.ConfirmQueueACK(packet.Sequence);
						packet.Dispose();
						break;
					case TechnicalChannel.DiscardACK://Confirmation of acceptance of the package by the other side
						client.ConfirmDiscardACK(packet.Sequence);
						packet.Dispose();
						break;


					case TechnicalChannel.Connection:

						if (!client.isRemoteHost)
						{
							//send confirmation of successful connection to the client
							Packet confirmPacket = Packet.Create();
							confirmPacket.InitClient(client);
							confirmPacket.TechnicalChannel = TechnicalChannel.Connection;
							client.CryptographerAES.WriteKey(confirmPacket);
							client.CryptographerRSA.Encrypt(confirmPacket);
							confirmPacket.SendImmediately();
						}
						packet.Dispose();
						break;

					case TechnicalChannel.Disconnect:
						//	System.Console.WriteLine("client Disconnect");
						client.CloseConnection(false);
						packet.Dispose();
						break;

					case TechnicalChannel.Reliable:
						client.ProcessReliable(packet);
						break;
					case TechnicalChannel.Queue:
						client.ProcessQueue(packet);
						break;
					case TechnicalChannel.Discard:
						client.ProcessDiscard(packet);
						break;

					case TechnicalChannel.Unreliable:
						//Обработка пакета
						client.HandlerPack(packet);
						break;

				case TechnicalChannel.Stream:
                 //   if (!client.isRemoteHost) 
				//	{ Console.WriteLine($"[{(client.isRemoteHost ? "client" : "server")}]receive part stream"); }

					foreach (var p in client.Stream.Read(packet))
					{
						//	if (!client.isRemoteHost)
					//	{ Console.WriteLine($"receiven Sequence:{p.Sequence} ch:{p.TechnicalChannel}"); }
						Process(server, p);
					}
					//packet.Dispose();
					break;
				}

		}
	}
}
