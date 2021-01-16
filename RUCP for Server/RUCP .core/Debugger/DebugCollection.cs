/* BSD 3-Clause License
 *
 * Copyright (c) 2020-2021, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Debugger
{
    class DebugCollection : DebugObject
    {
        public ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
        public ConcurrentQueue<Message> Messages { get; } = new ConcurrentQueue<Message>();
        public void Log(string className, string message)
        {
            Messages.Enqueue(new Message(className, message));
        }

        public void LogError(Exception exception)
        {
            Errors.Enqueue(exception);
        }
    }
}
