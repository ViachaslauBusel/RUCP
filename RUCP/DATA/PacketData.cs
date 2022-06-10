using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.DATA
{
    public partial class PacketData
    {
        /// <summary>
        /// Длина заголовка пакета
        /// 1 байт - 1 бит флаг зашифровано ли содержимое пакета. Остальные байты канал пакета
        /// 2-3 байт - Тип пакета (Opcode)
        /// 4-5 байт - Порядковый номер пакета(Sequence)
        /// </summary>
        internal const int HEADER_SIZE = 5;
        /// <summary>
        /// Buffer size for storing data to send including header
        /// MTU — (Max IP Header Size) — (UDP Header Size) - (RUCP Header Size) = 1500 — 60 — 8 - 5 = 1427 bytes. MTU (for Ethernet by default 1500 bytes)
        /// </summary>
        internal const int DATA_SIZE = 1_400;
        /// <summary>
        /// The size of the increased buffer for storing data for sending, including the header
        /// Theoretically, the maximum data size is 65535 bytes. IPv4 - 65507 (in addition to 8 bytes for the UDP header, another 20 is required for the IP header).
        /// </summary>
        internal const int LARGE_DATA_SIZE = 32_000;


        /// <summary>Buffer for storing packet data including header</summary>
        protected byte[] m_data;
        /// <summary>Index of the carriage position for writing/reading data to/from the buffer</summary>
        protected int m_index = 0;
        /// <summary>Number of bytes used in buffer by data</summary>
        protected int m_realLength;
        protected Access m_dataAccess = Access.Lock;




        internal byte[] Data => m_data;
        /// <summary>Number of bytes used in buffer by data</summary>
        public int Length => m_realLength;
        /// <summary>Available bytes for reading</summary>
        public int AvailableBytesForReading => m_realLength - m_index;
        /// <summary>Available bytes for writing</summary>
        public int AvailableBytesForWriting => m_data.Length - m_index;
        /// <summary>The number of bytes written to the packet, not including the header</summary>
        public int WrittenBytes => m_realLength - HEADER_SIZE;

        


        internal void InitData(int length) 
        { 
            m_realLength = length;
            m_dataAccess = Access.Read;
        }

    }
}
