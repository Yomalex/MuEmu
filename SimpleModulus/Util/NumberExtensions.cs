using System;
using System.Collections.Generic;
using System.Text;

namespace WebZen.Util
{
    public static class NumberExtensions
    {
        public static byte[] Shufle(byte[] array)
        {
            var output = new byte[array.Length];

            for(int i = array.Length - 1, j = 0; i >= 0 && j < array.Length; i--, j++)
            {
                output[j] = array[i];
            }

            return output;
        }

        public static ushort ShufleEnding(this ushort number)
        {
            return BitConverter.ToUInt16(Shufle(BitConverter.GetBytes(number)), 0);
        }

        public static short ShufleEnding(this short number)
        {
            return BitConverter.ToInt16(Shufle(BitConverter.GetBytes(number)), 0);
        }

        public static uint ShufleEnding(this uint number)
        {
            return BitConverter.ToUInt32(Shufle(BitConverter.GetBytes(number)), 0);
        }

        public static int ShufleEnding(this int number)
        {
            return BitConverter.ToInt32(Shufle(BitConverter.GetBytes(number)), 0);
        }

        public static ulong ShufleEnding(this ulong number)
        {
            return BitConverter.ToUInt64(Shufle(BitConverter.GetBytes(number)), 0);
        }

        public static long ShufleEnding(this long number)
        {
            return BitConverter.ToInt64(Shufle(BitConverter.GetBytes(number)), 0);
        }
    }
}
