using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.BufferChannels
{
    public class BufferOverflowException: Exception
    {
        public BufferOverflowException(string message) : base(message)
        {

        }
    }
}
