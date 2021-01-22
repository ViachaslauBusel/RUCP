/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.BufferChannels;
using RUCP.Cryptography;
using RUCP.Handler;
using RUCP.Network;
using RUCP.Packets;
using RUCP.Transmitter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RUCP
{
    public class ServerSocket
    {
        public static string Version => $"ver. {version.ToString("0.###")}a";
        internal const float version = 0.003f;
        internal UdpSocket Socket{ get; private set; }
        public NetworkInfo NetworkInfo { get; private set; }
        public ServerInfo ServerInfo { get; private set; }
        public NetworkStatus NetworkStatus { get; internal set; } = NetworkStatus.CLOSED;

        internal SocketConnector socketConnector;
        private SocketSender socketSender;
        private SocketListener socketListener;

        private ConcurrentQueue<Packet> pipeline = new ConcurrentQueue<Packet>();

        internal BufferReliable bufferReliable { get; private set; }
        internal BufferQueue bufferQueue { get; private set; }
        internal BufferDiscard bufferDiscard { get; private set; }

        private int remotePort;
        private IPAddress remoteIP;

        internal RSA CryptographerRSA { get; private set; } 
        internal AES CryptographerAES { get; private set; }
        private RSAKey publicRSAKey = null;

        public ServerSocket(string address, int port)
        {
            // Получаем данные, необходимые для соединения

            //удаленный IP-адрес
            remoteIP = IPAddress.Parse(address);

            remotePort = port;
        }
        public void SetPublicRSAKey(byte[] modulus, byte[] exponent)
        {
            publicRSAKey = new RSAKey(modulus, exponent);
        }
        public void SetPublicRSAKey(RSAKey publicKey)
        {
            publicRSAKey = publicKey;
        }
        internal void AddPipeline(Packet packet)
        {
            if (packet.Encrypt) CryptographerAES.Decrypt(packet);
            ServerInfo.proccesed++;
                pipeline.Enqueue(packet);
        }

        public void ProcessPacket(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (pipeline.TryDequeue(out Packet packet))
                    HandlersStorage.GetHandler(packet.ReadType())(packet);
                else return;
            }
        }
        

        /// <summary>
        /// Подключение к серверу
        /// </summary>
        public void Connection()
        {
             if(NetworkStatus != NetworkStatus.CLOSED) { Debug.Log("соединение уже установлено"); return; }
            // Создаем endPoint по информации об удаленном хосте
            Socket = new UdpSocket(new IPEndPoint(remoteIP, remotePort));

            NetworkInfo = new NetworkInfo();
            ServerInfo = new ServerInfo();

            bufferReliable = new BufferReliable(500);
            bufferQueue = new BufferQueue(this, 500);
            bufferDiscard = new BufferDiscard(500);

            socketConnector = new SocketConnector(this);
            //Создаем обьект для отправки пакетов
            socketSender = new SocketSender(this);
            //Запускаем поток слушатель для прием пакетов
            socketListener = new SocketListener(this);

            CryptographerRSA = new RSA();
            CryptographerAES = new AES();

            if (publicRSAKey != null)
                CryptographerRSA.SetPublicKey(publicRSAKey.Modulus, publicRSAKey.Exponent);

            socketConnector.Connect();// Подключение к серверу
        }


        public void Close()
        {
            Debug.Log("Закрытие соединение");
            Packet packet =  Packet.Create(Channel.Disconnect);
            socketSender?.Send(packet);


            socketSender?.Stop();
            socketSender = null;

            socketListener?.Stop();
            socketListener = null;

            socketConnector?.Stop();
            socketConnector = null;

            Socket?.Close();
            Socket = null;

            CryptographerRSA?.Dispose();
            CryptographerAES?.Dispose();

            publicRSAKey = null;
        }




        public bool IsConnected()
        {
            return (NetworkStatus == NetworkStatus.СONNECTED);
        }

        /// <summary>
        /// Отпровляет данные по сети
        /// </summary>
        public void Send(Packet packet)
        {
            socketSender?.Send(packet);
        }

    }

}
