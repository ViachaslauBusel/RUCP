/* BSD 3-Clause License
 *
 * Copyright (c) 2020-2021, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Debugger
{
    class Debug
    {
        private static DebugCollection debugObject = new DebugCollection();

        public static ConcurrentQueue<Exception> Errors => debugObject.Errors;
        public static ConcurrentQueue<Message> Messages => debugObject.Messages;
        /*   public static void init(DebugObject obj)
           {
               debugObject = obj;
           }*/
        public static void LogError(Exception exception)
        {
            debugObject.LogError(exception);
        }
        public static void Log(string name, string msg)
        {
            debugObject.Log(name, msg);
        }
    }
}
