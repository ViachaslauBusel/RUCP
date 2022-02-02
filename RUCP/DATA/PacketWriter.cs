using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.DATA
{
    public partial class PacketData
    {
        unsafe private void WriteValue(void* value, int len)
        {
            fixed (byte* d = m_data)
            { Buffer.MemoryCopy(value, d + m_index, len, len); }
            m_realLength = m_index += len;
        }
        /// <summary>
        /// Записывает byte[]  в заранее определнный массив данных на отправку.
        /// Если размер данных для записи превышает размер массива возврощает exception
        /// </summary>
        public void WriteBytes(byte[] bytes)
        {
            WriteShort((short)bytes.Length);
            Array.Copy(bytes, 0, m_data, m_index, bytes.Length);
            m_realLength = m_index += bytes.Length;
        }
        unsafe public void WriteFloat(float value)
        {
            WriteValue(&value, 4);
        }

        unsafe public void WriteInt(int value)
        {
            WriteValue(&value, 4);
        }
        unsafe public void WriteLong(long value)
        {
            WriteValue(&value, 8);
        }

        unsafe public void WriteShort(short value)
        {
            WriteValue(&value, 2);
        }
        unsafe public void WriteUshort(ushort value)
        {
            WriteValue(&value, 2);
        }
        public ref byte WriteByte(byte value)
        {
            m_data[m_index++] = value;
            m_realLength = m_index;
            return ref m_data[m_index - 1];
        }

        public void WriteBool(bool value)
        {
            m_data[m_index++] = (byte)(value ? 1 : 0);
            m_realLength = m_index;
        }

        public void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes);
        }

        /*  public void Write(float f) => WriteFloat(f);
          public void Write(int i) => WriteInt(i);
          public void Write(long l) => WriteLong(l);
          public void Write(short s) => WriteShort(s);
          public void Write(byte b) => WriteByte(b);
          public void Write(bool b) => WriteBool(b);
          public void Write(string s) => WriteString(s);*/

    }
}
