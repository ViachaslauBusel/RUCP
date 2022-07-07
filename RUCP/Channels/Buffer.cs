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
		private volatile int m_sequenceConfirm = 0, m_sequenceSent = 0;



		internal Buffer(int size)
		{
			m_sentPackages = new Packet[size];
		}

		internal void Tick()
        {
			lock (m_sentPackages)
            {
				for(int i = m_sequenceConfirm; i != m_sequenceSent; i = (i + 1) % SEQUENCE_WINDOW_SIZE)
                {
					int index = i % m_sentPackages.Length;

					if (m_sentPackages[index] == null) continue;

					Client client = m_sentPackages[index].Client;
					//TODO fix bug -> Something went wrong/ Client cannot be null 
					if (client == null) {  continue; }
					if (!client.isConnected())
                    {
						m_sentPackages[index].ForcedDispose();
						m_sentPackages[index] = null;

						continue;
					}


                    //If the waiting time for confirmation of receipt of the package by the client exceeds timeout, disconnect the client
                    if (m_sentPackages[index].CalculatePing() > m_sentPackages[index].Client.Server.Options.DisconnectTimeout)
                    {
                        Console.WriteLine($"[!]Disconect time:{m_sentPackages[index].CalculatePing()},  SentPackets:{m_sentPackages[index].Client.Statistic.SentPackets}, ResentPackets:{m_sentPackages[index].Client.Statistic.ResentPackets}");

						m_sentPackages[index].Client.CloseConnection();
						m_sentPackages[index].ForcedDispose();
						m_sentPackages[index] = null;
	
						continue;
                    }

                    m_sentPackages[index].Resend();
				}
            }

		}
		/// <summary>
		/// Подтверждение о принятии пакета клиентом
		/// </summary>
		/// <param name="sequence"></param>
		public void ConfirmAsk(int sequence)
		{
			lock (m_sentPackages)
			{
				
				int index = sequence % m_sentPackages.Length;
				if (m_sentPackages[index] != null && m_sentPackages[index].Sequence == sequence)
				{
					if (m_sequenceConfirm == sequence)
					{ m_sequenceConfirm = (m_sequenceConfirm + 1) % SEQUENCE_WINDOW_SIZE; }
					//Console.WriteLine($"пакет:[{sequence}]->ACK подвержден");
					m_sentPackages[index].Client.Statistic.Ping = m_sentPackages[index].CalculatePing();
					m_sentPackages[index].ACK = true;
					m_sentPackages[index].ForcedDispose();
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
			
				int index = m_sequenceSent % m_sentPackages.Length;
				//Если пакет в буффере еще не подтвержден и требует переотправки
				if (m_sentPackages[index] != null)
				{
					throw new BufferOverflowException($"[{(packet.Client.isRemoteHost ? "client" : "server")}]send buffer overflow. Try sent sequence:{m_sequenceSent}, in buffer sequence:{m_sentPackages[index].Sequence}, ch:{m_sentPackages[index].TechnicalChannel} time:{m_sentPackages[index].CalculatePing()}");
				}
				packet.Sequence = (ushort)m_sequenceSent;
				m_sentPackages[index] = packet;

				m_sequenceSent = (m_sequenceSent + 1) % SEQUENCE_WINDOW_SIZE;


				//Console.WriteLine($"пакет:[{packet.Sequence}]->отправлен");
			}
		}

		internal void Dispose()
		{
			Tick();
		}
	}
}
