/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Debugger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RUCP
{
    public class Debug
    {
        public static DebugObject Object { get; private set; }

        public static void Start()
        {
            Object = new DebugBuffer();
        }

        internal static void Log(string message)
        {
            Object?.Log(message);
        }

        public static void Stop()
        {
            Object = null;
        }

        internal static void logError(string name, string v, string stackTrace)
        {
            Object?.LogError(name, v, stackTrace);
        }
    }
}
