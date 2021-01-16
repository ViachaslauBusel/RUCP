/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;

namespace RUCP.Debugger
{
    internal interface DebugObject
    {
        public void LogError(Exception exception);
        public void Log(string className, string message);
    }
}