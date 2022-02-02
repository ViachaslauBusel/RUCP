﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.DATA
{
    public partial class PacketData
    {
        public long ReadLong()
        {
            long ret = BitConverter.ToInt64(m_data, m_index);
            m_index += 8;
            return ret;
        }
        public int ReadInt()
        {
            int ret = BitConverter.ToInt32(m_data, m_index);
            m_index += 4;
            return ret;
        }

        public short ReadShort()
        {
            short ret = BitConverter.ToInt16(m_data, m_index);
            m_index += 2;
            return ret;
        }
        public ushort ReadUshort()
        {
            ushort ret = BitConverter.ToUInt16(m_data, m_index);
            m_index += 2;
            return ret;
        }

        public byte[] ReadBytes()
        {
            int length = ReadShort();
            byte[] array = new byte[length];
            Array.Copy(m_data, m_index, array, 0, length);
            m_index += length;
            return array;
        }

        public int ReadByte()
        {
            return m_data[m_index++];
        }

        public bool ReadBool()
        {
            return m_data[m_index++] != 0;
        }

        public float ReadFloat()
        {
            float val = BitConverter.ToSingle(m_data, m_index);
            m_index += 4;
            return val;
        }

        public String ReadString()
        {
            return Encoding.UTF8.GetString(ReadBytes());
        }


    }
}
