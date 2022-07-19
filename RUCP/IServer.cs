using RUCP.ServerSide;
using RUCP.Transmitter;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCP
{
    internal interface IServer
    {
        ISocket Socket { get; }
        Resender Resender { get; }


        TaskPool TaskPool { get; }
        ClientList ClientList { get; }
        ServerOptions Options { get; }

        void CallException(Exception exception);
        bool AddClient(Client client);
        bool RemoveClient(Client client);
        BaseProfile CreateProfile();
    }
}
