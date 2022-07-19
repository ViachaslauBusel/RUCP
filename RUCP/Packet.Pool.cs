using System;

namespace RUCP
{
    public sealed partial class Packet 
    {
        private static volatile Packet m_head = null;
        private static Object m_poolLocker = new Object();

        private volatile bool m_inPool = false;
        private volatile Packet m_next;
       




        internal static bool TryTakeFromPool(out Packet packet)
        {
            lock (m_poolLocker)
            {
                packet = m_head;
                m_head = m_head?.m_next;

                if (packet != null)
                {
              //      m_poolSize--;
                    packet.m_inPool = false;
                    packet.Reset();
                    return true;
                }
            }
           
            packet = null;
            return false;
        }

        /// <summary>
        /// Insert the packet to the packet pool.
        /// </summary>
        public void Dispose()
        {
            lock (m_poolLocker)
            {
                //The packet is already in the pool
                if (m_inPool) return;
                m_dataAcces = DATA.DataAccess.Lock;
                m_inPool = true;
                m_next = m_head;
                m_head = this;
            }
        }
    }
}
