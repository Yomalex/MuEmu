using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebZen.Util
{
    public static class StreamExtensions
    {
        public static short ReadShort(this Stream stream)
        {
            var ret = new byte[2];
            stream.Read(ret, 0, 2);
            return BitConverter.ToInt16(ret, 0);
        }
        public static ushort ReadUShort(this Stream stream)
        {
            var ret = new byte[2];
            stream.Read(ret, 0, 2);
            return BitConverter.ToUInt16(ret, 0);
        }
        public static int ReadInt(this Stream stream)
        {
            var ret = new byte[4];
            stream.Read(ret, 0, 4);
            return BitConverter.ToInt32(ret, 0);
        }
        public static uint ReadUInt(this Stream stream)
        {
            var ret = new byte[4];
            stream.Read(ret, 0, 4);
            return BitConverter.ToUInt32(ret, 0);
        }
    }
}
