/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.BufferChannels;
using RUCPc.Cryptography;
using RUCPc.Debugger;
using RUCPc.Network;
using RUCPc.Packets;
using RUCPc.Transmitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RUCPc.Transmitter
{
    internal class SocketListener
    {
        private Thread receive_th;
        private ServerSocket serverSocket;
        private volatile bool work = true;


        internal SocketListener(ServerSocket server)
        {
            

            serverSocket = server;
            receive_th = new Thread(Listener) { IsBackground = true };
            receive_th.Start();
        }

        internal void Stop()
        {
            work = false;
        }
        private void Listener()
        {
            while (work)
            {
                try
                {
                    int receiveBytes = serverSocket.Socket.ReceiveFrom(out byte[] data);
                    Packet packet = Packet.Create(data, receiveBytes);
                   

                    if (packet == null) continue;
                

                    switch (packet.Channel)
                    {
                        case Channel.Unreliable://Пакет пришол по ненадежному каналу
                            serverSocket.ServerInfo.received++;
                            serverSocket.AddPipeline(packet);
                            break;
                        case Channel.Reliable://Пакет пришол по надежному каналу
                            serverSocket.ServerInfo.received++;
                            SendConfirmACK(packet.ReadNumber(), Channel.ReliableACK);
                            if (serverSocket.bufferReliable.Check(packet))
                                serverSocket.AddPipeline(packet);
                            break;
                        case Channel.Queue:
                            serverSocket.ServerInfo.received++;
                            SendConfirmACK(packet.ReadNumber(), Channel.QueueACK);
                            serverSocket.bufferQueue.Check(packet);
                            break;
                        case Channel.Discard:
                            serverSocket.ServerInfo.received++;
                            SendConfirmACK(packet.ReadNumber(), Channel.DiscardACK);
                            if (serverSocket.bufferDiscard.Check(packet))
                                serverSocket.AddPipeline(packet);
                            break;


                        case Channel.ReliableACK://Confirmation of acceptance of the package by the other side
                            serverSocket.NetworkInfo.SetPing(
                                  serverSocket.bufferReliable.ConfirmAsk(packet.ReadNumber()));
                            break;
                        case Channel.QueueACK://Confirmation of acceptance of the package by the other side
                            serverSocket.NetworkInfo.SetPing(
                                  serverSocket.bufferQueue.ConfirmAsk(packet.ReadNumber()));
                            break;
                        case Channel.DiscardACK://Confirmation of acceptance of the package by the other side
                            serverSocket.NetworkInfo.SetPing(
                            serverSocket.bufferDiscard.ConfirmAsk(packet.ReadNumber()));
                            break;


                        case Channel.Connection://Confirmation of connection
                            serverSocket.CryptographerRSA.Decrypt(packet);
                            serverSocket.CryptographerAES.SetKey(packet);
                            serverSocket.socketConnector.OpenConnection();
                            break;
                        case Channel.Disconnect:
                            Debug.Log("The server has decided to disconnect you");
                            serverSocket.Close();
                            break;
                    }
    
                }
                catch (SocketException e)
                {
                    //10054 = The remote host forcibly dropped the existing connection. 10004 = Socket close
                    if (e.ErrorCode == 10054 || e.ErrorCode == 10004)
                    {
                        serverSocket.Close();
                    }
                    else Debug.Log($"SocketException:{e}", MsgType.ERROR);
                }
                catch (Exception e)
                {
                    Debug.Log($"SocketListener:{e}", MsgType.ERROR);
                }
            }
          //  Debug.Log("Listener has completed its work");
        }



        /// <summary>
        /// Отпроввляет АСК подтверждения получение клиентам пакета серверу
        /// </summary>
        private void SendConfirmACK(int number, int channel)
        {
            Packet packet =  Packet.Create(channel);
            packet.WriteNumber((ushort)number);
            serverSocket.Socket.Send(packet);
        }
    }
}
