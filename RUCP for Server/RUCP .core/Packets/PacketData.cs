/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Packets
{
   public partial class PacketData
    {
        protected int index = 0;
     //   protected byte[] Data = new byte[1500]; //Буфер

        internal byte[] Data { get; } = new byte[1500]; //Буфер
        internal int Length { get; set; }
        //  public int length;//Размер данных
        public int AvailableBytes => Length - index;

    }
}
