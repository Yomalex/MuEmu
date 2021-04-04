using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.GensSystem
{
    [WZContract]
    public class SGensSendInfoS9 : IGensMessage
    {
        [WZMember(0)] public GensType Influence { get; set; }
        [WZMember(1)] public byte aligment1 { get; set; }
        [WZMember(2)] public byte aligment2 { get; set; }
        [WZMember(3)] public byte aligment3 { get; set; }
        [WZMember(4)] public int Ranking { get; set; }
        [WZMember(5)] public int Class { get; set; }
        [WZMember(6)] public int ContributePoint { get; set; }
        [WZMember(7)] public int NextContributePoint { get; set; }
    }
    [WZContract]
    public class SRequestJoin : IGensMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public GensType Influence { get; set; }
        [WZMember(2)] public ushort wzIndex { get; set; }
        public SRequestJoin()
        { }

        public SRequestJoin(byte result, GensType influence, ushort index)
        {
            Result = result;
            Influence = influence;
            wzIndex = index.ShufleEnding();
        }
    }
    [WZContract]
    public class SRegMember : IGensMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public GensType Influence { get; set; }
        public SRegMember()
        { }

        public SRegMember(byte result, GensType influence)
        {
            Result = result;
            Influence = influence;
        }
    }

    [WZContract]
    public class VPGensDto
    {
        [WZMember(0)] public GensType Influence { get; set; }     // +0x0(0x1)
        [WZMember(1)] public ushort wzNumber { get; set; }     // +0x1(0x2)
        [WZMember(2)] public byte aligment { get; set; }     // +0x3(0x1)
        [WZMember(3)] public int iGensRanking { get; set; }     // +0x4(0x4)
        [WZMember(4)] public int iGensClass { get; set; }       // +0x8(0x4)
        [WZMember(5)] public int iContributePoint { get; set; } // +0xc(0x4)
    }

    [WZContract(LongMessage = true)]
    public class SViewPortGens : IGensMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public VPGensDto[] VPGens { get; set; }
    }

    [WZContract]
    public class SGensLeaveResult : IGensMessage
    {
        [WZMember(0)] public byte Result { get; set; }
        [WZMember(1)] public ushort wIndex { get; set; }

        public SGensLeaveResult() { }
        public SGensLeaveResult(byte result, ushort Index)
        {
            Result = result;
            wIndex = Index.ShufleEnding();
        }
    }
}
