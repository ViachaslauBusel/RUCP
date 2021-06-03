/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RUCPc.Handler
{
    public class HandlersStorage
    {
        public delegate void Message(Packet packet);


        private static Message[] read = new Message[256];
        //Метод для обработки неизвестных пакетов
        private static Message unknown = Unknown;


        internal static void Unknown(Packet packet)
        {

        }

        public static void RegisterUnknown(Message rd)
        {
            unknown = rd;
        }

        public static void RegisterHandler(int id, Message rd)
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

        internal static Message GetHandler(int index)
        {
        
                if (index < 0 || index >= read.Length) return unknown;
                if (read[index] == null) return unknown;
                return read[index];
            
        }
    }
}
