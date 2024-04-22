using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP.Transmitter
{

    //internal class FlushRequest : IDisposable
    //{
    //    private object m_locker;
    //    private Action m_flush;
    //    private CancellationTokenSource m_cancelSource;

    //    public FlushRequest(object locker, Action flush)
    //    {
    //        m_locker = locker;
    //        m_flush = flush;
    //    }

    //    internal void FlushIn(int ticks)
    //    {
    //        lock (m_locker)
    //        {
    //            //Если запрос на вызов метода уже существует и он не был отменен
    //            if (m_cancelSource != null && !m_cancelSource.Token.IsCancellationRequested) return;

    //            m_cancelSource?.Dispose();

    //            m_cancelSource = new CancellationTokenSource();
    //            CancellationToken token = m_cancelSource.Token;
    //            Task.Run(() =>
    //            {
    //                Thread.Sleep(TimeSpan.FromTicks(ticks));
    //                lock (m_locker)
    //                {
    //                    if (token.IsCancellationRequested) return;
    //                  //  Console.WriteLine("Flush from request");
    //                    m_flush();
    //                }

    //            }, token);
    //        }
    //    }

    //    internal void Abort()
    //    {
    //        lock (m_locker)
    //        {
    //            m_cancelSource?.Cancel();
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        lock (m_locker)
    //        {
    //            Abort();
    //            m_cancelSource?.Dispose();
    //            m_cancelSource = null;
    //        }
    //    }
    //}
    public sealed class NetStream : IDisposable
    {
        private readonly Client m_owner;
        private readonly int _sendTimeOut;
        private readonly Packet m_buffer;
    //    private FlushRequest m_flushRequest;
        private readonly object m_locker = new object();
        private long _firstBufferPacketTime;


        public NetStream(Client client, int sendTimeout)
        {
            m_owner = client;
            _sendTimeOut = sendTimeout;
       //     m_flushRequest = new FlushRequest(m_locker, () => Flush());

            m_buffer = Packet.Create();
            m_buffer.TechnicalChannel = TechnicalChannel.Stream;
          
        }

      //  private volatile int m_test = 0;
        internal void Write(Packet packet)
        {
            lock (m_locker)
            {
                if (m_buffer.WrittenBytes == 0) _firstBufferPacketTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                else if (m_buffer.AvailableBytesForWriting < packet.Length + 2) { ForceFlushToSocket(); }//AvailableBytesForWriting < packet.Length + 2

              //  m_test++;
                m_buffer.WriteBytes(packet.Data, packet.Length);

              //  m_flushTickTime
               // m_flushRequest.FlushIn(m_client.Server.Options.SendTimeout);
            }
        }

        internal IEnumerable<Packet> Read(Packet inData)
        {
            while(inData.AvailableBytesForReading > 0)
            {
                Packet packet = Packet.Create();
                int readBytes = inData.ReadBytesIn(packet.Data);

                packet.InitData(readBytes);

                yield return packet;
            }
        }

        public void TimedFlushToSocket()
        {
            lock (m_locker)
            {
                // Calculate the time elapsed since the first packet was buffered
                long timeSinceInsertFirstPacket = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _firstBufferPacketTime;

                // If the buffer is empty or the timeout has not yet been reached, exit the method
                if (m_buffer.WrittenBytes == 0 || timeSinceInsertFirstPacket < _sendTimeOut) return;

                // Write the buffer to the socket
                m_owner.WriteInSocket(m_buffer);

                // Reset the buffer for the next set of data
                m_buffer.Reset();
                m_buffer.TechnicalChannel = TechnicalChannel.Stream;
            }
        }

        public void ForceFlushToSocket()
        {
            lock (m_locker)
            {
                // If the buffer is empty, exit the method
                if (m_buffer.WrittenBytes == 0) return;

                // Write the buffer to the socket
                m_owner.WriteInSocket(m_buffer);

                // Reset the buffer for the next set of data
                m_buffer.Reset();
                m_buffer.TechnicalChannel = TechnicalChannel.Stream;
            }
        }

        public void Dispose()
        {
         //   m_flushRequest.Dispose();
          //  m_outData.Dispose();
        }
    }
}
