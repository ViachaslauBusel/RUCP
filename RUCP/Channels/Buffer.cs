using System;

namespace RUCP.Channels
{
    internal class Buffer
	{
	
		/// <summary>
		/// размер окна нумерации пакетов
		/// </summary>
		internal const int SEQUENCE_WINDOW_SIZE = 65_536;
		internal const int HALF_NUMBERING_WINDOW_SIZE = SEQUENCE_WINDOW_SIZE / 2;

		protected Client m_owner;

		/// <summary>Буффер для хранения отправленных пакетов</summary>
		private Packet[] m_sentPackages;
		/// <summary>Порядковый номер отправляемого пакета</summary>
		private volatile int m_sequenceConfirm = 0, m_sequenceSent = 0;



		internal Buffer(Client client, int size)
		{
			m_owner = client;
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

                    //If the client is disconnected, dispose all packets in the buffer without sending
                    if (!m_owner.isConnected())
                    {
                        m_sentPackages[index].Dispose();
                        m_sentPackages[index] = null;

                        continue;
                    }


                    //If the waiting time for confirmation of receipt of the package by the client exceeds timeout, disconnect the client
                    if (m_sentPackages[index].CalculatePing() > m_owner.Server.Options.DisconnectTimeout)
                    {
                        Console.WriteLine($"[!]Disconect time:{m_sentPackages[index].CalculatePing()},  SentPackets:{m_owner.Statistic.SentPackets}, ResentPackets:{m_owner.Statistic.ResentPackets}");

						m_owner.CloseConnection(DisconnectReason.TimeoutExpired);
						//m_sentPackages[index].Dispose();
						//m_sentPackages[index] = null;
	
						return;
                    }

					if (m_sentPackages[index].GetDelay() <= 0)
					{
						//Console.WriteLine("resend");
						m_sentPackages[index].WriteSendTime(m_owner.Statistic.GetTimeoutInterval());
						m_owner.Stream.Write(m_sentPackages[index]);
						m_owner.Statistic.ResentPackets++;
					}
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
					//If this is the expected packet sequence number, reduce the window of unacknowledged packets
					if (m_sequenceConfirm == sequence)
					{ m_sequenceConfirm = (m_sequenceConfirm + 1) % SEQUENCE_WINDOW_SIZE; }

					m_owner.Statistic.Ping = m_sentPackages[index].CalculatePing();

					m_sentPackages[index].Dispose();
					m_sentPackages[index] = null;
				}
			}
		}
		/// <summary>
		/// Insert in buffer of unacknowledged packets to resend in case of loss
		/// </summary>
		internal void Insert(Packet packet)
		{
			lock (m_sentPackages)
			{

				packet.WriteSendTime(m_owner.Statistic.GetTimeoutInterval()); ;

				int index = m_sequenceSent % m_sentPackages.Length;
				//If the packet in the buffer has not yet been acknowledged and needs to be resent
				if (m_sentPackages[index] != null)
				{
					throw new BufferOverflowException($"[{(m_owner.isRemoteHost ? "client" : "server")}]send buffer overflow. Try sent sequence:{m_sequenceSent}, in buffer sequence:{m_sentPackages[index].Sequence}, ch:{m_sentPackages[index].TechnicalChannel} time:{m_sentPackages[index].CalculatePing()}");
				}
				packet.Sequence = (ushort)m_sequenceSent;
				m_sentPackages[index] = packet;

				m_sequenceSent = (m_sequenceSent + 1) % SEQUENCE_WINDOW_SIZE;
			}
		}

		internal void Dispose()
		{
			Tick();
		}
	}
}
