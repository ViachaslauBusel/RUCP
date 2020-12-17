/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Tools
{
    class BufferOverflowException: Exception
    {
        public BufferOverflowException(string message) : base(message)
        {

        }
    }
}
