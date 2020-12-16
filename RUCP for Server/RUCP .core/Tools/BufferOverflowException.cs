using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Tools
{
    class BufferOverflowException: Exception
    {
        public BufferOverflowException(string message) : base(message)
        {

        }
    }
}
