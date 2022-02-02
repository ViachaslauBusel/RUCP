/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPc.Packets
{
   public partial class PacketData
    {

        internal const int DATA_SIZE = 1300;
        /// <summary>
        /// The size of the increased buffer for storing data for sending, including the header
        /// Theoretically, the maximum data size is 65535 bytes. IPv4 - 65507 (in addition to 8 bytes for the UDP header, another 20 is required for the IP header).
        /// </summary>
        internal const int LARGE_DATA_SIZE = 65507;
        //Индекс в массиве Data для записи\чтения данных
        protected int index = 0;

        public byte[] Data { get; protected set; } 
        internal int Length { get; set; }

        public int AvailableBytes => Length - index;

    }
}
