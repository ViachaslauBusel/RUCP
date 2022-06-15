using RUCP.Channels;
using RUCP.Cryptography;
using RUCP.ServerSide;
using RUCP.Transmitter;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RUCP
{
	public class Server : IServer
    {
		private Func<IProfile> m_createProfile;

		private ISocket m_socket;
		private ClientList m_clients;
		private Resender m_resender;
		private CheckingConnections m_cheking;
		private TaskPool m_taskPool;
		private int m_port;
	//	private bool m_networkEmulator = false;
		private volatile bool m_work = false;
		private ServerOptions m_options;



	//	private volatile int m_processPackets = 0;
	//	private volatile int m_completedPackets = 0;

		/// <summary> Throwing exceptions received in the server</summary>
		public event Action<Exception> throwingExceptions;

		
		//internal void CallException(Exception exception) { throwingExceptions?.Invoke(exception); }
		ISocket IServer.Socket => m_socket;
		Resender IServer.Resender => m_resender;
        TaskPool IServer.TaskPool => m_taskPool;
        ServerOptions IServer.Options => m_options;

		void IServer.CallException(Exception exception) { throwingExceptions?.Invoke(exception); }
		internal void CallException(Exception exception) { throwingExceptions?.Invoke(exception); }
		bool IServer.AddClient(Client client)
        {
            if (m_clients.AddClient(client))
            {
				m_cheking.InsertClient(client);
				return true;
            }
			return false;
        }
		/// <summary>
		/// Удаляет клиента из списка клиентов
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		bool IServer.RemoveClient(Client client)
        {
			return m_clients.RemoveClient(client);
        }

		public void SetHandler(Func<IProfile> han)
		{
			m_createProfile = han;
		}

	    IProfile IServer.CreateProfile()
		{
			return m_createProfile.Invoke();
		}

		public Server(int port)
		{
			this.m_port = port;
			//m_networkEmulator = networkEmulator;
		}

		public void Start(ServerOptions options = null)
		{
			try
			{
				

				if (m_work) throw new Exception("The server is already in running mode");
				m_work = true;

				if (options == null) options = new ServerOptions();

				
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
				string version = fvi.FileVersion;
				System.Console.WriteLine($"RUCP ver.{version}");
				m_options = options.Clone();
				//	buffer = new BlockingCollection<Packet>(new ConcurrentQueue<Packet>());
				//Создание сокета по порту для считывание данных
				m_socket =  UDPSocket.CreateSocket(receiveBufferSize: 3_145_728, sendBufferSize: 3_145_728, localPort: m_port);//m_networkEmulator ? NetworkEmulator.CreateNetworkEmulatorSocket(m_port) :

				RSA.SetPrivateKey(ContainerRSAKey.LoadPrivateKey());

				


				m_clients = new ClientList(this);
				//Запуск потока переотправки потеряных пакетов
				m_resender = Resender.Start(this);
				//Запуск потока проверки соединений
				m_cheking = CheckingConnections.Start(this);
				m_taskPool = new TaskPool(m_options.MaxParallelism);

				if (m_options.Mode == Mode.Automatic)
				{
					//Запуск потока считывание датаграмм
					Thread server_th = new Thread(() => Run());
					server_th.IsBackground = false;
					server_th.Start();
				}

				System.Console.WriteLine("The server was started successfully");
			}
			catch (Exception e)
			{
				System.Console.WriteLine($"Failed to start the server: {e.Message}");
				CallException(e);
			}
		}

		public void ProcessPacket()
        {
			if (m_options.Mode != Mode.Manual || m_socket == null) return;
			int availableBytes = m_socket.AvailableBytes;
			EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
			while (availableBytes > 0)
			{
				try
				{   //Создание нового пакета для хранение данных
					Packet packet = Packet.Create();

					//Считывание датаграм
					int receiveBytes = m_socket.ReceiveFrom(packet.Data, ref senderRemote);
					availableBytes -= receiveBytes;
					packet.InitData(receiveBytes);
					packet.InitClient(m_clients.GetClient((IPEndPoint)senderRemote));
					

						PacketHandler.Process(this, packet);
					

				}
				catch (SocketException e)
				{
					if (e.ErrorCode == 10004)
						break;//Exit
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

		//Считывание датаграм из сокета
		private void Run()
		{
			EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
			while (m_work)
			{
				try
				{   
					//Создание нового пакета для хранение данных
					Packet packet = Packet.Create();

					//Считывание датаграм
					int receiveBytes = m_socket.ReceiveFrom(packet.Data, ref senderRemote);
					Client client = m_clients.GetClient((IPEndPoint)senderRemote);
					packet.InitData(receiveBytes);
					packet.InitClient(client);

					//m_processPackets++;

					client.InsertTask(() =>
					{
						try
						{
						
							PacketHandler.Process(this, packet);
						}
						catch (BufferOverflowException)
						{
							CallException(new Exception($"The client:{client.ID} was disconnected due to a buffer overflow"));
							client.Close();
						}
						catch (Exception e)
						{
							CallException(new Exception($"Client:{client.ID} was disconnected due to an unhandled exception"));
							client.Close();
							CallException(e);
						}
						
					});
					

					
				}
                catch (SocketException e)
                {
					//if (e.ErrorCode != 10004 && e.ErrorCode != 4)
					{ CallException(e); }
				}
                catch (Exception e)
				{
					  CallException(e); 
				}
			}
			m_socket.Close();

		}
		/// <summary>
		/// Shuts down the server, disconnects all clients
		/// </summary>
		public void Stop()
		{
			try
			{
				m_work = false;
				
				foreach (Client client in m_clients)
				{
					//Отправка клиенту команды на отключение и очистка списка клиентов
					client.CloseConnection();
				}

				m_taskPool.Dispose();

			} catch (Exception e)
            {
				CallException(e);
            }
			finally 
			{
				System.Console.WriteLine($"RUCP shutdown.");
			}
			
		}

      
    }
}
