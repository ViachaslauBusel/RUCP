using System;
using System.Collections.Generic;
using System.Text;

//Типы пакетов
  public class Types
    {
    public const short EnterWorld = 1; //C->S запрос на подключение к игровому миру. S->C ответ об успешном подключении и позицией игрока на сервере
    public const short CharacterCreate = 2;//S->C информация для создания персонажей подключенных к игровому миру на клиенте
    public const short CharacterMove = 3;//C->S отправка новой позиции игрока на сервер. S->C рассылка позиций персонажей
    public const short CharacterDestroy = 4;//S->C информация для удаление персонажей
    public const short ExitWorld = 5;//C->S Запрос на выход из игрового мира
}

