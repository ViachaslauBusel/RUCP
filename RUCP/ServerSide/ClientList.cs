using RUCP.Tools;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

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

        public void Dispose()
        {
           
        }

        public bool MoveNext()
        {

				Current = m_currentSlot?.Client;
				m_currentSlot = m_currentSlot?.PrevSlot;
			
			
			return Current != null;
        }

        public void Reset()
        {
        
        }
    }
	internal sealed class ClientSlot
	{
		public Client Client { get; set; }
		public ClientSlot NextSlot { get; set; }
		public ClientSlot PrevSlot { get; set; }
	}
	internal sealed class ClientList : IEnumerable<Client>	
    {
	
		private IServer m_master;
		private ClientSlot m_head;
		private object m_headLock = new object();
		/***
	   * Список всех подключенных сокетов(Клиентов)
	   */
		private ConcurrentDictionary<long, ClientSlot> m_list_client = new ConcurrentDictionary<long, ClientSlot>();

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
			if(m_list_client.TryAdd(client.ID, slot))
            {
                lock (m_headLock)
                {
					if(m_head != null) m_head.NextSlot = slot;
					slot.PrevSlot = m_head;
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
			return m_list_client.ContainsKey(key);
		}


		public Client GetClient(IPEndPoint endPoint)
        {
			long id = SocketInformer.GetID(endPoint);
			if (m_list_client.TryGetValue(id, out ClientSlot slot)) { return slot.Client; }
			return new Client(m_master, endPoint);
		}
		/// <summary>
		/// Удвляет из списка клиента, если такого клиента нет в списке возврощает false
		/// </summary>
		internal bool RemoveClient(Client client)
		{
			 if(m_list_client.TryRemove(client.ID, out ClientSlot slot))
            {
				if(m_head == slot) { m_head = m_head.PrevSlot; }

				if (slot.NextSlot != null) { slot.NextSlot.PrevSlot = slot.PrevSlot; }
				if (slot.PrevSlot != null) { slot.PrevSlot.NextSlot = slot.NextSlot; }
				slot.NextSlot = null;
				slot.PrevSlot = null;
				return true;
            }
			return false;
		}

		public int Count => m_list_client.Count;
		

        public IEnumerator<Client> GetEnumerator() => new ClientEnumerator(m_head);


        IEnumerator IEnumerable.GetEnumerator() => new ClientEnumerator(m_head);

	}
}
