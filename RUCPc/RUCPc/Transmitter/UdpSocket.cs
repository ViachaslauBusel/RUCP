/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.Debugger;
using RUCPc.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RUCPc.Transmitter
{
    internal class UdpSocket
    {
        private IPEndPoint remotePoint;
        private UdpClient udpClient;
        private ServerSocket server;

        internal UdpSocket(IPEndPoint remoteAdress, ServerSocket serverSocket)
        {
            server = serverSocket;
            udpClient = new UdpClient(0);
            remotePoint = remoteAdress;
            udpClient.Connect(remotePoint);
        }

        internal void Send(Packet packet)
        {
            SendTo(packet.Data, packet.Length);
        }
        internal void SendTo(byte[] data, int size)
        {
            try
            {
                udpClient.Send(data, size);
            }
            catch (SocketException e)
            {
                Debug.Log(e);
            }
        }

        internal int ReceiveFrom(out byte[] data)
        {
            IPEndPoint iPEnd = null;
            data = udpClient.Receive(ref iPEnd);
            return data.Length;
        }

        internal void Close()
        {
            udpClient.Close();
        }

        
    }
}
