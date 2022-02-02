using RUCPs.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Debugger.Monitoring
{
    public class Telemetry
    {
        private ConcurrentDictionary<string, Probe> m_measures = new ConcurrentDictionary<string, Probe>();

        //static Telemetry()
        //{
        //  //  Terminal.AddCommand(new TelemetryCommand());
        //}
        public int ProbesCount => m_measures.Count;
        public IEnumerable<Probe> GetProbes => m_measures.Values;

        public void Measure(string key, float value)
        {
            if (!m_measures.ContainsKey(key))
            {
                if (m_measures.TryAdd(key, new Probe(key, value))) return;
            }

            m_measures[key].Add(value);
        }
    }
}
