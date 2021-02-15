/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.BufferChannels;
using RUCP.Cryptography;
using RUCP.Debugger;
using RUCP.Network;
using RUCP.Packets;
using RUCP.Transmitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RUCP.Transmitter
{
    internal class SocketListener
    {
        private Thread receive_th;
        private ServerSocket serverSocket;


        internal SocketListener(ServerSocket server)
        {
            

            serverSocket = server;
            receive_th = new Thread(Listener);
            receive_th.Start();
        }

        internal void Stop()
        {
            receive_th.Abort();
            receive_th.Join();
        }
        private void Listener()
        {
            while (true)
            {
                try
                {
                    int receiveBytes = serverSocket.Socket.ReceiveFrom(out byte[] data);
                    Packet packet = Packet.Create(data, receiveBytes);

                    if (packet == null) continue;

                    switch (packet.Channel)
                    {

                        case Channel.Unreliable://Пакет пришол по ненадежному каналу
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
                    Debug.Log(e);
                    serverSocket.Close();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
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
