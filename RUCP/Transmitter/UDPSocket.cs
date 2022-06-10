using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RUCP.Transmitter
{
    internal class UDPSocket : ISocket
    {
        private Socket m_socket = null;
        private bool connection = false;
        private Object m_locker = new Object();

        public int AvailableBytes => m_socket.Available;



        //  private static UdpClient udpClient;

        private UDPSocket(int localPort) 
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, localPort);
            m_socket.Bind(localIP);
        }
        internal static UDPSocket CreateSocket(int localPort = 0)
        {
            UDPSocket udp = new UDPSocket(localPort);
          

            return udp;
        }

        public void Connect(IPEndPoint iPEndPoint)
        {
           m_socket.Connect(iPEndPoint);
            connection = true;
        }

        public void SendTo(Packet packet, IPEndPoint remoteAdress)
        {
           // lock (m_locker)
            {
                if (connection) { m_socket.Send(packet.Data, packet.Length, SocketFlags.None); }
                else { m_socket.SendTo(packet.Data, packet.Length, SocketFlags.None, remoteAdress); }
            }
        }
        public void Send(Packet packet)
        {
         //   lock (m_locker)
            {
                m_socket.Send(packet.Data, packet.Length, SocketFlags.None);
            }
        }
        public void SendTo(byte[] data, int size, IPEndPoint remoteAdress)
        {
          //  lock (m_locker)
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
            m_socket = null;
        }
    }
}
