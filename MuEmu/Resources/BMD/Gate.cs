using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Resources.BMD
{
    [WZContract]
    public class GateBMD
    {
        [WZMember(0)] public GateType Flag { get; set; }
        [WZMember(1)] public Maps Map { get; set; }
        [WZMember(2)] public byte X1 { get; set; }
        [WZMember(3)] public byte Y1 { get; set; }
        [WZMember(4)] public byte X2 { get; set; }
        [WZMember(5)] public byte Y2 { get; set; }
        [WZMember(6)] public ushort GateNumber { get; set; }
        [WZMember(7)] public byte Dir { get; set; }
        [WZMember(8)] public byte BZone { get; set; }
        [WZMember(9)] public ushort Level { get; set; }
        [WZMember(10)] public ushort BZLevel { get; set; }
    }
}
