using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Network
{
   public class ServerInfo
    {
        /// <summary>
        /// Полученные пакеты от сервера
        /// </summary>
        public int received = 0;
        /// <summary>
        /// Количество обработанных пакетов
        /// </summary>
        public int proccesed = 0;
    }
}
