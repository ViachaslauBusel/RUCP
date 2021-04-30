/* BSD 3-Clause License
 *
 * Copyright (c) 2020-2021, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RUCPs.Debugger
{
    public class Debug
    {
        public static ConcurrentQueue<Note> Messages { get; } = new ConcurrentQueue<Note>();
        internal static event Action<Note> log;

        public static void Log(Exception exception)
        {
            Note _m = new Note(exception.Message, exception.StackTrace, MsgType.ERROR);
            Messages.Enqueue(_m);
            log?.Invoke(_m);
        }
        public static void Log(string message, MsgType type = MsgType.INFO)
        {
            Note _m = new Note(message, type: type);
            Messages.Enqueue(_m);
            log?.Invoke(_m);
        }
    }
}
