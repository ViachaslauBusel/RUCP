﻿/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPc.Network
{
    public class NetworkInfo
    {
        /// <summary>Среднее время колебаний задержек между отправкой пакета и получении подтверждения об доставке пакета</summary>
        private int m_devRTT = 0;
        /// <summary>Среднее значение времени задержек между отправкой пакета и получении подтверждения об доставке пакета</summary>
        private int m_estimatedRTT = 500;
        private int sentPackets = 0;
        private int resentPackets = 0;
        private int pl_sentPackets = 0;
        private int pl_resentPackets = 0;
        private int counter = 0;
        /// <summary>
        /// Количество отправленных пакетов по надежным каналас
        /// </summary>
        public int SentPackets
        {
            get => sentPackets;
            internal set
            {
                sentPackets = value;
                pl_sentPackets++;
                if (++counter >= 25)
                {
                    pl_sentPackets = 0;
                    pl_resentPackets = 0;
                }
            }
        }


        /// <summary>
        /// Количество повторно отправленных пакетов
        /// </summary>
        public int ResentPackets
        {
            get => resentPackets;
            internal set
            {
                resentPackets = value;
                pl_resentPackets++;
                counter = 0;
            }
        }

        /// <summary>
        /// Packet loss percentage
        /// </summary>
        public float PacketLoss
        {
            get
            {
                if (pl_sentPackets == 0) return 0.0f;
                return (pl_resentPackets / (float)pl_sentPackets) * 100.0f;
            }
        }

        /// <summary>
        /// Время до повторной отправки пакета при патери пакетов
        /// </summary>
        public int GetTimeoutInterval() => m_estimatedRTT + 3 * ((m_devRTT < 4) ? 4 : m_devRTT);

        public int Ping => m_estimatedRTT;
        internal void InitPing(int ping)
        {
            m_devRTT = 0;
            m_estimatedRTT = ping;
        }

        internal void SetPing(int ping)
        {
            m_devRTT = (int)(m_devRTT * 0.75 + Math.Abs(ping - m_estimatedRTT) * 0.25);
            m_estimatedRTT = (int)(m_estimatedRTT * 0.875 + ping * 0.125);
        }
    }
}
