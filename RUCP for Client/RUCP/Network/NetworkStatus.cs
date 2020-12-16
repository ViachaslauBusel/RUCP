using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Network
{
    public enum NetworkStatus: int
    {
        /// <summary>
        /// Соединение закрыто
        /// </summary>
        CLOSED = 0,
        /// <summary>
        /// Соединение установлено
        /// </summary>
        СONNECTED = 1,
        /// <summary>
        /// Ожидается соединение (слушается порт)
        /// </summary>
        LISTENING = 2
    }
}
