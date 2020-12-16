using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Packets
{
   public partial class PacketData
    {
        protected int index = 0;
        protected byte[] data; //Буфер
                               //  public int length;//Размер данных
        public byte[] Data => data;
        public int Length { get; protected set; }

        public int AvailableBytes => Length - index;

    }
}
