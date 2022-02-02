using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public class NetworkInfo
    {
        /// <summary>Среднее время колебаний задержек между отправкой пакета и получении подтверждения об доставке пакета</summary>
        private int m_devRTT = 0;
        /// <summary>Среднее значение времени задержек между отправкой пакета и получении подтверждения об доставке пакета</summary>
        private int m_estimatedRTT = 500;
        /// <summary>Количество отправленных пакетов</summary>
        private int m_sentPackets = 0;
        /// <summary>Количество переотправленных пакетов</summary>
        private int m_resentPackets = 0;
        /// <summary>Количество отправленных пакетов, для подсчета потерянных пакетов</summary>
        private int m_pl_sentPackets = 0;
        /// <summary>Количество переотправленных пакетов, для подсчета потерянных пакетов</summary>
        private int m_pl_resentPackets = 0;
        private int counter = 0;


        public NetworkStatus Status { get; internal set; } = NetworkStatus.LISTENING;

        /// <summary>
        /// Количество отправленных пакетов по надежным каналас
        /// </summary>
        public int SentPackets
        {
            get => m_sentPackets;
            internal set
            {
                m_sentPackets = value;
                m_pl_sentPackets++;
                if (++counter >= 25)
                {
                    m_pl_sentPackets = 0;
                    m_pl_resentPackets = 0;
                }
            }
        }


        /// <summary>
        /// Количество повторно отправленных пакетов
        /// </summary>
        public int ResentPackets
        {
            get => m_resentPackets;
            internal set
            {
                m_resentPackets = value;
                m_pl_resentPackets++;
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
                if (m_pl_sentPackets == 0) return 0.0f;
                return (m_pl_resentPackets / (float)m_pl_sentPackets) * 100.0f;
            }
        }

        /// <summary>
        /// Время до повторной отправки пакета при патери пакетов
        /// </summary>
        internal int GetTimeoutInterval() => m_estimatedRTT + 3 * ((m_devRTT < 4) ? 4 : m_devRTT);

        public int Ping
        {
            get => m_estimatedRTT;
            internal set
            {
                m_devRTT = (int)(m_devRTT * 0.75 + Math.Abs(value - m_estimatedRTT) * 0.25);
                m_estimatedRTT = (int)(m_estimatedRTT * 0.875 + value * 0.125);
            }
        }
        internal void InitPing(int ping)
        {
            m_devRTT = 0;
            m_estimatedRTT = ping;
        }
    }
}
