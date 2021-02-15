/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RUCP.Transmitter
{
    internal class UdpSocket
    {
        private static Socket socket = null;
      //  private static UdpClient udpClient;

        internal static void CreateSocket(int localPort)
        {
            if (socket != null) throw new Exception("The server is already running");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, localPort);
            socket.Bind(localIP);
           
            //udpClient = new UdpClient(localPort);
        }

        internal static void Send(Packet packet)
        {
            SendTo(packet.Data, packet.Length, packet.Client.Address);
        }
        internal static void SendTo(byte[] data, int size, IPEndPoint remoteAdress)
        {
             socket.SendTo(data, size, SocketFlags.None, remoteAdress);
          //  udpClient.Send(data, size, remoteAdress);
        }

        internal static void ReceiveFrom(ref Packet packet)
        {

            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
          //  byte[] buffer = udpClient.Receive(ref senderRemote);
          //  Array.Copy(buffer, packet.Data, buffer.Length);
         //   packet.address = senderRemote;
          //  packet.Length = buffer.Length;
            packet.Length =  socket.ReceiveFrom(packet.Data, ref senderRemote);
            packet.address = (IPEndPoint)senderRemote;
        }

        internal static void Close()
        {
          //  udpClient.Close();
           socket?.Close();
           socket = null;
        }

        
    }
}
