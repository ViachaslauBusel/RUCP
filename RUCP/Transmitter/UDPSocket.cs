using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RUCP.Transmitter
{
    internal sealed class UDPSocket : ISocket
    {
        private Socket m_socket = null;
        //private bool connection = false;
        //private Object m_locker = new Object();
        //private static UdpClient udpClient;

        public int AvailableBytes => m_socket.Available;



      

        private UDPSocket(int receiveBufferSize, int sendBufferSize, int localPort) 
        {
            m_socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            m_socket.DualMode = true;
           // m_socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            if (receiveBufferSize > 0) m_socket.ReceiveBufferSize = receiveBufferSize;
            if(sendBufferSize > 0) m_socket.SendBufferSize = sendBufferSize;
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, localPort);
            m_socket.Bind(localIP);
        }
        internal static UDPSocket CreateSocket(int receiveBufferSize = 0, int sendBufferSize = 0, int localPort = 0)//int ReceiveBufferSize, int SendBufferSize, 
        {
            UDPSocket udp = new UDPSocket(receiveBufferSize, sendBufferSize, localPort);
          

            return udp;
        }

        public void Connect(IPEndPoint iPEndPoint)
        {
           m_socket.Connect(iPEndPoint);
          //  connection = true;
        }

        public void SendTo(Packet packet, IPEndPoint remoteAdress)
        {
           // lock (m_locker)
            {
                if (m_socket.Connected) {
                   int sentBytes = m_socket.Send(packet.Data, packet.Length, SocketFlags.None);
                   if (sentBytes != packet.Length) throw new Exception("Failed to send packet");
                }
                else {
                    int sentBytes = m_socket.SendTo(packet.Data, packet.Length, SocketFlags.None, remoteAdress);
                    if (sentBytes != packet.Length) throw new Exception("Failed to send packet");
                }
            }
        }

        public void SendTo(byte[] data, int size, IPEndPoint remoteAdress)
        {
           // lock (m_locker)
            {
                m_socket.SendTo(data, size, SocketFlags.None, remoteAdress);
            }
        }
        public int ReceiveFrom(byte[] buffer, ref EndPoint endPoint)
        { 
            return m_socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);
        }


        public void Close()
        {
            m_socket?.Close();
        }
        public void Dispose()
        {
            m_socket?.Dispose();
            m_socket = null;
        }

        //public void Wait(int waitingTime)
        //{
        //    while (m_socket.Available > 0 && waitingTime-- > 0)
        //    {
        //        Thread.Sleep(1);
        //    }
        //}
    }
}
