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
	public sealed class Server : IServer
    {
		private Func<BaseProfile> m_createProfile;

		private ISocket m_socket;
		private ClientList m_clients;
		private Resender m_resender;
		private CheckingConnections m_cheking;
		private TaskPool m_taskPool;
		private int m_port;
	//	private bool m_networkEmulator = false;
		private volatile bool m_packetHandlerWork = false, m_acceptConnection = false;
		private ServerOptions m_options;
        private Thread m_server_th;



        //	private volatile int m_processPackets = 0;
        //	private volatile int m_completedPackets = 0;

        /// <summary> Throwing exceptions received in the server</summary>
        public event Action<Exception> throwingExceptions;

		
		//internal void CallException(Exception exception) { throwingExceptions?.Invoke(exception); }
		ISocket IServer.Socket => m_socket;
		Resender IServer.Resender => m_resender;
        TaskPool IServer.TaskPool => m_taskPool;
        ServerOptions IServer.Options => m_options;
        ClientList IServer.ClientList => m_clients;

        void IServer.CallException(Exception exception) { throwingExceptions?.Invoke(exception); }
		internal void CallException(Exception exception) { throwingExceptions?.Invoke(exception); }
		bool IServer.AddClient(Client client)
        {
            if (m_acceptConnection && m_clients.AddClient(client))
            {
				m_cheking.InsertClient(client);
				return true;
            }
			return false;
        }
        /// <summary>
        /// Removes a client from the list of clients
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        bool IServer.RemoveClient(Client client)
        {
			return m_clients.RemoveClient(client);
        }

		public void SetHandler(Func<BaseProfile> han)
		{
			m_createProfile = han;
		}

	    BaseProfile IServer.CreateProfile()
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
				

				if (m_packetHandlerWork) throw new Exception("The server is already in running mode");
				m_packetHandlerWork = true;
				m_acceptConnection = true;

				if (options == null) options = new ServerOptions();

				
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
				string version = fvi.FileVersion;
				System.Console.WriteLine($"RUCP ver.{version}");
				m_options = options.Clone();
                //	buffer = new BlockingCollection<Packet>(new ConcurrentQueue<Packet>());
                //Creating a socket by port for reading data
                m_socket =  UDPSocket.CreateSocket(receiveBufferSize: 3_145_728, sendBufferSize: 3_145_728, localPort: m_port);//m_networkEmulator ? NetworkEmulator.CreateNetworkEmulatorSocket(m_port) :

				RSA.SetPrivateKey(ContainerRSAKey.LoadPrivateKey());

				


				m_clients = new ClientList(this);
                //Starting the Lost Packet Resend Thread
                m_resender = Resender.Start(this);
                //Starting a connection check thread
                m_cheking = CheckingConnections.Start(this);
				m_taskPool = new TaskPool(m_options.MaxParallelism);

				if (m_options.Mode == ServerMode.Automatic)
				{
                    //Starting a thread reading datagrams
                    m_server_th = new Thread(() => Run());
					m_server_th.IsBackground = true;
					m_server_th.Start();
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
			if (m_options.Mode != ServerMode.Manual || m_socket == null) return;
			int availableBytes = m_socket.AvailableBytes;
			EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
			Client client = null;

            while (availableBytes > 0)
			{
				try
                {   
				    //Create a new packet for data storage
                    Packet packet = Packet.Create();

                    //Reading datagrams
                    int receiveBytes = m_socket.ReceiveFrom(packet.Data, ref senderRemote);
					availableBytes -= receiveBytes;
					packet.InitData(receiveBytes);
				    client = m_clients.GetClient((IPEndPoint)senderRemote);
					//packet.InitClient();
				

				    PacketHandler.Process(this, client, packet);
				}
                catch (BufferOverflowException)
                {
					if (client != null)
					{
						CallException(new Exception($"The client:{client.ID} was disconnected due to a buffer overflow"));
						client.CloseConnection(DisconnectReason.BufferOverflow);
					}
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

        //Reading datagrams from a socket
        private void Run()
		{
			EndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);
			while (m_packetHandlerWork)
			{
				try
				{
					remoteSender = new IPEndPoint(IPAddress.Any, 0);

                    //Create a new packet for data storage
                    Packet packet = Packet.Create();

                    //Reading datagrams
                    int receiveBytes = m_socket.ReceiveFrom(packet.Data, ref remoteSender);
					Client client = m_clients.GetClient((IPEndPoint)remoteSender);
					packet.InitData(receiveBytes);
				//	packet.InitClient(client);

					//m_processPackets++;

					client.InsertTask(() =>
					{
						try
						{
							PacketHandler.Process(this, client, packet);
						}
						catch (BufferOverflowException)
						{
							CallException(new Exception($"The client:{client.ID} was disconnected due to a buffer overflow"));
							client.CloseConnection(DisconnectReason.BufferOverflow);
						}
						catch (Exception e)
						{
							//CallException(new Exception($"Client:{client.ID} was disconnected due to an unhandled exception"));
							if (!client.HandleException(e))
							{ client.CloseConnection(DisconnectReason.UnhandledException); }
							//CallException(e);
						}
						
					});
					

					
				}
                catch (SocketException e)
                {
					//e.ErrorCode == 10054 The remote host forcibly terminated the existing connection
					if(e.ErrorCode == 10054)
                    {
						//TODO Handle the exception thrown by the local socket when sending a packet to a remote closed socket
						//CallException(new Exception($"The client:{remoteSender} was disconnected. Error code:{e.ErrorCode}"));
						continue;
					}
					//if (e.ErrorCode != 10004 && e.ErrorCode != 4)
					
					if (m_packetHandlerWork){ CallException(e); }
				}
                catch (Exception e)
				{
					if (m_packetHandlerWork) { CallException(e); }
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
				m_acceptConnection = false;
				foreach (Client client in m_clients)
				{
					//Отправка клиенту команды на отключение и очистка списка клиентов
					client.CloseConnection(DisconnectReason.NormalClosed, true);
				}
				
				m_resender?.Stop();
				m_cheking?.Stop();

				
				
				m_taskPool?.Dispose();

				m_packetHandlerWork = false;
				m_socket?.Close();
				m_socket?.Dispose();

				m_server_th?.Join();

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
