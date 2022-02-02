using RUCPs.Logger;
using RUCPs.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Cryptography
{
   internal class AES
    {
        private RijndaelManaged aes = new RijndaelManaged();
        private ICryptoTransform encryptor, decryptor;


        internal AES()
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            encryptor = aes.CreateEncryptor();
            decryptor = aes.CreateDecryptor();
        }

        internal void WriteKey(Packet packet)
        {
            packet.WriteBytes(aes.Key);
            packet.WriteBytes(aes.IV);
        }

        internal void Decrypt(Packet packet)
        {
            Convert(packet, decryptor);
        }
        internal void Encrypt(Packet packet)
        {
            Convert(packet, encryptor);
        }
        private void Convert(Packet packet, ICryptoTransform crypto)
        {
            int encryptLength = Transform(packet.Data, Packet.HEADER_SIZE, packet.Length - Packet.HEADER_SIZE, crypto);

            packet.Length = Packet.HEADER_SIZE + encryptLength;
        }
        private int Transform(byte[] buffer, int offset, int count, ICryptoTransform transform)
        {
            using (var stream = new MemoryStream())
            {
                using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
                {
                    cs.Write(buffer, offset, count);
                    cs.FlushFinalBlock();
                    stream.Position = 0L;
                    int encryptLength = (int)stream.Length;
                    stream.Read(buffer, offset, encryptLength);

                    return encryptLength;
                }
            }
        }
        internal void Dispose()
        {
            encryptor.Dispose();
            decryptor.Dispose();
            aes.Dispose();
        }
    }
}
