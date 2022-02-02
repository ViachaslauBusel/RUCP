using RUCP.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public class UnityProfile : IProfile
    {
        private ConcurrentQueue<Packet> m_pipeline = new ConcurrentQueue<Packet>();
        private HandlersStorage<Action<Packet>> m_handlersStorage = new HandlersStorage<Action<Packet>>();

        public HandlersStorage<Action<Packet>> Handlers => m_handlersStorage;
        /// <summary>Доступно пакетов для обработки</summary>
        public int AvailablePackets => m_pipeline.Count;

        /// <summary>
        /// Обработать указанное количество пакетов(count) из очереди принятых пакетов
        /// </summary>
        /// <param name="count"></param>
        public void ProcessPacket(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (m_pipeline.TryDequeue(out Packet packet))
                {
                    m_handlersStorage.GetHandler(packet.ReadType())?.Invoke(packet);
                }
                else return;
            }
        }

        void IProfile.ChannelRead(Packet pack)
        {
            m_pipeline.Enqueue(pack);
        }

        void IProfile.CheckingConnection()
        {

        }

        void IProfile.CloseConnection()
        {
          //  Console.WriteLine("CloseConnection");
        }

        void IProfile.OpenConnection()
        {
        //    Console.WriteLine("OpenConnection");
        }
    }
}
