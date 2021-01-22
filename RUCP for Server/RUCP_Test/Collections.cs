using NUnit.Framework;
using RUCP.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP_Test
{
    class Collections
    {
        [Test]
        public void ProrityQueue_Test()
        {
            PriorityQueue<int> priority = new PriorityQueue<int>();
            priority.Enqueue(10);
            priority.Enqueue(7);
            priority.Enqueue(9);
            priority.Enqueue(8);
            priority.Enqueue(1);
            priority.Enqueue(2);
            priority.Enqueue(6);
            priority.Enqueue(5);
            priority.Enqueue(4);
            priority.Enqueue(3);
            int first = priority.Dequeue();
            for(int i=0; i<priority.Count; i++)
            {
                Assert.IsTrue(first < priority.Peek());
                first = priority.Dequeue();
            }
        }
    }
}
