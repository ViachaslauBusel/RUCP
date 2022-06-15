using System;

namespace RUCP
{
    public partial class Packet 
    {
     //   private const int MAX_POOL_SIZE = 10_000;
        private static volatile Packet m_head = null;
        private static Object m_poolLocker = new Object();
      //  private static volatile int m_poolSize = 0;

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
            //////Cannot insert in pool a packet that is in the send buffer
            if (m_sendCicle != 0) return;
            ForcedDispose();
        }

        internal void ForcedDispose()
        {
            //  Console.WriteLine($"пакет:[{Sequence}]->Освобожден");
            lock (m_poolLocker)
            {
                //The packet is already in the pool
                if (m_inPool) return;
                m_dataAccess = DATA.Access.Lock;
                m_inPool = true;
                m_next = m_head;
                m_head = this;
            }
        }
       

        //internal void Hold()
        //{
        //    m_inPool = true;
        //    m_dataAccess = DATA.Access.Lock;
        //}

        //internal void Unhold()
        //{
        //    m_inPool = false;
        //}

    }
}
