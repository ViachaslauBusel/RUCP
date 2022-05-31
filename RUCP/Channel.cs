using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public enum Channel : byte
    {

        Unreliable = TechnicalChannel.Unreliable,
        Reliable = TechnicalChannel.Reliable,
        Discard = TechnicalChannel.Discard,
        Queue = TechnicalChannel.Queue
    }

    internal static class TechnicalChannel
    {
        internal const int Unreliable = 0;
        internal const int Reliable = 1;
        internal const int Discard = 2;
        internal const int Queue = 3;
        internal const int ReliableACK = 4;
        internal const int QueueACK = 5;
        internal const int DiscardACK = 6;

        internal const int Connection = 7;
        internal const int Disconnect = 8;
    }
}
