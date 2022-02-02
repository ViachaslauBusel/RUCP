/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Client;
using RUCPs.Cryptography;
using RUCPs.Debugger;
using RUCPs.Handler;
using RUCPs.Packets;
using RUCPs.Transmitter;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RUCPs
{
    public class Server
    {
		public static string Version => $"ver. {VERSION.ToString("0.###")}a";
		internal const float VERSION = 0.007f;
		internal const float MIN_SUPPORTED_VERSION = 0.003f;
		/***
		 * Буфер для хранение пакетов в очереди
		 */
		//	private static BlockingCollection<Packet> buffer;
		private static Func<IProfile> createProfile;
		
		private int port;
		private volatile int processPackets = 0;
		private volatile int completedPackets = 0;
		//	private HandlerThread[] pool_handler;
		/// <summary> Throwing exceptions received in the server</summary>
		public static event Action<Exception> throwingExceptions;

		internal static void CallException(Exception e) { throwingExceptions?.Invoke(e); }

		public static void SetHandler(Func<IProfile> han)
		{
			createProfile = han;
		}

		internal static IProfile ProfileCreate()
		{
			return createProfile();
		}

		public Server(int port)
		{
			//Debug.init(new DebugConsole());
			this.port = port;
		}

		public void Start()
		{
			try
			{
				System.Console.WriteLine("RUCP " + Version);
				//	buffer = new BlockingCollection<Packet>(new ConcurrentQueue<Packet>());
				//Создание сокета по порту для считывание данных
				UdpSocket.CreateSocket(port);

				RSA.SetPrivateKey(ContainerRSAKey.LoadPrivateKey());

				//Запуск потока проверки соединений
				CheckingConnections.Start();

				//Запуск потока переотправки потеряных пакетов
				Resender.Start();

				//Запуск потока считывание датаграмм
				Thread server_th = new Thread(() => Run());
				server_th.IsBackground = false;
				server_th.Start();

				System.Console.WriteLine("The server was started successfully");
			}
			catch (Exception e)
			{
				System.Console.WriteLine("Failed to start server");
				CallException(e);
			}
		}


	//	internal static BlockingCollection<Packet> GetBuffer(){
	//		return buffer;
//		}

		//Считывание датаграм из сокета
		internal void Run()
		{

			//Создание потоков оброботчиков(количество ядер х2)
			//	ThreadPool.SetMaxThreads(Environment.ProcessorCount * 3, Environment.ProcessorCount * 3);
			//	ThreadPool.GetMaxThreads(out int worker, out int port);
			//	Console.WriteLine($"worker: {worker} port: {port}");
			//	pool_handler = new HandlerThread[Environment.ProcessorCount * 2];
			//Запускаем потоки
			/*	for (int i = 0; i < pool_handler.Length; i++)
				{
					pool_handler[i] = new HandlerThread();
					pool_handler[i].Start();
				}*/



			while (true)
			{
				try
				{
					//Создание нового пакета для хранение данных
					Packet packet = Packet.Create();
					//Считывание датаграм
				    UdpSocket.ReceiveFrom(ref packet);

					processPackets++;
					Task.Run(() => {
						HandlerThread.Process(packet);
					    Interlocked.Increment(ref completedPackets);
						});

				}
                catch (SocketException e)
                {
					if(e.ErrorCode == 10004)
					break;
					if (e.ErrorCode == 10054)
						continue;
					CallException(e);
				}
				catch (Exception e)
				{
					CallException(e);
				}
			}

		}

		public void Stop()
		{
			UdpSocket.Close();



			while(processPackets != completedPackets) { Thread.Sleep(1); }
			//Console.WriteLine($"process: {processPackets} completed: {completedPackets}");

			foreach (ClientSocket client in ClientList.instance)
			{
				//Отправка клиенту команды на отключение и очистка списка клиентов
				client.CloseConnection();
			}
	

			System.Console.WriteLine("RUCP shutdown");
		}
	}
}
