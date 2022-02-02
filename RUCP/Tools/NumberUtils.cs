using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RUCP.Tools
{
    public static class NumberUtils
    {
        /// <summary>
        ///  Возвращает -1 если X < Y, 0 если X == Y, если X > Y возвращает 1
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int UshortCompare(int x, int y)
        {
            Debug.Assert(x <= ushort.MaxValue & x >= 0,  $"NumberUtils: параметр X:{x} имеет допустимое значение");
            Debug.Assert(y <= ushort.MaxValue & y >= 0, $"NumberUtils: параметр Y:{y} имеет допустимое значение");
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
