
using RUCP.Channels;
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
            Debug.Assert(x <= ushort.MaxValue & x >= 0,  $"NumberUtils: параметр X:{x} имеет недопустимое значение");
            Debug.Assert(y <= ushort.MaxValue & y >= 0, $"NumberUtils: параметр Y:{y} имеет недопустимое значение");
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

        /// <summary>
        /// Returns a negative number if X < Y, 0 if X == Y, if X > Y returns a positive number
        /// </summary>
        /// <returns></returns>
        public static int RelativeSequenceNumber(int x, int y)
        {
            Debug.Assert(x <= Buffer.SEQUENCE_WINDOW_SIZE & x >= 0, $"NumberUtils: Parameter X:{x} has an invalid value");
            Debug.Assert(y <= Buffer.SEQUENCE_WINDOW_SIZE & y >= 0, $"NumberUtils: Parameter Y:{y} has an invalid value");
            return (x - y + Buffer.SEQUENCE_WINDOW_SIZE + Buffer.HALF_NUMBERING_WINDOW_SIZE) % Buffer.SEQUENCE_WINDOW_SIZE - Buffer.HALF_NUMBERING_WINDOW_SIZE;
        }
    }
}
