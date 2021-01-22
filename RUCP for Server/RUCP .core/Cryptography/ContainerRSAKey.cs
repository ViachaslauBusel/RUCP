using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Cryptography
{
    internal static class ContainerRSAKey
    {
        internal static void GenerateKey()
        {
            RSAParameters publicKey;
            byte[] privateKey;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                publicKey = rsa.ExportParameters(false);
                privateKey = rsa.ExportRSAPrivateKey();
            }
        //    System.Console.WriteLine("public: "+BitConverter.ToString(publicKey));
         //   System.Console.WriteLine("private: " + BitConverter.ToString(privateKey));
            using (BinaryWriter writer = new BinaryWriter(File.Open("public.key", FileMode.Create)))
            {
                writer.Write(publicKey.Modulus.Length);
                writer.Write(publicKey.Modulus);
                writer.Write(publicKey.Exponent.Length);
                writer.Write(publicKey.Exponent);
            }
            using (FileStream file = new FileStream("private.key", FileMode.Create))
            {
                file.Write(privateKey);
            }
            System.Console.WriteLine("key generated successfully");
        }

        internal static byte[] LoadPrivateKey()
        {
            byte[] privateKey = null;
            if (File.Exists("private.key"))
            {
                using (FileStream file = new FileStream("private.key", FileMode.Open))
                {
                    privateKey = new byte[file.Length];
                    file.Read(privateKey, 0, privateKey.Length);
                }
            }
            return privateKey;
        }
    }
}
