using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebZen.Network;

namespace WebZen.Util
{
    public static class BuxDecode
    {
        private static byte[] bBuxCode = new byte[] { 0xFC, 0xCF, 0xAB };
        private static byte[] bBuxCode2 = new byte[] { 0x81, 0xDE, 0xE1 };

        private static void Decode(byte[] buffer, byte[] key)
        {
            for (var n = 0; n < buffer.Length; n++)
            {
                buffer[n] ^= key[n % 3];      // Nice trick from WebZen
            }
        }

        public static void Decode(byte[] buffer)
        {
            Decode(buffer, bBuxCode);
        }

        public static void Decode2(byte[] buffer)
        {
            Decode(buffer, bBuxCode2);
        }
    }

    public class WZExtraPacketEncodeS16Kor : IExtraEncoder
    {
        private byte[] buffer = new byte[16];
        public void Encoder(MemoryStream ms)
        {
            //Function 00CEA0CC
            BitConverter.GetBytes(0x67452301).CopyTo(buffer, 0);
            BitConverter.GetBytes(0xEFCDAB89).CopyTo(buffer, 4);
            BitConverter.GetBytes(0x98BADCEF).CopyTo(buffer, 8);
            BitConverter.GetBytes(0x10325476).CopyTo(buffer, 12);
            //End Function 00CEA0CC

            var initial = ms.Position;
            var length = ms.Length - ms.Position;
            var tmp = new byte[length];
            ms.Read(tmp, 0, (int)length);
            for(var i =0; i < length; i++)
            {
                tmp[i] ^= buffer[i%16];
            }
            ms.Position = initial;
            ms.Write(tmp, 0, (int)length);
        }
    }
}
