﻿using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Channels
{
    internal sealed class ReliableBuffer : Buffer
    {

		//Буфер для хранения полученных пакетов, хранит порядковый номер полученного пакета
		private ushort[] m_receivedPackages;
		internal ReliableBuffer(Client client, int size) : base(client, size)
		{
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
					m_receivedPackages[index] = (ushort)sequence;
					m_owner.Statistic.ReceivedPackets++;

					m_owner.HandlerPack(pack);
				}
				else m_owner.Statistic.ReacceptedPackets++;
			}
			return true;
		}
	}
}
