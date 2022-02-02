using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RUCP.Cryptography
{
    internal class RSA
    {
        private RSACryptoServiceProvider encryptor;
        private static RSACryptoServiceProvider decryptor;

        internal static void SetPrivateKey(byte[] key)
        {
            if (key == null) return;
            decryptor = new RSACryptoServiceProvider();
            decryptor.ImportRSAPrivateKey(key);
        }
        internal void SetPublicKey(Packet packet)
        {
            encryptor = new RSACryptoServiceProvider();
            RSAParameters publicKey = new RSAParameters();
            publicKey.Modulus = packet.ReadBytes();
            publicKey.Exponent = packet.ReadBytes();
            encryptor.ImportParameters(publicKey);
        }

        internal void WritePublicKey(Packet packet)
        {
            decryptor = new RSACryptoServiceProvider();
            RSAParameters publicKey = decryptor.ExportParameters(false);
            packet.WriteBytes(publicKey.Modulus);
            packet.WriteBytes(publicKey.Exponent);
        }

        internal void Encrypt(Packet packet)
        {
            if (encryptor == null) return;
            Span<byte> spanData = packet.Data;
            encryptor.TryEncrypt(spanData.Slice(Packet.HEADER_SIZE, packet.Length - Packet.HEADER_SIZE), spanData.Slice(Packet.HEADER_SIZE, packet.Data.Length - Packet.HEADER_SIZE),
                           RSAEncryptionPadding.OaepSHA1, out int bytesWritten);
            packet.InitData(Packet.HEADER_SIZE + bytesWritten);
        }
        internal void Decrypt(Packet packet)
        {
            Span<byte> spanData = packet.Data;
            Span<byte> buffer = decryptor.Decrypt(spanData.Slice(Packet.HEADER_SIZE, packet.Length - Packet.HEADER_SIZE).ToArray(), false);
            buffer.CopyTo(spanData.Slice(Packet.HEADER_SIZE, buffer.Length));
            packet.InitData(Packet.HEADER_SIZE + buffer.Length);
        }

        internal void Dispose()
        {
            encryptor?.Dispose();
        }

    }
}
