using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
    public class BufferOverflowException : Exception
    {
        public BufferOverflowException(string message) : base(message)
        {

        }
    }
}
