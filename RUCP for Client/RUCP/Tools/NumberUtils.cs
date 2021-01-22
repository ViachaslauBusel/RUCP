/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RUCP.Tools
{
   public class NumberUtils
    {
        /// <summary>
        /// Возвращает -1 если X < Y, 0 если X == Y, если X > Y возвращает разницу между X и Y - N > 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int ByteCompare(byte x, byte y)
        {
            if(x > y)
            {
                int z = x - y;
                if (z >= 128) return -1;
                return z;
            }

            if(x < y)
            {
                int z = y - x;
                if (z >= 128) return (byte)(x - y);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Возвращает -1 если X < Y, 0 если X == Y, если X > Y возвращает 1
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int UshortCompare(int x, int y)
        {
            if (x > y)
            {
                int z = x - y;
                if (z >= short.MaxValue) return -1;
                return 1;
            }

            if (x < y)
            {
                int z = y - x;
                if (z >= short.MaxValue) return 1;
                return -1;
            }

            return 0;
        }
    }
}
