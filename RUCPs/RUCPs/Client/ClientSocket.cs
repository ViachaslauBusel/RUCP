/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.BufferChannels;
using RUCPs.Cryptography;
using RUCPs.Debugger;
using RUCPs.Packets;
using RUCPs.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCPs.Client
{
   public class ClientSocket
    {

		private BufferQueue bufferQueue;
		private BufferReliable bufferReliable;
		private BufferDiscard bufferDiscard;

		public IPEndPoint Address { get; private set; }
		private IProfile profile;
		
		private int m_devRTT = 0;//Средняя время колебаний задержек пакетов
		private int m_estimatedRTT = 500;

		public long ID { get; private set; }
		private bool online = false;

		public long CheckingTime = 0;//Время последней проверки соеденение

		internal RSA CryptographerRSA { get; set; } = new RSA();
		internal AES CryptographerAES { get; set; } = new AES();



	internal ClientSocket(IPEndPoint address, IProfile profile)
		{
			bufferReliable = new BufferReliable(500);
			bufferQueue = new BufferQueue(this, 500);
			bufferDiscard = new BufferDiscard(this, 500);
			this.Address = address;
			this.profile = profile;
			ID = SocketInformer.GetID(address);
		}

		/// <summary>
		/// Время до повторной отправки неподтвержденных пакетов 
		/// </summary>
		public int GetTimeoutInterval() => Ping + 5 * ((m_devRTT < 4) ? 4 : m_devRTT);

		public int Ping
		{
			get { return m_estimatedRTT; }
			set
			{
				m_devRTT = (int)(m_devRTT * 0.75 + Math.Abs(value - m_estimatedRTT) * 0.25);
				m_estimatedRTT = (int)(m_estimatedRTT * 0.875 + value * 0.125);
			}
		}

		/// <summary>
		/// Передает пакеты в Обрабатывающий класс
		/// </summary>
		internal void HandlerPack(Packet packet)
		{
			if (packet.Encrypt) CryptographerAES.Decrypt(packet);
			if (online) profile.ChannelRead(packet);
		}


		public bool isConnected()
		{
			return online;
		}

		internal void OpenConnection()
		{
			online = true;
			profile.OpenConnection();
		}

		internal void checkingConnection()
		{
			profile.CheckingConnection();
		}
		/// <summary>
		/// Removing a client from the list of connections and calling the CloseConnection method in the profile
		/// </summary>
		public void CloseConnection(bool disconnect = true)
		{
			if(disconnect)
			   Disconnect();
			if (ClientList.RemoveClient(ID))
			{
				profile.CloseConnection();

				online = false;

				bufferReliable.Dispose();
				bufferQueue.Dispose();
				bufferDiscard.Dispose();

				CryptographerAES.Dispose();
				CryptographerRSA.Dispose();
			}
		}

		/// <summary>
		/// Отсылает клиенту команду на отключения
		/// </summary>
		internal void Disconnect()
		{
			 Packet.Create(this, Channel.Disconnect).Send();
		}
		private void SendACK(Packet packet, int channel)
		{
			Packet packet1 = Packet.Create(packet.Client, channel);
			packet1.WriteNumber((ushort)packet.ReadNumber());
			packet1.Send();
		}

		//Подтверждение о принятии пакета клиентом
		internal void ConfirmReliableACK(int number) { bufferReliable.ConfirmAsk(number); }
		internal void ConfirmQueueACK(int number) { bufferQueue.ConfirmAsk(number); }
		internal void ConfirmDiscardACK(int number) { bufferDiscard.ConfirmAsk(number); }

		/// <summary>
		/// Вставка в буффер не подтвержденных пакетов
		/// </summary>
		internal bool InsertBuffer(Packet packet)
		{
            switch (packet.Channel)
            {
				case Channel.Reliable:
					bufferReliable.Insert(packet);
					return true;
				case Channel.Queue:
					bufferQueue.Insert(packet);
					return true;
				case Channel.Discard:
					bufferDiscard.Insert(packet);
					return true;

				default: return false;
			}
		}

		
		//Обработка пакетов
		internal void ProcessReliable(Packet packet)
		{
			//Отправка ACK>>
			SendACK(packet, Channel.ReliableACK);
			//Отправка ACK<<
			if (bufferReliable.Check(packet))
				HandlerPack(packet);
		}
		internal void ProcessQueue(Packet packet)
		{
			//Отправка ACK>>
			SendACK(packet, Channel.QueueACK);
			//Отправка ACK<<
			bufferQueue.Check(packet);
		}
		internal void ProcessDiscard(Packet packet)
		{
			//Отправка ACK>>
			SendACK(packet, Channel.DiscardACK);
			//Отправка ACK<<
			bufferDiscard.Check(packet);
		}

	}
}
