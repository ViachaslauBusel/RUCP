using RUCP.Client;

namespace Example
{
    //Игровой персонаж
    internal class Character
    {
        //Уникальный ИД
        public int ID { get; internal set; } = 0;
        public string Name { get; set; }
        //Позиция на карте
        public Vector3 Position { get; set; }
        //Сокет через который можно отправлять пакеты клиенту создавшего этого персонажа
        public ClientSocket Socket { get; internal set; }
    }
}