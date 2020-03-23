using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace WebZen.Network
{
    public class PacketEncrypt
    {
        private static PacketEncrypt _instance;
        private static byte[] _key;
        private static byte[] _iv;
        private PacketEncrypt(byte[] key)
        {
            using (var rijndael = Rijndael.Create())
                _iv = (byte[])rijndael.IV.Clone();

            _key = (byte[])key.Clone();
        }

        public static void Initialize(byte[] key)
        {
            _instance = new PacketEncrypt(key);
        }

        public static void Encrypt(MemoryStream dest, MemoryStream src)
        {
            using(var rijndael = Rijndael.Create())
            {
                rijndael.Key = _key;
                rijndael.IV = _iv;
                using(var cs = new CryptoStream(dest, rijndael.CreateEncryptor(_key, _iv), CryptoStreamMode.Write))
                {
                    src.CopyTo(cs);
                }
                dest.Seek(0, SeekOrigin.Begin);
            }
        }

        public static byte[] Decrypt(byte[] src)
        {
            using (var rijndael = Rijndael.Create())
            {
                rijndael.Key = _key;
                rijndael.IV = _iv;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, rijndael.CreateDecryptor(_key, _iv), CryptoStreamMode.Read))
                    {
                        using (var bw = new BinaryReader(cs))
                        {
                            bw.Read(src, 0, src.Length);
                        }
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
            }
        }
    }
}
