/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.BufferChannels;
using RUCPc.Cryptography;
using RUCPc.Debugger;
using RUCPc.Handler;
using RUCPc.Network;
using RUCPc.Packets;
using RUCPc.Transmitter;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace RUCPc
{
    public class Client
    {
        public static string Version => $"ver. {version.ToString("0.###")}a";
        internal const float version = 0.007f;
        internal UdpSocket Socket{ get; private set; }
        public NetworkInfo NetworkInfo { get; private set; }
        public ServerInfo ServerInfo { get; private set; }
        public NetworkStatus NetworkStatus { get; internal set; } = NetworkStatus.CLOSED;

        internal SocketConnector m_socketConnector;
        private SocketSender m_socketSender;
        private SocketListener m_socketListener;

        private ConcurrentQueue<Packet> m_pipeline = new ConcurrentQueue<Packet>();

        internal BufferReliable bufferReliable { get; private set; }
        internal BufferQueue bufferQueue { get; private set; }
        internal BufferDiscard bufferDiscard { get; private set; }

        private int remotePort;
        private IPAddress remoteIP;

        internal RSA CryptographerRSA { get; private set; } 
        internal AES CryptographerAES { get; private set; }
        private RSAKey publicRSAKey = null;

        internal object lockStatus = new object();

        public Client(string address, int port)
        {
            //Get the data required for the connection
            //remote IP address
            remoteIP = IPAddress.Parse(address);
            //remote port
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
                m_pipeline.Enqueue(packet);
        }
        /// <summary>
        /// Обработать указанное количество пакетов(count) из очереди принятых пакетов
        /// </summary>
        /// <param name="count"></param>
        public void ProcessPacket(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (m_pipeline.TryDequeue(out Packet packet))
                {
                    HandlersStorage.GetHandler(packet.ReadType())(packet);
                    //Debug.Log($"Process:{packet.ReadType()}");
                }
                else return;
            }
        }


        /// <summary>
        /// Server connection
        /// </summary>
        public void Connection()
        {
            try
            {
                lock (lockStatus)
                {
                    if (NetworkStatus != NetworkStatus.CLOSED) { Debug.Log("Connection already started", MsgType.WARNING); return; }
                    NetworkStatus = NetworkStatus.LISTENING;

                    // Create an endPoint based on information about a remote host
                    Socket = new UdpSocket(new IPEndPoint(remoteIP, remotePort), this);

                    NetworkInfo = new NetworkInfo();
                    ServerInfo = new ServerInfo();

                    bufferReliable = new BufferReliable(500);
                    bufferQueue = new BufferQueue(this, 500);
                    bufferDiscard = new BufferDiscard(500);

                    m_socketConnector = new SocketConnector(this);
                    //Create an object for sending packets
                    m_socketSender = new SocketSender(this);
                    //Start the listener stream to receive packets
                    m_socketListener = new SocketListener(this);


                    CryptographerRSA = new RSA();
                    CryptographerAES = new AES();

                    if (publicRSAKey != null)
                        CryptographerRSA.SetPublicKey(publicRSAKey.Modulus, publicRSAKey.Exponent);

                    m_socketConnector.Connect();

                }
            }
            catch { }
        }

        public void Close()
        {
            try
            {
                Packet packet = Packet.Create(Channel.Disconnect);
                m_socketSender?.Send(packet);

                TechnicalClose();
            }
            catch (Exception e){ Debug.Log($"Failed to successfully close the connection:{e}", MsgType.ERROR); }
        }

        internal void TechnicalClose()
        {
            lock (lockStatus)
            {
                if (NetworkStatus != NetworkStatus.CLOSED)
                {
                    NetworkStatus = NetworkStatus.CLOSED;
                    Debug.Log("Connection closed");
                }


                m_socketSender?.Stop();
                m_socketSender = null;

                m_socketConnector = null;

                Socket?.Close();
                Socket = null;

                CryptographerRSA?.Dispose();
                CryptographerRSA = null;
                CryptographerAES?.Dispose();
                CryptographerAES = null;

                publicRSAKey = null;

                m_socketListener?.Stop();
                m_socketListener = null;
            }
        }

        public bool IsConnected()
        {
            return (NetworkStatus == NetworkStatus.СONNECTED);
        }

        /// <summary>
        /// Sends data over the network
        /// </summary>
        public void Send(Packet packet)
        {
            try
            {
                m_socketSender?.Send(packet);
            }
            catch { }
        }

    }

}
