using RUCPs.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Logger
{
    internal class Note
    {
        internal DateTime Time { get; private set; }
        internal string Text { get; private set; }
        internal string StackTrace { get; private set; } = null;
        internal MsgType Type { get; private set; }
        public string Thread { get; private set; }

        internal Note(string text, string stackTrace, string thread, MsgType type)
        {
            Time = DateTime.Now;
            Text = text;
            StackTrace = stackTrace;
            Thread = thread;
            Type = type;
        }

        private string Format(DateTime t) => t.Hour.ToString("00")+":"+ t.Minute.ToString("00")+":"+ t.Second.ToString("00");

        internal string FormatMessage(string format)
        {
            string formatMsg = format.Replace("%date", Time.ToString());
            formatMsg = formatMsg.Replace("%thread", Thread);
            formatMsg = formatMsg.Replace("%stacktrace", StackTrace);
            formatMsg = formatMsg.Replace("%message", Text);
            return formatMsg;
        }

    }
}
