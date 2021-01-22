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
        private IPEndPoint remotePoint;
        private UdpClient udpClient;

        internal UdpSocket(IPEndPoint remoteAdress)
        {
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
        }

        internal void Close()
        {
            udpClient.Close();
        }

        
    }
}
