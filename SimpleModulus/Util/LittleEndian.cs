using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Serialization;

namespace WebZen.Util
{
    [WZContract]
    public class ushortle
    {
        [WZMember(0, typeof(ArraySerializer))] public byte[] data { get; set; } = new byte[2] { 0, 0 };
        public ushortle()
        {
            Set(0);
        }
        public ushortle(ushort value)
        {
            Set(value);
        }
        public void Set(ushort value)
        {
            data = BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public ushort Get()
        {
            return BitConverter.ToUInt16(data.Reverse().ToArray(), 0);
        }

        public static implicit operator ushortle(ushort value)
        {
            var r = new ushortle();
            r.Set(value);
            return r;
        }

        public static implicit operator ushortle(int value)
        {
            var r = new ushortle();
            r.Set((ushort)value);
            return r;
        }

        public static implicit operator ushort(ushortle value)=> value.Get();
    }

    [WZContract]
    public class uintle
    {
        [WZMember(0, typeof(ArraySerializer))] public byte[] data { get; set; }
        public void Set(uint value)
        {
            data = BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public uint Get()
        {
            return BitConverter.ToUInt32(data.Reverse().ToArray(), 0);
        }

        public static implicit operator uintle(uint value)
        {
            var r = new uintle();
            r.Set(value);
            return r;
        }

        public static implicit operator uint(uintle value) => value.Get();
    }
}
