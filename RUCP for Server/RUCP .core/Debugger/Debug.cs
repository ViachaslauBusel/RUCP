/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Debugger
{
    class Debug
    {
        private static DebugObject debugObject;
        public static void init(DebugObject obj)
        {
            debugObject = obj;
        }
        public static void logError(String className, String error, string trace)
        {
            try
            {
                Console.Error.WriteLine(error);
                //debugObject?.logError(className, error, trace);
            }
            catch { }
        }
    }
}
