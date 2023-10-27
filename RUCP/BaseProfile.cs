using System;

namespace RUCP
{
    public abstract class BaseProfile
    {
        public Client Client { get; private set; }   
        internal void TechnicalInit(Client client)
        {
            Client = client;
        }
        public abstract void OpenConnection();
        public abstract void ChannelRead(Packet packet);
        public abstract bool HandleException(Exception exception);
        public abstract void CloseConnection(DisconnectReason reason);
        public abstract void CheckingConnection();
    }
}
