using RUCP.Tools;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCP.ServerSide
{
    internal class ClientList : IEnumerable<Client>	
    {
		private IServer m_master;
		/***
	   * Список всех подключенных сокетов(Клиентов)
	   */
		private ConcurrentDictionary<long, Client> m_list_client = new ConcurrentDictionary<long, Client>();

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
			return m_list_client.TryAdd(client.ID, client);
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
			if (m_list_client.TryGetValue(id, out Client client)) { return client; }
			return new Client(m_master, endPoint);
		}
		/// <summary>
		/// Удвляет из списка клиента, если такого клиента нет в списке возврощает false
		/// </summary>
		internal bool RemoveClient(Client client)
		{
			return m_list_client.TryRemove(client.ID, out Client _c);
		}

		public int online()
		{
			return m_list_client.Count;
		}

        public IEnumerator<Client> GetEnumerator() => m_list_client.Values.GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator() => m_list_client.Values.GetEnumerator();

	}
}
