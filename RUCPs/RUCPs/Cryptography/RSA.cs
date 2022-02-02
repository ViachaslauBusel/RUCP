using RUCPs.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Cryptography
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
            encryptor.TryEncrypt(spanData.Slice(Packet.HEADER_SIZE, packet.Length - Packet.HEADER_SIZE), spanData.Slice(Packet.HEADER_SIZE, packet.Data.Length - Packet.HEADER_SIZE),
                           RSAEncryptionPadding.OaepSHA1, out int bytesWritten);
            packet.Length = Packet.HEADER_SIZE + bytesWritten;
        }
        internal void Decrypt(Packet packet)
        {
            Span<byte> spanData = packet.Data;
            Span<byte> buffer = decryptor.Decrypt(spanData.Slice(Packet.HEADER_SIZE, packet.Length - Packet.HEADER_SIZE).ToArray(), false);
            buffer.CopyTo(spanData.Slice(Packet.HEADER_SIZE, buffer.Length));
            packet.Length = Packet.HEADER_SIZE + buffer.Length;
        }

        internal void Dispose()
        {
            encryptor?.Dispose();
        }

    }
}
