using RUCP.Transmitter;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    internal interface IServer
    {
        UDPSocket Socket { get; }
        Resender Resender { get; }

        void CallException(Exception exception);
        bool Connect(Client client);
        bool Disconnect(Client client);
        IProfile CreateProfile();
    }
}
