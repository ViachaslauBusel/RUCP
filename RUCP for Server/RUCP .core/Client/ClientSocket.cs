/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.BufferChannels;
using RUCP.Debugger;
using RUCP.Packets;
using RUCP.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RUCP.Client
{
   public class ClientSocket
    {

		private BufferQueue bufferQueue;
		private BufferReliable bufferReliable;
		private BufferDiscard bufferDiscard;

		public IPEndPoint Address { get; private set; }
		private IProfile profile;
		
		private int rtt = 0;//Средняя время колебаний задержек пакетов
		private int ping = 500;

		public long ID { get; private set; }
		private bool online = false;

		public long CheckingTime = 0;//Время последней проверки соеденение

		

		internal ClientSocket(IPEndPoint address, IProfile profile)
		{
			bufferReliable = new BufferReliable(500);
			bufferQueue = new BufferQueue(this, 500);
			bufferDiscard = new BufferDiscard(this, 500);
			this.Address = address;
			this.profile = profile;
			ID = SocketInformer.GetID(address);
			Ping = 500;
		}

		/// <summary>
		/// Время до повторной отправки неподтвержденных пакетов 
		/// </summary>
		public int GetTimeout() => Ping + 5 * ((rtt < 4) ? 4 : rtt);

		public int Ping
		{
			get { return ping; }
			set
			{
				rtt = (int)(rtt * 0.75 + Math.Abs(value - ping) * 0.25);
				ping = (int)(ping * 0.875 + value * 0.125);
			}
		}

		/// <summary>
		/// Передает пакеты в Обрабатывающий класс
		/// </summary>
		internal void HandlerPack(Packet packet)
		{
			if (online) profile.ChannelRead(packet);
		}


		public bool isConnected()
		{
			return online;
		}

		internal bool openConnection(Packet pack)
		{
			return online = profile.OpenConnection(pack);
		}

		internal void checkingConnection()
		{
			profile.CheckingConnection();
		}
		/// <summary>
		/// Удаление ClientInfo из списка соедений и вызов завершающего метода в профиле
		/// </summary>
		public void CloseConnection()
		{
			Console.WriteLine("Закрытие соединение");
			Disconnect();
			if (ClientList.RemoveClient(ID))
			{
				profile.CloseConnection();

				online = false;

				bufferReliable.Dispose();
				bufferQueue.Dispose();
				bufferDiscard.Dispose();
			}
		}

		/// <summary>
		/// Отсылает клиенту команду на отключения
		/// </summary>
		private void Disconnect()
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
            switch (packet.ReadChannel())
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
				profile.ChannelRead(packet);
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
