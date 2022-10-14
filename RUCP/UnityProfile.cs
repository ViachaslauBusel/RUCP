using RUCP.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public class UnityProfile : BaseProfile
    {
        private ConcurrentQueue<Packet> m_pipeline = new ConcurrentQueue<Packet>();
        private HandlersStorage<Action<Packet>> m_handlersStorage = new HandlersStorage<Action<Packet>>();

        public HandlersStorage<Action<Packet>> Handlers => m_handlersStorage;
        /// <summary>Доступно пакетов для обработки</summary>
        public int AvailablePackets => m_pipeline.Count;

        public override void ChannelRead(Packet pack)
        {
            m_pipeline.Enqueue(pack);
        }

        public override void CheckingConnection()
        {
           
        }

        public override void CloseConnection(DisconnectReason reason)
        {
           // Console.WriteLine($"Client -> Connection closed with result -> {reason}");
        }

        public override bool HandleException(Exception exception)
        {
           // Console.WriteLine($"Client: Exception caught:{exception}");
            return false;
        }

        public override void OpenConnection()
        {
           // Console.WriteLine($"Client -> Connection open");
        }

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
                    m_handlersStorage.GetHandler(packet.OpCode)?.Invoke(packet);
                }
                else return;
            }
        }

      
    }
}
