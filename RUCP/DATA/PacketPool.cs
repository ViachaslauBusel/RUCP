using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RUCP.DATA
{
    internal static class PacketPool
    {
        private static Packet m_head = null;
        private static Object m_locker = new Object();


        internal static bool TryTake(out Packet packet)
        {
            lock (m_locker)
            {
                packet = m_head;
                m_head = m_head?.Next;
            }
            return packet != null;
        }

        internal static void Insert(Packet packet)
        {
            lock (m_locker)
            {
                packet.Next = m_head;
                m_head = packet;
            }
        }
    }
}
