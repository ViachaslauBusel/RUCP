using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RUCP.DATA
{
    public partial class PacketData
    {
        public T ReadValue<T>() where T : struct
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");

            int size = Marshal.SizeOf<T>();

            T value = MemoryMarshal.Read<T>(new Span<byte>(m_data, m_index, size));
            m_index += size;
            return value;
        }

        public long ReadLong()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            long ret = BitConverter.ToInt64(m_data, m_index);
            m_index += 8;
            return ret;
        }

        public ulong ReadUlong()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            ulong ret = BitConverter.ToUInt64(m_data, m_index);
            m_index += 8;
            return ret;
        }

        public int ReadInt()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            int ret = BitConverter.ToInt32(m_data, m_index);
            m_index += 4;
            return ret;
        }

        public uint ReadUint()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            uint ret = BitConverter.ToUInt32(m_data, m_index);
            m_index += 4;
            return ret;
        }

        public short ReadShort()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            short ret = BitConverter.ToInt16(m_data, m_index);
            m_index += 2;
            return ret;
        }
        public ushort ReadUshort()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            ushort ret = BitConverter.ToUInt16(m_data, m_index);
            m_index += 2;
            return ret;
        }

        public byte[] ReadBytes()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            int length = ReadShort();
            byte[] array = new byte[length];
            Array.Copy(m_data, m_index, array, 0, length);
            m_index += length;
            return array;
        }
        public int ReadBytesIn(byte[] data)
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");

            int length = ReadShort();
            Array.Copy(m_data, m_index, data, 0, length);

            m_index += length;

            return length;
        }

        public int ReadByte()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            return m_data[m_index++];
        }

        public sbyte ReadSbyte()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            return (sbyte)m_data[m_index++];
        }

        public bool ReadBool()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            return m_data[m_index++] != 0;
        }

        public float ReadFloat()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            float val = BitConverter.ToSingle(m_data, m_index);
            m_index += 4;
            return val;
        }

        public double ReadDouble()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            double val = BitConverter.ToDouble(m_data, m_index);
            m_index += 8;
            return val;
        }

        public unsafe decimal ReadDecimal()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            return ReadValue<decimal>();
        }

        public String ReadString()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            return Encoding.UTF8.GetString(ReadBytes());
        }

        public char ReadChar()
        {
            if (m_dataAcces != DataAccess.Read) throw new Exception("Packet unreadable");
            char ret = BitConverter.ToChar(m_data, m_index);
            m_index += 2;
            return ret;
        }
    }
}
