using System;

namespace RUCP
{
    [Flags]
    public enum DisconnectReason : Int16
    {
        None = 0,
        UnhandledException = 2,
        InnerException = 4,
        NormalClosed = 8,
        ConnectionFailed = 16,
        TimeoutExpired = 32,
        ClosedRemoteSide = 64,
        BufferOverflow = 128
    }
    [Flags]
    internal enum DisconnectSide : Int16
    {
        ClientSide = 0, 
        ServerSide = 1
    }
}
