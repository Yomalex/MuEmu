using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    public class CharClass
    {
        public int Base { get; set; }
        public int Evolution { get; set; }
        public int Action { get; set; }

        public static explicit operator byte(CharClass @char)
        {
            return (byte)(@char.Base << 5 | @char.Evolution << 4 | @char.Action);
        }
    }
    [WZContract]
    public class CharsetDto
    {
        [WZMember(0)]
        public byte Class { get; set; }

        [WZMember(1, 17)]
        public byte[] CharSet { get; set; }
    }
}
