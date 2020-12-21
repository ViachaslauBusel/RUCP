using RUCP.Packets;
using UnityEngine;

//Запись\чтения из\в пакет Vector3
public static class Vector3RW
    {
        public static void Read(this Packet packet, out Vector3 vector)
        {
            vector.x = packet.ReadFloat();
            vector.y = packet.ReadFloat();
            vector.z = packet.ReadFloat();
        }
        public static void Write(this Packet packet, Vector3 vector)
        {
            packet.WriteFloat(vector.x);
            packet.WriteFloat(vector.y);
            packet.WriteFloat(vector.z);
        }
    }

