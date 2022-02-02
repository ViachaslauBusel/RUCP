using RUCPc.Debugger;
using RUCPc.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RUCPc.Cryptography
{
    internal class RSA
    {
        private RSACryptoServiceProvider decryptor;
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
            decryptor = new RSACryptoServiceProvider();
            RSAParameters publicKey = decryptor.ExportParameters(false);
            packet.WriteBytes(publicKey.Modulus);
            packet.WriteBytes(publicKey.Exponent);
        }

        internal void Encrypt(Packet packet)
        {
            if (encryptor == null) return;
            try
            {
                byte[] data = new byte[packet.Length - Packet.HEADER_SIZE];

                Array.Copy(packet.Data, Packet.HEADER_SIZE, data, 0, data.Length);
                byte[] buffer = encryptor.Encrypt(data, false);

                Array.Copy(buffer, 0, packet.Data, Packet.HEADER_SIZE, buffer.Length);
                packet.Length = Packet.HEADER_SIZE + buffer.Length;
                packet.Encrypt = true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        internal void Decrypt(Packet packet)
        {
            byte[] data = new byte[packet.Length - Packet.HEADER_SIZE];
            Array.Copy(packet.Data, Packet.HEADER_SIZE, data, 0, data.Length);
            byte[] buffer = decryptor.Decrypt(data, RSAEncryptionPadding.OaepSHA1);
            Array.Copy(buffer, 0, packet.Data, Packet.HEADER_SIZE, buffer.Length);
            packet.Length = Packet.HEADER_SIZE + buffer.Length;
        }
        internal void Dispose()
        {
            decryptor?.Dispose();
        }
    }
}
