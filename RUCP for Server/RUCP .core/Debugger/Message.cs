using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Debugger
{
    class Message
    {
        public Message(string className, string message)
        {
            Name = className;
            Msg = message;
        }

        public string Name { get; private set; }
        public string Msg { get; private set; }
    }
}
