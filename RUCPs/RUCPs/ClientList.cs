/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Client;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RUCPs
{
    public class ClientList: IEnumerable<ClientSocket>
    {
		public static readonly ClientList instance = new ClientList();
		/***
	   * Список всех подключенных сокетов(Клиентов)
	   */
		private static ConcurrentDictionary<long, ClientSocket> list_client = new ConcurrentDictionary<long, ClientSocket>();



		/// <summary>
		/// добовляет в список клиента, если такой клиент уже есть в списке возврощает false
		/// </summary>
		internal static bool AddClient(long id, ClientSocket cl)
		{
			return list_client.TryAdd(id, cl);
		}

		/***
		 * Проверяет есть ли клиент с заданным ид в списке клиетов
		 * @param key
		 * @return
		 */
		public static bool containsKey(long key)
		{
			return list_client.ContainsKey(key);
		}

		/***
		 * Возврощает клиента по ключу, если такого клиента нет возврощает null
		 * @param key
		 * @return
		 */
		public static ClientSocket GetClient(long key)
		{
		    list_client.TryGetValue(key, out ClientSocket clientSocket);
			return clientSocket;
		}
		/// <summary>
		/// Удвляет из списка клиента, если такого клиента нет в списке возврощает false
		/// </summary>
		internal static bool RemoveClient(long id)
		{
			return list_client.TryRemove(id, out ClientSocket client);
		}

		public static int online()
		{
			return list_client.Count;
		}

        public IEnumerator<ClientSocket> GetEnumerator()
        {
			return new ClientListEnumerator(list_client.GetEnumerator());
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
			return new ClientListEnumerator(list_client.GetEnumerator());
        }
    }
}