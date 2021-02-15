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
                  Debug.Log("Error in Sender: client == null", MsgType.ERROR);
                    return;
                }
                if (sendCicle != 0)
                {
                    Debug.Log("Package is blocked, sending is not possible", MsgType.ERROR);
                    return;
                }


                bool dispose = false;

                if (Encrypt) Client.CryptographerAES.Encrypt(this);

                if (Client.InsertBuffer(this))
                    Resender.Add(this); //Record for re-sending
                else dispose = true;

                UdpSocket.SendTo(Data, Length, Client.Address);
                if (dispose) Dispose();
            }
            catch (BufferOverflowException e)
            {
                Debug.Log(e);
                Client.CloseConnection();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

    }

}
