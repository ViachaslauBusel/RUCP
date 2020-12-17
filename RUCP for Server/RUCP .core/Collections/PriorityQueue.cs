/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP.Collections
{
    public class PriorityQueue <T> where T: IComparable
    {
        private LinkedList<T> list = new LinkedList<T>();

        public int Count => list.Count;

        public void Enqueue(T obj)
        {
               LinkedListNode<T> current = list.Last;

                
                while(current != null)
                {
                    if ((current.Value.CompareTo(obj) < 1))
                    { list.AddAfter(current, obj); return; }
                    current = current.Previous;
                }
                list.AddFirst(obj);
        }

        public T Dequeue()
        {
          //  if (Count == 0) return default;
            T ret = list.First.Value;
            list.RemoveFirst();
            return ret;
        }

        public T Peek()
        {
            return list.First.Value;
        }
    }
}
