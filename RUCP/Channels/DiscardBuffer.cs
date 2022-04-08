using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
    internal class DiscardBuffer : Buffer
    {
		struct DiscardNode
		{
			public ushort Sequence { get; set; }
			public short Type { get; private set; }

			public void Set(Packet packet)
			{
				Sequence = packet.Sequence;
				Type = (short)packet.ReadType();
			}
		}
		private Client m_master;
		//Буфер для хранения полученных пакетов, хранит порядковый номер полученного пакета
		private DiscardNode[] m_receivedPackages;
		/// <summary>Ожидаемый порядковый номер получаемого пакета</summary>
		private volatile int m_nextExpectedSequenceNumber = 0;

		internal DiscardBuffer(Client client, int size) : base(size)
		{
			this.m_master = client;
			m_receivedPackages = new DiscardNode[size];
			m_receivedPackages[0].Sequence = ushort.MaxValue;
		}


		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal bool Check(Packet packet)
		{
			//try
			//{

				ushort sequence = packet.Sequence;//Порядковый номер принятого пакета
				int bufferIndex = sequence % m_receivedPackages.Length;//Порядковый номер в буфере
			

				lock (m_receivedPackages)
				{
					int relative = NumberUtils.RelativeSequenceNumber(sequence, m_receivedPackages[bufferIndex].Sequence);
					//Буфер приема переполнен пока невозможно принять этот пакет
					if (relative > m_receivedPackages.Length) return false;
					//Если пакет еще не был принят
					if (relative > 0)
					{
						m_receivedPackages[bufferIndex].Set(packet);
						//Discard >>
						int compare = NumberUtils.RelativeSequenceNumber(sequence, m_nextExpectedSequenceNumber);
						if (compare >= 0)// Пакет пришел первым
						{
							m_nextExpectedSequenceNumber = sequence;
						}
						// Пакет пришел не первым, ищем пакеты с таким же типом, если они есть, отбрасываем этот пакет
						else
						{
							int packetType = packet.ReadType();
							for (int x = (sequence + 1) % SEQUENCE_WINDOW_SIZE; NumberUtils.RelativeSequenceNumber(x, m_nextExpectedSequenceNumber) <= 0; x = (x + 1) % SEQUENCE_WINDOW_SIZE)//Перебор пакетов пришедших после
							{
								int xBuffer = x % m_receivedPackages.Length;
						
								//Если пакет совпадает по типу и был отправлен после этого пакета, пакет не подлежит обработке
								if (packetType == m_receivedPackages[xBuffer].Type && NumberUtils.RelativeSequenceNumber(m_receivedPackages[xBuffer].Sequence, sequence) > 0)
								{ return true; }
							}
						}
						//Discard <<
						m_master.HandlerPack(packet);
					}
				}
				return true;
			//}
			//catch (Exception e)
			//{
			//	m_master.Server.CallException(e);
			//}
		}
	}
}
