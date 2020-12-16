using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Client
{
    public interface IProfile
    {
        /// <summary>
        /// Открытие соединения
        /// </summary>
        bool OpenConnection(Packet pack);
        void ChannelRead(Packet pack);
        void CloseConnection();
        void CheckingConnection();
    }
}
