using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Debugger.Monitoring
{
    public class Probe
    {
        private Object m_locker = new object();
        public string Name { get; init; }
        public float MinValue { get; private set; }
        public float MidValue { get; private set; }
        public float MaxValue { get; private set; }
        public int Count { get; private set; } = 1;
       

        public Probe(string name, float value)
        {
            Name = name;
            MinValue = MidValue = MaxValue = value;
        }

        public void Add(float value)
        {
            lock (m_locker)
            {
                if (value < MinValue) MinValue = value;
                if (value > MaxValue) MaxValue = value;
                MidValue = (MidValue + value) / 2.0f;
                Count++;
            }
        }
    }
}
