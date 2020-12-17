/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Handler
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerAttribute: Attribute
    {
        public int Number { get; private set; }

        public HandlerAttribute(int number)
        {
            Number = number;
        }
    }
}
