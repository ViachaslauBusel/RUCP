

using RUCP.Collections;
using RUCP.Network;
using System;
using System.Net;

namespace RUCP.Packets
{
    public partial class Packet : PacketData, IDelayed, IComparable
    {
		/// <summary>
		/// Длина заголовка пакета
		/// </summary>
		private static readonly int headerLength = 5;
		internal long sendTime = 0;//Время отправки
		private long resendTime = 0;//Время повторной отправки пакета при неудачной попытке доставки
		internal volatile int sendCicle = 0;//При отправке или получении пакета, пакет блокируется для невозможности повторной отправки
		private volatile bool ack = false;


        public long ResendTime => resendTime;
		public bool isBlock => sendCicle != 0;

		/// <summary>
		/// Записывает время отправки/переотправки
		/// </summary>
		public void WriteSendTime(long timeOut)
		{
			if (sendCicle == 1) sendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			resendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (timeOut * sendCicle);
		}
		public long CalculatePing()
		{
			return (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sendTime);
		}

		public bool isAck()
		{
			return ack;
		}

		/***
		 * Задает метку доставки пакета.
		 * Задается автоматически при получении пакета от клиента с подтверждением доставки
		 * @param ack
		 */
		public void setAck(bool ack)
		{
			//Debug.Log($"подтверждение полученно через: {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sendTime}");
			this.ack = ack;
		}


		/***
		 * Возврощает канал по которому будет\был передан пакет
		 * @return
		 */
		public int ReadChannel()
		{
			return (int)data[0];
		}

		public bool isChannel(int channel)
		{
			return data[0] == channel;
		}

		/***
			 * Задает порядковый номер отпровляемого пакета.
			 *
			 */
		internal void WriteNumber(int number)
		{

			byte[] number_b = BitConverter.GetBytes((ushort)number);
			data[3] = number_b[0];
			data[4] = number_b[1];
		}

		/// <summary>
		/// Возврощает порядковый номер отпровленного пакета
		/// </summary>
		internal int ReadNumber()
		{
			return BitConverter.ToUInt16(data, 3);
		}



		/// <summary>
		/// Записывает тип пакета в заголовок
		/// </summary>
		public void WriteType(int typ)
		{
			byte[] type_b = BitConverter.GetBytes((short)typ);

			data[1] = type_b[0];
			data[2] = type_b[1];
		}
		public int ReadType()
		{
			return BitConverter.ToInt16(data, 1);
		}

		public long GetDelay()
        {
			return resendTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		}

        public int CompareTo(object obj)
        {
			if (resendTime > ((Packet)obj).resendTime) return 1;
			return -1;
		}
    }
}
