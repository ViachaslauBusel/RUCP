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
        //Индекс в массиве Data для записи\чтения данных
        protected int index = 0;

        public byte[] Data { get; protected set; } 
        public int Length { get; protected set; }

        public int AvailableBytes => Length - index;

    }
}
