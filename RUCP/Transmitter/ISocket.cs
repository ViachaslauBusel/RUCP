using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Transmitter
{
    public interface ISocket
    {
         int AvailableBytes { get; }

        void Connect(IPEndPoint iPEndPoint);
        void SendTo(Packet packet, IPEndPoint remoteAdress);
        void Send(Packet packet);
        void SendTo(byte[] data, int size, IPEndPoint remoteAdress);
        int ReceiveFrom(byte[] buffer, ref EndPoint endPoint);
        /// <summary>
        /// Close socket for incoming packets
        /// </summary>
        void Close();
     //   void Wait(int waitingTime);
        void Dispose();
    }
}
