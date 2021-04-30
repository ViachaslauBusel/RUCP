using RUCPs.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Debugger
{
    public class Note
    {
        public long Time { get; private set; }
        public string Text { get; private set; }
        public string StackTrace { get; private set; } = null;
        public MsgType Type { get; private set; }

        internal Note(string text, string stackTrace = null, MsgType type = MsgType.INFO)
        {
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Text = text;
            StackTrace = stackTrace;
            Type = type;
        }
        public override string ToString()
        {
            TimeSpan t = TimeSpan.FromMilliseconds(Time);
            if (string.IsNullOrEmpty(StackTrace))
                return $"[{Format(t)}] {Text}";
            else return $"[{Format(t)}] {Text} \n {StackTrace}";
        }
        private string Format(TimeSpan t) => t.Hours.ToString("00")+":"+ t.Minutes.ToString("00")+":"+ t.Seconds.ToString("00");
        internal void Print()
        {
            switch (Type)
            {
                case MsgType.INFO:
                    Terminal.PrintLine(ToString(), ConsoleColor.White);
                    break;
                case MsgType.WARNING:
                    Terminal.PrintLine(ToString(), ConsoleColor.Yellow);
                    break;
                case MsgType.ERROR:
                    Terminal.PrintLine(ToString(), ConsoleColor.Red);
                    break;
            }
        }
    }
}
