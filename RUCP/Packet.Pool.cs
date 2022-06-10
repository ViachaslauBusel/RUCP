using System;

namespace RUCP
{
    public partial class Packet 
    {
        private static Packet m_head = null;
        private static Object m_poolLocker = new Object();

        protected volatile bool m_inPool = false;
        private Packet m_next;
       


        internal static bool TryTakeFromPool(out Packet packet)
        {
            //lock (m_poolLocker)
            //{
            //    packet = m_head;
            //    m_head = m_head?.m_next;
            //}
            //if (packet != null)
            //{
            //    packet.m_inPool = false;
            //    packet.Reset();
            //    return true;
            //}
            packet = null;
            return false;
        }

        /// <summary>
        /// Insert the packet to the packet pool.
        /// </summary>
        public void Dispose()
        {
            //////Cannot insert in pool a packet that is in the send buffer
            //if (m_sendCicle != 0) return;
            //ForcedDispose();
        }

        internal void ForcedDispose()
        {
            ////  Console.WriteLine($"пакет:[{Sequence}]->Освобожден");
            //lock (m_poolLocker)
            //{
            //    //The packet is already in the pool
            //    if (m_inPool) return;
            //    m_dataAccess = DATA.Access.Lock;
            //    m_inPool = true;
            //    m_next = m_head;
            //    m_head = this;
            //}
        }
       

    }
}
