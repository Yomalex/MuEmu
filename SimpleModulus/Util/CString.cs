using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
            return Encoding.ASCII.GetBytes(String);
        }
    }
}
