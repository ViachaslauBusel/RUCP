using RUCP.Client;
using RUCP.Handler;
using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Example
{
    //Объект этого класса будет создаваться для каждого нового клиента.
    class Profile : IProfile
    {
        //хранилище ключей "тип пакета -> метод для его обработки"
        public static HandlersStorage<Action<Profile, Packet>> handlersStorage = new HandlersStorage<Action<Profile, Packet>>(50);
        //Игровой персонаж клиента
        public Character Character { get; set; }
        //Получения пакетов
        public void ChannelRead(Packet pack)
        {
            //Поиск метода прявязаного к этому типу пакета и передача его в обработку
            handlersStorage.GetHandler(pack.ReadType())?.Invoke(this, pack);
        }

        public void CheckingConnection()
        {
            
        }
        //Закрытие соединение
        public void CloseConnection()
        {
            World.Exit(Character);
        }
        //Открытие соединение
        public bool OpenConnection(Packet pack)
        {
            return true;
        }
    }
}
