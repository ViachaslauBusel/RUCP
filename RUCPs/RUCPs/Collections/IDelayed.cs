/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCPs.Collections
{
    public interface IDelayed
    {
        /// <summary>
        /// Оставшееся время до переотправки
        /// </summary>
        long GetDelay();
        /// <summary>
        /// Время переотправки пакета
        /// </summary>
        long ResendTime { get; }
    }
}
