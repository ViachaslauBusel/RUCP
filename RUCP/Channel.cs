using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public static class Channel
    {
        public const int Unreliable = 0;
        public const int Reliable = 1;
        public const int Discard = 2;
        public const int Queue = 3;
        internal const int ReliableACK = 4;
        internal const int QueueACK = 5;
        internal const int DiscardACK = 6;

        internal const int Connection = 7;
        internal const int Disconnect = 8;
    }
}
