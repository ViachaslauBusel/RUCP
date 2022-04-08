using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
    internal class ReliableBuffer : Buffer
    {
		private Client m_master;

		//Буфер для хранения полученных пакетов, хранит порядковый номер полученного пакета
		private ushort[] m_receivedPackages;
		internal ReliableBuffer(Client client, int size) : base(size)
		{
			m_master = client;
			m_receivedPackages = new ushort[size];
			m_receivedPackages[0] = ushort.MaxValue;
		}


		/// <summary>
		/// Проверка подлежит ли этот полученный пакет обработке
		/// </summary>
		internal bool Check(Packet pack)
		{
			int sequence = pack.Sequence;//Порядковый номер принятого пакета
			int index = sequence % m_receivedPackages.Length;//Порядковый номер в буфере

			lock (m_receivedPackages)
			{
				int relative = NumberUtils.RelativeSequenceNumber(sequence, m_receivedPackages[index]);
				//Буфер приема переполнен пока невозможно принять этот пакет
				if(relative > m_receivedPackages.Length) return false;
				//Если пакет еще не был принят
				// Если принятый пакет был отправлен после чем пакет записанный в буффер
				if (relative > 0)
				{
				//	receivedPackages[index]?.Dispose();
					m_receivedPackages[index] = (ushort)sequence;

					m_master.HandlerPack(pack);
				}

			}
			return true;
		}
	}
}
