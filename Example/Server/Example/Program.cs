using RUCP;
using System;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //Создание сервера по порту 3737
            Server server = new Server(3737);
            //Назначение класса который будет предстовлять объект игрока
            Server.SetHandler(() => new Profile());
            //Запуск сервера
            server.Start();
        }
    }
}
