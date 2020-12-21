using RUCP;
using RUCP.Handler;
using RUCP.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class World
    {
        //Коллекция со всеми персонажами находящимися на карте
        private static ConcurrentDictionary<int, Character> charatcers = new ConcurrentDictionary<int, Character>();
        //Обьект блокировки на вход и выход с карты
        private static object gate_lock = new object();

        [Handler(Types.EnterWorld)]
        public static void Enter(Profile profile, Packet packet)
        {
            profile.Character = new Character();

            profile.Character.Socket = packet.Client;
            profile.Character.Name = packet.ReadString();
            profile.Character.Position = new Vector3(0, 1, 0);

            //Отправка пакета с подтверждением успешного создания персонажа и его позиции на карте
            packet = Packet.Create(packet.Client, Channel.Reliable);
            packet.WriteType(Types.EnterWorld);
            packet.Write(profile.Character.Position);
            packet.Send();

            //блокировка для предотвращение рассинхронизации подключенных\удаленных персонажей на клиенте
            lock (gate_lock)
            {
                //Присвоение уникального ИД персонажу
                while (!charatcers.TryAdd(profile.Character.ID, profile.Character))
                  profile.Character.ID++;

                //Синхронизация подключенных игроков
                SendAllCreate(profile.Character);
            }
        }

        [Handler(Types.ExitWorld)]
        public static void Exit(Profile profile, Packet packet)
        {
            Exit(profile.Character);
        }
        public static void Exit(Character character)
        {
            //блокировка для предотвращение рассинхронизации подключенных\удаленных персонажей на клиенте
            lock (gate_lock)
            {
                if (character != null && charatcers.TryRemove(character.ID, out Character _character))
                {
                    //Отправка информации об отключенном персонаже подключеным игрокам
                    SendAllDestroy(_character);
                }
            }
        }


        [Handler(Types.CharacterMove)]
        public static void Move(Profile profile, Packet packet)
        {
            //Считывание новой позиции персонажа на клиенте
            packet.Read(out Vector3 position);
            //Изменение позиции на сервере
            profile.Character.Position = position;

            //Рассылка новой позиции всем подключенным клиентам
            foreach (Character otherCharacter in charatcers.Values)
            {
                if (otherCharacter == null || profile.Character.ID == otherCharacter.ID) continue;
                Packet sendingPacket = Packet.Create(otherCharacter.Socket, Channel.Discard);
                sendingPacket.WriteType(Types.CharacterMove);
                sendingPacket.WriteInt(profile.Character.ID);
                sendingPacket.Write(profile.Character.Position);
                sendingPacket.Send();
            }
        }

        private static void SendAllCreate(Character character)
        {
            foreach (Character otherCharacter in charatcers.Values)
            {
                if (otherCharacter == null || character.ID == otherCharacter.ID) continue;

                //Отправка информации о новом персонаже подключеным игрокам-->>
                Packet packet = Packet.Create(otherCharacter.Socket, Channel.Queue);
                packet.WriteType(Types.CharacterCreate);
                packet.WriteInt(character.ID);
                packet.WriteString(character.Name);
                packet.Write(character.Position);
                packet.Send();
                //<<--

                //Отправка информации о подключеных персонажах новому игроку -->>
                packet = Packet.Create(character.Socket, Channel.Queue);
                packet.WriteType(Types.CharacterCreate);
                packet.WriteInt(otherCharacter.ID);
                packet.WriteString(otherCharacter.Name);
                packet.Write(otherCharacter.Position);
                packet.Send();
                //<<--

            }
        }
        private static void SendAllDestroy(Character character)
        {
            foreach (Character otherCharacter in charatcers.Values)
            {
                if (otherCharacter == null || character.ID == otherCharacter.ID) continue;

                //Отправка информации об отключенном персонаже подключеным игрокам-->>
                Packet packet = Packet.Create(otherCharacter.Socket, Channel.Queue);
                packet.WriteType(Types.CharacterDestroy);
                packet.WriteInt(character.ID);
                packet.Send();
                //<<--

            }
        }
    }
}
