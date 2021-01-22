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
        private RSACryptoServiceProvider decryptor = new RSACryptoServiceProvider();
        private RSACryptoServiceProvider encryptor;

        internal void SetPublicKey(byte[] modulus, byte[] exponent)
        {
            encryptor = new RSACryptoServiceProvider();

            RSAParameters publicKey = new RSAParameters();
            publicKey.Modulus = modulus;
            publicKey.Exponent = exponent;
            encryptor.ImportParameters(publicKey);
        }
        internal void WritePublicKey(Packet packet)
        {
            RSAParameters publicKey = decryptor.ExportParameters(false);
            packet.WriteBytes(publicKey.Modulus);
            packet.WriteBytes(publicKey.Exponent);
        }

        internal void Encrypt(Packet packet)
        {
            if (encryptor == null) return;
            try
            {
                byte[] data = new byte[packet.Length - Packet.headerLength];

                Array.Copy(packet.Data, Packet.headerLength, data, 0, data.Length);
                byte[] buffer = encryptor.Encrypt(data, false);

                Array.Copy(buffer, 0, packet.Data, Packet.headerLength, buffer.Length);
                packet.Length = Packet.headerLength + buffer.Length;
                packet.Encrypt = true;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to encrypt package RSA: "+e);
            }
        }
        internal void Decrypt(Packet packet)
        {
            byte[] data = new byte[packet.Length - Packet.headerLength];
            Array.Copy(packet.Data, Packet.headerLength, data, 0, data.Length);
            byte[] buffer = decryptor.Decrypt(data, RSAEncryptionPadding.OaepSHA1);
            Array.Copy(buffer, 0, packet.Data, Packet.headerLength, buffer.Length);
            packet.Length = Packet.headerLength + buffer.Length;
        }
        internal void Dispose()
        {
            decryptor?.Dispose();
        }
    }
}
