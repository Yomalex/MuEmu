using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using BlubLib.IO;
using System.ComponentModel.DataAnnotations;

namespace WebZen.Util
{
    public static class CString
    {
        public static string MakeString(this byte[] stringBytes)
        {
            var id = Array.FindIndex(stringBytes, x => x == (byte)0);
            //var id = stringBytes.FirstOrDefault(x => x == 0);
            if (id == -1) id = (byte)stringBytes.Length;
            return Encoding.ASCII.GetString(stringBytes, 0, id);
        }

        public static byte[] GetBytes(this string String)
        {
            if (String == null)
                return Array.Empty<byte>();

            return Encoding.ASCII.GetBytes(String);
        }

        public static void WriteString(this BinaryWriter w, string value, int length)
        {
            var a = new byte[length];

            if(value != null)
                Array.Copy(value.GetBytes(), a, Math.Min(length, value.Length));

            w.Write(a, 0, length);
        }

        public static string ReadString(this BinaryReader w, int length)
        {
            return w.ReadBytes(length).MakeString();
        }
    }
}
