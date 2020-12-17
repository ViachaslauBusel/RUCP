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
      //  private Socket socket = null;
        private IPEndPoint remotePoint;
        private UdpClient udpClient;

        internal UdpSocket(IPEndPoint remoteAdress)
        {
            udpClient = new UdpClient(0);
            remotePoint = remoteAdress;
            udpClient.Connect(remotePoint);
           // socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
           // IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
         //   socket.Bind(localIP);
            // Debug.Log($"ip adress: {remoteAdress}");

        }

        internal void Send(Packet packet)
        {
            SendTo(packet.Data, packet.Length);
        }
        internal void SendTo(byte[] data, int size)
        {
            //  socket.SendTo(data, remotePoint);
            try
            {
                udpClient.Send(data, size);
            } catch (SocketException e) {
                Debug.Log($"e: {e.ErrorCode} : {e.Message}");
                    }
        }

        internal int ReceiveFrom(out byte[] data)
        {
            data = null;
            try
            {
                IPEndPoint iPEnd = null;
            data = udpClient.Receive(ref iPEnd);
            return data.Length;
            }
            catch (SocketException e)
            {
                Debug.Log($"e: {e.ErrorCode} : {e.Message}");
            }
            return 0;
            //  return socket.Receive(data);
        }

        internal void Close()
        {
            udpClient.Close();
           // socket.Close();
        }

        
    }
}
