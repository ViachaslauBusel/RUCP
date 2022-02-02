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
        private IPEndPoint m_remotePoint;
        private UdpClient m_udpClient;
        private Client server;

        internal UdpSocket(IPEndPoint remoteAdress, Client serverSocket)
        {
            server = serverSocket;
            m_udpClient = new UdpClient(0);
            m_remotePoint = remoteAdress;
            m_udpClient.Connect(m_remotePoint);
        }

        internal void Send(Packet packet)
        {
            SendTo(packet.Data, packet.Length);
        }
        internal void SendTo(byte[] data, int size)
        {
            try
            {
                m_udpClient.Send(data, size);
            }
            //При возникновении ошибок при отправке пакетов, немедленно отключить от сервера
            catch (Exception e)
            {
              //  Debug.Log($"UdpSocket:{e}", MsgType.ERROR);
                server.TechnicalClose();
                ////Если клиент потерял связь с сервером, выполнить повторное соединение
                //if (!m_udpClient.Client.Connected)
                //{ m_udpClient.Connect(m_remotePoint); }
            }
        }

        internal int ReceiveFrom(out byte[] data)
        {
            IPEndPoint iPEnd = null;
            data = m_udpClient.Receive(ref iPEnd);
            return data.Length;
        }

        internal void Close()
        {
            m_udpClient.Close();
        }

        
    }
}
