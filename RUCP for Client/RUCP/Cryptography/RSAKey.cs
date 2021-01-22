using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Cryptography
{
    public class RSAKey
    {
        public byte[] Modulus { get; private set; }
        public byte[] Exponent { get; private set; }

        public RSAKey(byte[] modulus, byte[] exponent)
        {
            Modulus = modulus;
            Exponent = exponent;
        }
    }
}
