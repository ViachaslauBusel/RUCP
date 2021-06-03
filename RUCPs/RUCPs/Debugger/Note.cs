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
        public DateTime Time { get; private set; }
        public string Text { get; private set; }
        public string StackTrace { get; private set; } = null;
        public MsgType Type { get; private set; }

        internal Note(string text, string stackTrace = null, MsgType type = MsgType.INFO)
        {
            Time = DateTime.Now;
            Text = text;
            StackTrace = stackTrace;
            Type = type;
        }
        public override string ToString()
        {

            if (string.IsNullOrEmpty(StackTrace))
                return $"[{Format(Time)}] {Text}";
            else return $"[{Format(Time)}] {Text} \n {StackTrace}";
        }
        private string Format(DateTime t) => t.Hour.ToString("00")+":"+ t.Minute.ToString("00")+":"+ t.Second.ToString("00");
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
