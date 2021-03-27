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
        public static ConcurrentQueue<Message> Messages { get; } = new ConcurrentQueue<Message>();
        internal static event Action<Message> log;

        public static void Log(Exception exception)
        {
            Message _m = new Message(exception.Message, exception.StackTrace, MsgType.ERROR);
            Messages.Enqueue(_m);
            log?.Invoke(_m);
        }
        public static void Log(string message, MsgType type = MsgType.INFO)
        {
            Message _m = new Message(message, type: type);
            Messages.Enqueue(_m);
            log?.Invoke(_m);
        }
    }
}
