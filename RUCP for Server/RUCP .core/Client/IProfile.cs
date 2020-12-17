/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

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
