using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Packets
{
   public partial class PacketData
    {
        public long ReadLong()
        {
            long ret = BitConverter.ToInt64(data, index);
            index += 8;
            return ret;
        }
        public int ReadInt()
        {
            int ret = BitConverter.ToInt32(data, index);
            index += 4;
            return ret;
        }

        public short ReadShort()
        {
            short ret = BitConverter.ToInt16(data, index);
            index += 2;
            return ret;
        }

        public byte[] ReadBytes(int len)
        {
            byte[] array = new byte[len];
            Array.Copy(data, index, array, 0, len);
            index += len;
            return array;
        }

        public int ReadByte()
        {
            return data[index++];
        }

        public bool ReadBool()
        {
            return data[index++] != 0;
        }

        public float ReadFloat()
        {
            float val = BitConverter.ToSingle(data, index);
            index += 4;
            return val;
        }

        public String ReadString()
        {
            int len = ReadShort();
            return Encoding.UTF8.GetString(ReadBytes(len)); 
        }


    }
}
