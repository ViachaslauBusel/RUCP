/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Debugger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RUCP.Debugger
{
    public class Debug
    {
        private static ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();
        internal static void Log(string message, MsgType msgType = MsgType.INFO)
        {
            messages.Enqueue(new Message(message, type:msgType));
        }

        internal static void Log(Exception exception)
        {
            messages.Enqueue(new Message(exception.Message, exception.StackTrace, MsgType.ERROR));
        }

        public static Message GetMessage()
        {
            messages.TryDequeue(out Message message);
            return message;
        }

       
    }
}
