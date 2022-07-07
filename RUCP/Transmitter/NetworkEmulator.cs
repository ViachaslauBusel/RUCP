using RUCP.Transmitter;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Transmitter
{
    public class NetworkEmulator : ISocket
    {
        class NetworkBuffer
        {
            public long timeStamp;
            public byte[] data;
            public EndPoint endPoint;
        }

        private Socket m_socket = null;
        private List<NetworkBuffer> m_buffer = new List<NetworkBuffer>();


        public int AvailableBytes => m_socket.Available;
        private NetworkEmulator(int localPort) 
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, localPort);
            m_socket.Bind(localIP);
        }
        public static ISocket CreateNetworkEmulatorSocket(int localPort = 0)
        {
            return new NetworkEmulator(localPort);
        }


        public void Connect(IPEndPoint iPEndPoint)
        {
            m_socket.Connect(iPEndPoint);
        }

        public void SendTo(Packet packet, IPEndPoint remoteAdress)
        {
            m_socket.SendTo(packet.Data, packet.Length, SocketFlags.None, remoteAdress);
        }
        public void Send(Packet packet)
        {
            m_socket.Send(packet.Data, packet.Length, SocketFlags.None);
        }
        public void SendTo(byte[] data, int size, IPEndPoint remoteAdress)
        {
            m_socket.SendTo(data, size, SocketFlags.None, remoteAdress);
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint endPoint)
        {
            return m_socket.ReceiveFrom(buffer, ref endPoint);
        }


        public void Close()
        {
            m_socket?.Close();
            m_socket = null;
        }

        public void Wait(int waitingTime)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
