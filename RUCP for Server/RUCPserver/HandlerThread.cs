/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Client;
using RUCP.Debugger;
using RUCP.Packets;
using RUCP.Transmitter;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace RUCP
{
	internal class HandlerThread
    {


	//	private BlockingCollection<Packet> buffer;
	//	private bool work = true;
	//
	//	private Thread thread;






		internal static void Process(Packet packet)
		{
			//	thread = Thread.CurrentThread;
			//	buffer = Server.GetBuffer();
			//Packet packet;

			//long startTime;
			//	while (work)
			//	{
			try
			{

				//	packet = buffer.Take();//Получаем задачу(пакет) из очереди

				
				ClientSocket client = packet.BindClient();  //Получаем оброботчик(Сокет) конкретного клиента

				if (!client.isConnected() && packet.isChannel(Channel.Connection))
				{
					if (packet.Encrypt)
						client.CryptographerRSA.Decrypt(packet);
					float versionClient = packet.ReadFloat();
					if (versionClient < Server.minSupportedVersion) { client.CloseConnection(); return; }

					if (ClientList.AddClient(client.ID, client))
					{

						client.OpenConnection();//If the connection was successful

						CheckingConnections.InsertClient(client);//Вставка клиента в очередь проверки соеденение



						client.CryptographerRSA.SetPublicKey(packet);

						//отпровляем подтверждение клиенту
						Packet confirmPacket = Packet.Create(client, Channel.Connection);
						client.CryptographerAES.WriteKey(confirmPacket);
						client.CryptographerRSA.Encrypt(confirmPacket);
						confirmPacket.Send();

					}
					return;
				
				}
				
				//Package processing
				switch (packet.Channel)
				{

					case Channel.ReliableACK://Confirmation of acceptance of the package by the other side
						client.ConfirmReliableACK(packet.ReadNumber());
						break;
					case Channel.QueueACK://Confirmation of acceptance of the package by the other side
						client.ConfirmQueueACK(packet.ReadNumber());
						break;
					case Channel.DiscardACK://Confirmation of acceptance of the package by the other side
						client.ConfirmDiscardACK(packet.ReadNumber());
						break;


					case Channel.Connection:
						
						{
							//send confirmation of successful connection to the client
							Packet confirmPacket = Packet.Create(client, Channel.Connection);
							client.CryptographerAES.WriteKey(confirmPacket);
							client.CryptographerRSA.Encrypt(confirmPacket);
							confirmPacket.Send();
						}
						break;

					case Channel.Disconnect:
					//	System.Console.WriteLine("client Disconnect");
						client.CloseConnection(false);
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
					Debug.Log(e);
			}
		//	}

		}

    //    internal void Start()
   //     {
		//	new Thread(() => Run()).Start();
	//	}
	//	internal void Stop()
	//	{
	//		work = false;
	//	}
	//	internal void Join()
    //    {
	//		thread.Join();
    //    }
    }
}
