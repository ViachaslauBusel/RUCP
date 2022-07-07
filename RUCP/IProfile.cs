using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public interface IProfile
    {
        void OpenConnection();
        void ChannelRead(Packet pack);
        bool HandleException(Exception exception);
        void CloseConnection();
        void CheckingConnection();
    }
}
