using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Cryptography
{
    internal class RSA
    {
        private RSACryptoServiceProvider encryptor = new RSACryptoServiceProvider();
        private static RSACryptoServiceProvider decryptor;

        internal static void SetPrivateKey(byte[] key)
        {
            if (key == null) return;
            decryptor = new RSACryptoServiceProvider();
            decryptor.ImportRSAPrivateKey(key, out int bytesRead);
        }
        internal void SetPublicKey(Packet packet)
        {
            RSAParameters publicKey = new RSAParameters();
            publicKey.Modulus = packet.ReadBytes();
            publicKey.Exponent = packet.ReadBytes();
            encryptor.ImportParameters(publicKey);
        }

        internal void Encrypt(Packet packet)
        {
            Span<byte> spanData = packet.Data;
            encryptor.TryEncrypt(spanData.Slice(Packet.headerLength, packet.Length - Packet.headerLength), spanData.Slice(Packet.headerLength, packet.Data.Length - Packet.headerLength),
                           RSAEncryptionPadding.OaepSHA1, out int bytesWritten);
            packet.Length = Packet.headerLength + bytesWritten;
        }
        internal void Decrypt(Packet packet)
        {
            Span<byte> spanData = packet.Data;
            Span<byte> buffer = decryptor.Decrypt(spanData.Slice(Packet.headerLength, packet.Length - Packet.headerLength).ToArray(), false);
            buffer.CopyTo(spanData.Slice(Packet.headerLength, buffer.Length));
            packet.Length = Packet.headerLength + buffer.Length;
        }

        internal void Dispose()
        {
            encryptor?.Dispose();
        }

    }
}
