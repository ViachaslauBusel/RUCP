using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
    internal class QueueBuffer : Buffer
    {
	    struct QueueNode
		{
			public ushort Sequence { get; set; }
			public Packet Value { get; private set; }

			public void Set(Packet packet)
			{
				Sequence = packet.Sequence;
				Value = packet;
			}

			internal void Free()
			{
				Value = null;
			}
		}

		private Client m_master;
		//Буфер для хранения полученных пакетов, хранит порядковый номер полученного пакета
		private QueueNode[] m_receivedPackages;
		/// <summary>Ожидаемый порядковый номер получаемого пакета</summary>
		private volatile int m_nextExpectedSequenceNumber = 0;


		internal QueueBuffer(Client client, int size) : base(size)
		{
			this.m_master = client;
			m_receivedPackages = new QueueNode[size];
			m_receivedPackages[0].Sequence = ushort.MaxValue;
		}

		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal bool Check(Packet packet)
		{
			int sequence = packet.Sequence;//Порядковый номер принятого пакета
			int index = sequence % m_receivedPackages.Length;//Порядковый номер в буфере

			

			lock (m_receivedPackages)
			{
				int relative = NumberUtils.RelativeSequenceNumber(sequence, m_receivedPackages[index].Sequence);
				//Буфер приема переполнен пока невозможно принять этот пакет
				if (relative > m_receivedPackages.Length) return false;
				//Если пакет еще не был принят
				// Если пакет в буфере  был отправлен после записаного в буфер
				if (relative > 0)
				{
					m_receivedPackages[index].Set(packet);

					while (IsExpectedSequence(m_receivedPackages[index].Sequence))//Если номер пакета совпадает с ожидаемым
					{
						m_master.HandlerPack(m_receivedPackages[index].Value);//Обрабатываем, и смотрим в перед есть ли еще не обработанные пакеты
						m_receivedPackages[index].Free();

						index = (index + 1) % m_receivedPackages.Length;//Порядковый номер следующего пакета в буфере
					}
				}
			}
			return true;
		}

		private bool IsExpectedSequence(int sequence)
		{

			//	int compare = NumberUtils.ShortCompare(number, comingNumber);
			if (sequence == m_nextExpectedSequenceNumber)// Пакет пришел в нужном порядке
			{
				m_nextExpectedSequenceNumber = (m_nextExpectedSequenceNumber + 1) % SEQUENCE_WINDOW_SIZE;

				return true;
			}
			return false;
		}
	}
}
