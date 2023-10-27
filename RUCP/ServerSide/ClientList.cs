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
		private ReaderWriterLockSlim m_locker;
		public Client Current { get; private set; }

		object IEnumerator.Current => Current;

		internal ClientEnumerator(ClientSlot slot, ReaderWriterLockSlim locker)
		{
			m_currentSlot = slot;
			m_locker = locker;
			Current = null;
			m_locker.EnterReadLock();
		}

		public void Dispose()
		{
			m_locker?.ExitReadLock();
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
	internal sealed class ClientList
    {
		private IServer m_master;
		private ClientSlot m_head;
		private ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();
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
			if(m_clients.TryAdd(client.ID, slot))
            {
                try
                {
					m_locker.EnterWriteLock();
					if (m_head != null) { m_head.NextSlot = slot; }
					slot.PrevSlot = m_head;
					m_head = slot;
                }
				finally { m_locker.ExitWriteLock(); }
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


		public Client GetClient(IPEndPoint endPoint)
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
				try
				{
					m_locker.EnterWriteLock();
					if (m_head == slot) { m_head = m_head.PrevSlot; }

					if (slot.NextSlot != null) { slot.NextSlot.PrevSlot = slot.PrevSlot; }
					if (slot.PrevSlot != null) { slot.PrevSlot.NextSlot = slot.NextSlot; }
					slot.NextSlot = null;
					slot.PrevSlot = null;
				}
				finally { m_locker.ExitWriteLock(); }
				return true;
            }
			return false;
		}

		public int Count => m_clients.Count;

        internal ClientEnumerator CreateEnumerator() => new ClientEnumerator(m_head, m_locker);
    }
}
