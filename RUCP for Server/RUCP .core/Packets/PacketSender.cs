/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Debugger;
using RUCP.Tools;
using RUCP.Transmitter;
using System;

namespace RUCP.Packets
{
    public partial class Packet
    {
        public void Send()
        {
            try
            {

                if (Client == null)
                {
                    Console.Error.WriteLine("Error in Sender: client == null");
                    return;
                }
                if (sendCicle != 0)
                {
                    Console.WriteLine("Пакет заблокирован, отправка невозможна");
                    return;
                }


                bool dispose = false;

                if (Client.InsertBuffer(this))
                    Resender.Add(this); //Запись на переотправку
                else dispose = true;

                UdpSocket.SendTo(Data, Length, Client.Address);
                if (dispose) Dispose();
            }
            catch (BufferOverflowException e)
            {
                Console.WriteLine("Переполнения буффера");
                Debug.logError(GetType().Name, e.Message, e.StackTrace);
                Client.CloseConnection();
            }
            catch (Exception e)
            {
                Debug.logError(GetType().Name, e.Message, e.StackTrace);
            }
        }

    }

}
