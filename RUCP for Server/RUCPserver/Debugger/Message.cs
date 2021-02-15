﻿using RUCP.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Debugger
{
    public class Message
    {
        public long Time { get; private set; }
        public string Text { get; private set; }
        public string StackTrace { get; private set; } = null;
        public MsgType Type { get; private set; }

        internal Message(string text, string stackTrace = null, MsgType type = MsgType.INFO)
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
                    Terminal.Print(ToString(), ConsoleColor.White);
                    break;
                case MsgType.WARNING:
                    Terminal.Print(ToString(), ConsoleColor.Yellow);
                    break;
                case MsgType.ERROR:
                    Terminal.Print(ToString(), ConsoleColor.Red);
                    break;
            }
        }
    }
}
