/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Debugger;
using RUCPs.Tools;
using RUCPs.Transmitter;
using System;

namespace RUCPs.Packets
{
    public partial class Packet
    {
        public void Send()
        {
            try
            {

                if (Client == null)
                {
                    Server.CallException(new Exception("The packet cannot be sent, the client is not specified"));
                    return;
                }
                if (m_sendCicle != 0)
                {
                    Server.CallException(new Exception("Packet is blocked, sending is not possible"));
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
                Server.CallException(e);
                Client.CloseConnection();
            }
            catch (Exception e)
            {
                Server.CallException(e);
            }
        }

    }

}
