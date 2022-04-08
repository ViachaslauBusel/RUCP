using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
	internal class Buffer
	{
		/// <summary>
		/// размер окна нумерации пакетов
		/// </summary>
		internal const int SEQUENCE_WINDOW_SIZE = 65_536;
		internal const int HALF_NUMBERING_WINDOW_SIZE = SEQUENCE_WINDOW_SIZE / 2;

		/// <summary>Буффер для хранения отправленных пакетов</summary>
		private Packet[] m_sentPackages;
		/// <summary>Порядковый номер отправляемого пакета</summary>
		private volatile int m_numberSent = 0;



		internal Buffer(int size)
		{
			m_sentPackages = new Packet[size];
		}

		/// <summary>
		/// Подтверждение о принятии пакета клиентом
		/// </summary>
		/// <param name="number"></param>
		public void ConfirmAsk(int number)
		{
			lock (m_sentPackages)
			{
				int index = number % m_sentPackages.Length;
				if (m_sentPackages[index] != null && m_sentPackages[index].Sequence == number)
				{
					m_sentPackages[index].ACK = true;
					m_sentPackages[index].Client.Network.Ping = m_sentPackages[index].CalculatePing();
					m_sentPackages[index] = null;
				}
			}
		}
		/// <summary>
		/// Вставка в буффер не подтвержденных пакетов
		/// </summary>
		internal void Insert(Packet packet)
		{
			lock (m_sentPackages)
			{
			
				int index = m_numberSent % m_sentPackages.Length;
				//Если пакет в буффере еще не подтвержден и требует переотправки
				if (m_sentPackages[index] != null)
				{
					throw new BufferOverflowException("send buffer overflow");
				}
				packet.Sequence = (ushort)m_numberSent;
				m_sentPackages[index] = packet;

				m_numberSent = (m_numberSent + 1) % SEQUENCE_WINDOW_SIZE;
			}
		}

		internal void Dispose()
		{
			//The resender will release these packets
			//for (int i = 0; i < receivedPackages.Length; i++)
			//	receivedPackages[i]?.Dispose();
		}
	}
}
