using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.DATA
{
    public partial class PacketData
    {
        public long ReadLong()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            long ret = BitConverter.ToInt64(m_data, m_index);
            m_index += 8;
            return ret;
        }
        public int ReadInt()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            int ret = BitConverter.ToInt32(m_data, m_index);
            m_index += 4;
            return ret;
        }

        public short ReadShort()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            short ret = BitConverter.ToInt16(m_data, m_index);
            m_index += 2;
            return ret;
        }
        public ushort ReadUshort()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            ushort ret = BitConverter.ToUInt16(m_data, m_index);
            m_index += 2;
            return ret;
        }

        public byte[] ReadBytes()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            int length = ReadShort();
            byte[] array = new byte[length];
            Array.Copy(m_data, m_index, array, 0, length);
            m_index += length;
            return array;
        }

        public int ReadByte()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            return m_data[m_index++];
        }

        public bool ReadBool()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            return m_data[m_index++] != 0;
        }

        public float ReadFloat()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            float val = BitConverter.ToSingle(m_data, m_index);
            m_index += 4;
            return val;
        }

        public String ReadString()
        {
            if (m_dataAccess != Access.Read) throw new Exception("Packet not readable");
            return Encoding.UTF8.GetString(ReadBytes());
        }


    }
}
