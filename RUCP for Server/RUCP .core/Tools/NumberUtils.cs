/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Tools
{
    public class NumberUtils
    {
        /// <summary>
        /// Возвращает -1 если X < Y, 0 если X == Y, если X > Y возвращает 1
        /// </summary>
        public static int ShortCompare(int x, int y)
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
        /*
           */
    }
}
