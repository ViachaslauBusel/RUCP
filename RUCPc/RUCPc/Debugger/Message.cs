using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPc.Debugger
{
   public class Message
    {
        public string Text { get; private set; }
        public string StackTrace { get; private set; } = null;
        public MsgType Type { get; private set; }

        public Message(string text, string stackTrace = null, MsgType type = MsgType.INFO)
        {
            Text = text;
            StackTrace = stackTrace;
            Type = type;
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(StackTrace))
                return Text;
            else return $"{Text} \n {StackTrace}";
        }
    }
}
