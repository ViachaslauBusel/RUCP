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
        CONNECTED = 2,
        /// <summary>
        /// Ожидается соединение
        /// </summary>
        LISTENING = 4,
        /// <summary>
        /// Waiting for connection to close
        /// </summary>
        CLOSE_WAIT = 8
    }
}