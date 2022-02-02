/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPs.Packets
{
   public partial class PacketData
    {
        /// <summary>
        /// Buffer size for storing data to send including header
        /// MTU — (Max IP Header Size) — (UDP Header Size) - (RUCP Header Size) = 1500 — 60 — 8 - 5 = 1427 bytes. MTU (for Ethernet by default 1500 bytes)
        /// </summary>
        internal const int DATA_SIZE = 1300;
        /// <summary>
        /// The size of the increased buffer for storing data for sending, including the header
        /// Theoretically, the maximum data size is 65535 bytes. IPv4 - 65507 (in addition to 8 bytes for the UDP header, another 20 is required for the IP header).
        /// </summary>
        internal const int LARGE_DATA_SIZE = 65507;
        /// <summary>Write/Read position index(carriage) in the data buffer</summary>
        protected int index = 0;

       
        /// <summary>Buffer for storing packet data including header</summary>
        internal byte[] Data { get; init; }

        /// <summary>The number of bytes used by the buffer </summary>
        internal int Length { get; set; }

        /// <summary>Available bytes for reading</summary>
        public int AvailableBytesForReading => Length - index;
        /// <summary>Available bytes for writing</summary>
        public int AvailableBytesForWriting => Data.Length - index;

        /// <summary>The number of bytes written to the packet, not including the header</summary>
        public int WrittenBytes => Length - Packet.HEADER_SIZE;

    }
}
