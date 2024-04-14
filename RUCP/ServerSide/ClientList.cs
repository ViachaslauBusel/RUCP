using RUCP.Tools;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace RUCP.ServerSide
{
	internal struct ClientEnumerator : IEnumerator<Client>
	{
		private ClientSlot m_currentSlot;
		public Client Current { get; private set; }

		object IEnumerator.Current => Current;

		internal ClientEnumerator(ClientSlot slot)
		{
			m_currentSlot = slot;
			Current = null;
		}

		public bool MoveNext()
		{
			if(m_currentSlot == null) return false;

			Current = m_currentSlot.Client;
			m_currentSlot = m_currentSlot.NextSlot;

			return Current != null;
		}

		public void Reset()
		{
		}

        public void Dispose()
        {
        }
    }
	internal sealed class ClientSlot
	{
		public Client Client { get; set; }
		public ClientSlot NextSlot { get; set; }
		public ClientSlot PrevSlot { get; set; }
	}
	internal sealed class ClientList
    {
		private IServer m_master;
		private ClientSlot m_head;
		private object m_locker = new object();
		/***
	   * Список всех подключенных сокетов(Клиентов)
	   */
		private ConcurrentDictionary<long, ClientSlot> m_clients = new ConcurrentDictionary<long, ClientSlot>();

		internal ClientList(IServer server)
        {
			m_master = server;
        }

		/// <summary>
		/// добовляет в список клиента, если такой клиент уже есть в списке возврощает false
		/// </summary>
		internal bool AddClient(Client client)
		{
			if (client.Server != m_master) return false;
			ClientSlot slot = new ClientSlot()
			{
				Client = client
			};
			if (m_clients.TryAdd(client.ID, slot))
			{
				lock (m_locker)
				{
					if (m_head != null) { m_head.PrevSlot = slot; }
					slot.NextSlot = m_head;
					m_head = slot;
				}
				return true;
			}
			return false;
		}

		/***
		 * Проверяет есть ли клиент с заданным ид в списке клиетов
		 * @param key
		 * @return
		 */
		public bool containsKey(long key)
		{
			return m_clients.ContainsKey(key);
		}


		public Client GetOrCreateClient(IPEndPoint endPoint)
        {
			long id = SocketInformer.GetID(endPoint);
			if (m_clients.TryGetValue(id, out ClientSlot slot)) { return slot.Client; }
			return new Client(m_master, endPoint);
		}
        /// <summary>
        /// Удвляет из списка клиента, если такого клиента нет в списке возврощает false
        /// </summary>
        internal bool RemoveClient(Client client)
        {
            if (m_clients.TryRemove(client.ID, out ClientSlot slot))
            {
                lock (m_locker)
				{ 
                    // If the slot to be removed is the head, move the head to the previous slot
                    if (m_head == slot)
                    {
                        m_head = m_head.NextSlot;
                    }

                    // Update the links of the next and previous slots
                    if (slot.PrevSlot != null)
                    {
                        slot.PrevSlot.NextSlot = slot.NextSlot;
                    }
                    if (slot.NextSlot != null)
                    {
                        slot.NextSlot.PrevSlot = slot.PrevSlot;
                    }
                }
                return true;
            }
            return false;
        }

        public int Count => m_clients.Count;

        internal ClientEnumerator CreateEnumerator() => new ClientEnumerator(m_head);
    }
}
