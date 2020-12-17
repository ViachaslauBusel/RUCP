/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Network
{
    public class NetworkInfo
    {
        /// <summary>
        /// Количество отправленных пакетов по надежным каналас
        /// </summary>
        public int Send { get; internal set; } = 0;
        /// <summary>
        /// Количество повторно отправленных пакетов
        /// </summary>
        public int Resend { get; internal set; } = 0;
        /// <summary>
        /// Среднее значение времени задержек между отправкой пакета и получении подтверждения об доставке пакета
        /// </summary>
        public int Ping { get; internal set; } = 500;
        /// <summary>
        /// Среднее значение разности времени задержек между отправкой пакета и получении подтверждения об доставке пакета
        /// </summary>
        public int RTT { get; internal set; } = 0;

        /// <summary>
        /// Время до повторной отправки пакета при патери пакетов
        /// </summary>
        public int GetTimeout() => Ping + 5 * ((RTT < 4) ? 4 : RTT);


        internal void SetPing(int ping)
        {
            RTT = (int)(RTT * 0.75 + Math.Abs(ping - Ping) * 0.25);
            Ping = (int)(Ping * 0.875 + ping * 0.125);
        }
    }
}
