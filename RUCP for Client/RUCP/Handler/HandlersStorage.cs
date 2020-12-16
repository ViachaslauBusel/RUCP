using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RUCP.Handler
{
    public class HandlersStorage
    {
        private static Action<Packet>[] read = new Action<Packet>[256];
        //Метод для обработки неизвестных пакетов
        private static Action<Packet> unknown = Unknown;


        internal static void Unknown(Packet packet)
        {

        }

        public static void RegisterUnknown(Action<Packet> rd)
        {
            unknown = rd;
        }

        public static void RegisterHandler(int id, Action<Packet> rd)
        {
            if (read == null) return;
            if (id < 0 || id >= read.Length) return;

            read[id] = rd;
        }

        public static void UnregisterHandler(int id)
        {
            if (read == null) return;
            if (id < 0 || id >= read.Length) return;

            read[id] = null;
        }

        internal static Action<Packet> GetHandler(int index)
        {
        
                if (index < 0 || index >= read.Length) return unknown;
                if (read[index] == null) return unknown;
                return read[index];
            
        }
    }
}
