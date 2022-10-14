using NUnit.Framework;
using RUCP;
using System;
using System.Net.Sockets;
using System.Threading;

namespace nTests
{
    public class Custom
    {
        class ServerProfile : BaseProfile
        {
            public override void ChannelRead(Packet packet)
            {
               
            }

            public override void CheckingConnection()
            {
               
            }

            public override void CloseConnection(DisconnectReason reason)
            {
             
            }

            public override bool HandleException(Exception exception)
            {
             return false;
            }

            public override void OpenConnection()
            {
               
            }
        }

        private Server m_server;

        [SetUp]
        public void SetUp()
        {
            m_server = new Server(3232);
            m_server.SetHandler(() => new ServerProfile());
            m_server.Start();
        }
        [Test]
        public void UnknownClient()
        {
            byte[] data = new byte[1];
            UdpClient socket = new UdpClient();
           

            for(int i = 0; i< 100; i++)
            {
                socket.Send(data, "127.0.0.1", 3232);
                Thread.Sleep(1);
            }
        }
    }
}
