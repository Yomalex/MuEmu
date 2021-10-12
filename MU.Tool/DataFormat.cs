using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Tool
{
    [WZContract]
    public class ServerList
    {
        [WZMember(0)] public ushort group { get; set; }
        [WZMember(1, typeof(BinaryStringSerializer), 13)] public string Name { get; set; }
        [WZMember(2, typeof(BinarySerializer), 20)] public byte[] Data { get; set; }
        [WZMember(3)] public byte d35 { get; set; }
        [WZMember(4)] public byte d36 { get; set; }
        [WZMember(5)] public byte descriptionLength { get; set; }
        [WZMember(6)] public byte d38 { get; set; }
        //[WZMember(5, typeof(ArrayWithScalarSerializer<byte>))] public byte[] desc { get; set; }
    }
}
