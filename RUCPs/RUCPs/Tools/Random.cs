using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Tools
{
    public static class Rand
    {
        private static Random random = new Random();

        public static int Range(int min, int max)
        {
            lock (random)
            {
                return random.Next(min, max);
            }
        }
        public static float Range(float min, float max)
        {
            lock (random)
            {
                float dif = max - min;
                return min + ((float)random.NextDouble() * dif);
            }
        }
    }
}
