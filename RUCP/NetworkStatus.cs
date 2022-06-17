using System;

namespace RUCP
{
    [Flags]
    public enum NetworkStatus : int
    {
        /// <summary>
        /// Соединение закрыто
        /// </summary>
        CLOSED = 1,
        /// <summary>
        /// Соединение установлено
        /// </summary>
        СONNECTED = 2,
        /// <summary>
        /// Ожидается соединение
        /// </summary>
        LISTENING = 4
    }
}