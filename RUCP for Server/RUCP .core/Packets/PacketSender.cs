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
                  Debug.LogError(new Exception("Error in Sender: client == null"));
                    return;
                }
                if (sendCicle != 0)
                {
                    Debug.Log("Packet.Send()", "Пакет заблокирован, отправка невозможна");
                    return;
                }


                bool dispose = false;

                if (Encrypt) Client.CryptographerAES.Encrypt(this);

                if (Client.InsertBuffer(this))
                    Resender.Add(this); //Запись на переотправку
                else dispose = true;

                UdpSocket.SendTo(Data, Length, Client.Address);
                if (dispose) Dispose();
            }
            catch (BufferOverflowException e)
            {
                Debug.LogError(e);
                Client.CloseConnection();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

    }

}
