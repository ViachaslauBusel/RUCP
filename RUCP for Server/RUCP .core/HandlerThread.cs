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
				if (!client.isConnected())
				{
					if (packet.isChannel(Channel.Connection) && ClientList.AddClient(client.ID, client))
					{
						Console.WriteLine("Клиент добавлен");
						if (client.openConnection(packet))//Если установка соеденения прошла успешна
						{
							CheckingConnections.InsertClient(client);//Вставка клиента в очередь проверки соеденение
																	 //отпровляем подтверждение клиенту
							Packet.Create(client, Channel.Connection).Send();
							Console.WriteLine("online: " + ClientList.online());
						}
						else
						{
							Console.WriteLine("Неудачная попытка соеденениея");
							client.CloseConnection();
						}

					}
					return;
				//	Debug.logError("HandlerThread", "Client not found: " + packet.Client.ID, null);
				}
				//Обработка пакета
				switch (packet.ReadChannel())
				{

					case Channel.ReliableACK://Подтвердить доставку
						client.ConfirmReliableACK(packet.ReadNumber());
						break;
					case Channel.QueueACK:
						client.ConfirmQueueACK(packet.ReadNumber());
						break;
					case Channel.DiscardACK:
						client.ConfirmDiscardACK(packet.ReadNumber());
						break;


					case Channel.Connection:
						//Если клиент уже есть в списке
						{
						//	Console.WriteLine("Не удалось добавить клиента");
							if (client.isConnected())//Если клиент уже подключен
							 Packet.Create(client, Channel.Connection).Send(); //отпровляем подтверждение клиенту
						}
						break;

					case Channel.Disconnect:
						Console.WriteLine("client Disconnect");
						client.CloseConnection();
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
				//long duration = System.nanoTime() - startTime;
				//System.out.println("Время обработки пакета: "+duration);

			}
			catch (Exception e)
			{

					Debug.logError("HandlerThread", e.Message, e.StackTrace);

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
