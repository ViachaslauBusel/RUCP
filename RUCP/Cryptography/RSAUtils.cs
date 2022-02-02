using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RUCP.Cryptography
{
    internal static class RSAUtils
    {
        internal static void ImportRSAPrivateKey(this RSACryptoServiceProvider rsa, byte[] key)
        {
            RSAParameters rsaParam = rsa.ExportParameters(false);
            using (BinaryReader reader = new BinaryReader(new MemoryStream(key)))
            {
                int modules = reader.ReadInt32();
                rsaParam.Modulus = reader.ReadBytes(modules);
                int exponent = reader.ReadInt32();
                rsaParam.Exponent = reader.ReadBytes(exponent);
            }
           
            rsa.ImportParameters(rsaParam);
        }

        internal static void TryEncrypt(this RSACryptoServiceProvider rsa, ReadOnlySpan<byte> data, Span<byte> destination, System.Security.Cryptography.RSAEncryptionPadding padding, out int bytesWritten)
        {
            Span<byte> result = rsa.Encrypt(data.ToArray(), padding);
            bytesWritten = result.Length;
           result.CopyTo(destination);
        }
    }
}
