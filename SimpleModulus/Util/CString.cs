using System;
using System.Collections.Generic;
using System.Text;

namespace WebZen.Util
{
    public static class CString
    {
        public static string MakeString(this byte[] stringBytes)
        {
            return Encoding.ASCII.GetString(stringBytes).TrimEnd((char)0);
        }

        public static byte[] GetBytes(this string String)
        {
            return Encoding.ASCII.GetBytes(String);
        }
    }
}
