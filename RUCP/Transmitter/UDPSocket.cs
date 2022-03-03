using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RUCP.Transmitter
{
    internal class UDPSocket
    {
        private Socket m_socket = null;
        private bool connection = false;



        //  private static UdpClient udpClient;

        private UDPSocket() { }
        internal static UDPSocket CreateSocket(int localPort = 0)
        {
            UDPSocket udp = new UDPSocket();
            udp.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, localPort);
            udp.m_socket.Bind(localIP);

            return udp;
        }

        internal void Connect(IPEndPoint iPEndPoint)
        {
           m_socket.Connect(iPEndPoint);
            connection = true;
        }

        internal void SendTo(Packet packet, IPEndPoint remoteAdress)
        {
            if (connection) { m_socket.Send(packet.Data, packet.Length, SocketFlags.None); }
            else { m_socket.SendTo(packet.Data, packet.Length, SocketFlags.None, remoteAdress); }
        }
        internal void Send(Packet packet)
        {
            m_socket.Send(packet.Data, packet.Length, SocketFlags.None);
        }
        internal void SendTo(byte[] data, int size, IPEndPoint remoteAdress)
        {
            m_socket.SendTo(data, size, SocketFlags.None, remoteAdress);
        }

        internal int ReceiveFrom(byte[] buffer, ref EndPoint endPoint)
        { 
            return m_socket.ReceiveFrom(buffer, ref endPoint);
        }


        internal void Close()
        {
            m_socket?.Close();
            m_socket = null;
        }
    }
}
