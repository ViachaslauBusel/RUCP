using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RUCP.DATA
{
    public partial class PacketData
    {
        public unsafe void WriteValue<T>(T value) where T : struct
        {
            fixed (byte* d = m_data)
            {
                IntPtr ptr = new IntPtr(d + m_index);
                m_index += Marshal.SizeOf<T>();
                Marshal.StructureToPtr(value, ptr, false);
            }
        }
        unsafe private void WriteValue(void* value, int len)
        {
            if (m_dataAcces != DataAccess.Write) throw new Exception("Packet not writable");
            fixed (byte* d = m_data)
            { Buffer.MemoryCopy(value, d + m_index, len, len); }
            m_realLength = m_index += len;
        }
        /// <summary>
        /// Writes a byte[] to a predefined data array to send.
        /// If the size of the data to write exceeds the size of the array, returns an exception
        /// </summary>
        unsafe public void WriteBytes(byte[] bytes)
        {
            //fixed (byte* d = bytes)
            //{ WriteValue(d, bytes.Length); }
            if (m_dataAcces != DataAccess.Write) throw new Exception("Packet not writable");
            WriteShort((short)bytes.Length);
            Array.Copy(bytes, 0, m_data, m_index, bytes.Length);
            m_realLength = m_index += bytes.Length;
        }
        unsafe public void WriteBytes(byte[] data, int length)
        {
            //fixed (byte* d = data)
            //{ WriteValue(d, length); }
            if (m_dataAcces != DataAccess.Write) throw new Exception("Packet not writable");
            WriteShort((short)length);
            Array.Copy(data, 0, m_data, m_index, length);
            m_realLength = m_index += length;
        }

        public unsafe void WriteDecimal(decimal value)
        {
            WriteValue(&value, 16);
        }

        unsafe public void WriteDouble(double value)
        {
            WriteValue(&value, 8);
        }

        unsafe public void WriteFloat(float value)
        {
            WriteValue(&value, 4);
        }

        unsafe public void WriteInt(int value)
        {
            WriteValue(&value, 4);
        }

        public unsafe void WriteUint(uint value)
        {
            WriteValue(&value, 4);
        }

        unsafe public void WriteLong(long value)
        {
            WriteValue(&value, 8);
        }

        public unsafe void WriteUlong(ulong value)
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
            if (m_dataAcces != DataAccess.Write) throw new Exception("Packet not writable");
            m_data[m_index++] = value;
            m_realLength = m_index;
            return ref m_data[m_index - 1];
        }

        public void WriteSByte(sbyte value)
        {
            if (m_dataAcces != DataAccess.Write) throw new Exception("Packet not writable");
            m_data[m_index++] = (byte)value;
            m_realLength = m_index;
        }

        public void WriteBool(bool value)
        {
            if (m_dataAcces != DataAccess.Write) throw new Exception("Packet not writable");
            m_data[m_index++] = (byte)(value ? 1 : 0);
            m_realLength = m_index;
        }

        public void WriteString(string value)
        {
            byte[] bytes = string.IsNullOrEmpty(value) ? new byte[0] 
                                                       : Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes);
        }

        public unsafe void WriteChar(char value)
        {
            WriteValue(&value, 2);
        }
    }
}
