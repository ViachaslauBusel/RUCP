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
