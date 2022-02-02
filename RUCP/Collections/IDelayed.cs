using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Collections
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
