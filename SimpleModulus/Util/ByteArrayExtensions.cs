using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebZen.Util
{
    public static class ByteArrayExtensions
    {
        public static string GetHex(this byte[] byteArray)
        {
            return string.Join("", byteArray.Select(x => x.ToString("X2")));
        }
    }
}
