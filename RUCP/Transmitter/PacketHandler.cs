using System.Net;

namespace RUCP.Transmitter
{
    internal static class PacketHandler
    {
		internal static void Process(IServer server, Client client, Packet packet)
		{


			//Если связь с клиентом не установлена 
			if (!client.isConnected())
			{
				if (client.isRemoteHost)//Client
				{
					//Прием ответа от сервера на открытие подключение
					if (packet.TechnicalChannel == TechnicalChannel.Connection)
					{
						 client.TryOpenConnection(); 
					}
					//Если сервер отклонил подключения или пользватель решиль закрыть подключения до успешной установки подключения
					else if (packet.TechnicalChannel == TechnicalChannel.Disconnect)
					{
						client.CloseConnection(DisconnectReason.ConnectionFailed);
					}
				}
				else//Server
				{
					//и клиент хочет ее установить
					if (packet.TechnicalChannel == TechnicalChannel.Connection)
					{
						if (packet.Encrypt)
						{ client.CryptographerRSA.Decrypt(packet); }
						float versionClient = packet.ReadFloat();
						if (versionClient < Config.MIN_SUPPORTED_VERSION) { return; }

						if (client.TryOpenConnection())
						{
				            //If the connection was successful
							client.CryptographerRSA.SetPublicKey(packet);

							//отпровляем подтверждение клиенту
							Packet confirmPacket = Packet.Create();
							confirmPacket.TechnicalChannel = TechnicalChannel.Connection;
							client.CryptographerAES.WriteKey(confirmPacket);
							client.CryptographerRSA.Encrypt(confirmPacket);
							client.WriteInSocket(confirmPacket);
							confirmPacket.Dispose();

						}
					}
					else//Если получен пакет без установленной связи, отправить этому клиенту команду на отключения
					{
						Packet disconnectPacket = Packet.Create();
						disconnectPacket.TechnicalChannel = TechnicalChannel.Disconnect;
						client.WriteInSocket(disconnectPacket);
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
						confirmPacket.TechnicalChannel = TechnicalChannel.Connection;
						client.CryptographerAES.WriteKey(confirmPacket);
						client.CryptographerRSA.Encrypt(confirmPacket);
						client.WriteInSocket(confirmPacket);
						confirmPacket.Dispose();
					}
					packet.Dispose();
					break;

				case TechnicalChannel.Disconnect:
					//If the client is in a connection closing cicle
					if (client.Status == NetworkStatus.CLOSE_WAIT) { client.CloseConnection(DisconnectReason.NormalClosed, false); }
					else { client.CloseConnection(DisconnectReason.ClosedRemoteSide, true); }
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
						Process(server, client, p);
					}
					//packet.Dispose();
					break;
			}

		}
	}
}
