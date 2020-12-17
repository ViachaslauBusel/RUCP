/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;

namespace RUCP.Tools
{
    class Time
    {
       // private static DateTime time_start = DateTime.Now;
        public static long currentTimeMillis
        {
            get
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
    }
}
