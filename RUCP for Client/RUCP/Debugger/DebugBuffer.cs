/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Debugger
{
    public class DebugBuffer : DebugObject
    {
        private ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        private ConcurrentQueue<(string className, string message, string stackTrace)> errors = new ConcurrentQueue<(string className, string messagem, string stackTrace)>();
        internal override void Log(string message)
        {
            messages.Enqueue(message);
        }

        internal override void LogError(string className, string message, string stackTrace)
        {
            errors.Enqueue((className, message, stackTrace));
        }

        public string GetMessage()
        {
            messages.TryDequeue(out string message);
            return message;
        }

        public (string className, string message, string stackTrace) GetError()
        {
            errors.TryDequeue(out (string className, string message, string stackTrace) error);
            return error;
        }
    }
}
