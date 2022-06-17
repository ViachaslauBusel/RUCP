using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RUCP.Collections
{
    public class BlockingQueue<T> where T : IDelayed, IComparable
    {
        private volatile bool m_dispose = false;
        private PriorityQueue<T> container = new PriorityQueue<T>();


        public void Add(T t)
        {
            lock (container)
            {
                container.Enqueue(t);
                Monitor.Pulse(container);
            }
        }


        public T Take()
        {
            lock (container)
            {
                while (!m_dispose)
                {
                    if (container.Count == 0)
                    { Monitor.Wait(container); continue; }

                    long time = container.Peek().GetDelay();
                    if (time > 0){ Monitor.Wait(container, (int)time); continue; }

                    else return container.Dequeue();
                }
                return default(T);
            }
        }

        internal void Dispose()
        {
            m_dispose = true;
            lock (container)
            {
                Monitor.Pulse(container);
            }
        }
    }
}
