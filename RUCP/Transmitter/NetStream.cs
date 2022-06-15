using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP.Transmitter
{

    internal class FlushRequest
    {
        private object m_locker;
        private Action m_flush;
        private CancellationTokenSource m_cancelSource;

        public FlushRequest(object locker, Action flush)
        {
            m_locker = locker;
            m_flush = flush;
        }

        internal void FlushIn(int millis)
        {
            lock (m_locker)
            {
                //Если запрос на вызов метода уже существует и он не был отменен
                if (m_cancelSource != null && !m_cancelSource.Token.IsCancellationRequested) return;

                m_cancelSource = new CancellationTokenSource();
                Task.Delay(millis).ContinueWith((t) =>
                {
                    lock (m_locker)
                    {
                        if (m_cancelSource.Token.IsCancellationRequested) return;
                      //  Console.WriteLine("Flush from request");
                        m_flush();
                    }

                });
            }
        }

        internal void Abort()
        {
            lock (m_locker)
            {
                m_cancelSource?.Cancel();
            }
        }
    }
    public class NetStream
    {
        private Client m_client;
        private Packet m_outData;
        private FlushRequest m_flushRequest;
        private object m_locker = new object();


        public NetStream(Client client)
        {
            m_client = client;
            m_flushRequest = new FlushRequest(m_locker, () => Flush());

            m_outData = Packet.Create();
            m_outData.InitClient(client);
            m_outData.TechnicalChannel = TechnicalChannel.Stream;
          
        }

        internal void Write(Packet packet)
        {
            lock (m_locker)
            {
                if (m_outData.AvailableBytesForWriting < packet.Length + 2) { Flush(); }//AvailableBytesForWriting < packet.Length + 2


                m_outData.WriteBytes(packet.Data, packet.Length);

                m_flushRequest.FlushIn(m_client.Server.Options.SendTimeout);
            }
        }

        internal IEnumerable<Packet> Read(Packet inData)
        {
            while(inData.AvailableBytesForReading > 0)
            {
                Packet packet = Packet.Create();
                packet.InitClient(m_client);
                int readBytes = inData.ReadBytesIn(packet.Data);

                packet.InitData(readBytes);

                yield return packet;
            }
        }

        public void Flush()
        {
            lock (m_locker)
            {
                m_flushRequest.Abort();
                if(m_outData.WrittenBytes == 0) return;
                m_client.Server.Socket.SendTo(m_outData, m_client.RemoteAddress);
             //   Console.WriteLine($"Send to remote address");
                m_outData.Reset();
                m_outData.InitClient(m_client);
                m_outData.TechnicalChannel = TechnicalChannel.Stream;
            }
        }
    }
}
