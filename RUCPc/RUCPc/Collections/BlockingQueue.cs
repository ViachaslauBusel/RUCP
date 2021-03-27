/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPc.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCPc.Collections
{
    public class BlockingQueue<T> where T : IDelayed, IComparable
    {
        private PriorityQueue<T> container = new PriorityQueue<T>();

        private bool blocking = true;
        public bool isBlocking
        {
            get => blocking;
            set
            {
                    lock (container)
                    {
                        blocking = value;
                        Monitor.PulseAll(container);
                    }
            }
        }
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
                while (true)
                {
                    if (container.Count == 0)
                    {
                        if (blocking) { Monitor.Wait(container); continue; }
                        return default;
                    }

                    long time = container.Peek().GetDelay();
                    if (time > 0) Monitor.Wait(container, (int)time);

                    else return container.Dequeue();
                }

            }
        }
    }
}