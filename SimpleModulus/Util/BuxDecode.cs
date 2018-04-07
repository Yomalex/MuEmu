using System;
using System.Collections.Generic;
using System.Text;

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
}
