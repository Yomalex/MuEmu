using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.Guild
{
    [WZContract(LongMessage = true)]
    public class SGuildViewPort : IGuildMessage
    {
        [WZMember(0, SerializerType = typeof(ArrayWithScalarSerializer<byte>))]
        public GuildViewPortDto[] Guilds { get; set; }

        public SGuildViewPort()
        {
            Guilds = Array.Empty<GuildViewPortDto>();
        }
    }

    [WZContract]
    public class SGuildMasterQuestion : IGuildMessage
    {

    }

    [WZContract]
    public class SGuildCreateResult : IGuildMessage
    {
        [WZMember(0)] public byte Result { get; set; }

        [WZMember(1)] public byte GuildType { get; set; }
    }

    [WZContract]
    public class SGuildAnsViewport : IGuildMessage
    {
        // 0xC1 // 0
        // Size // 1
        // 0x66 // 2
        [WZMember(0)] public byte padding { get; set; } // 3
        [WZMember(1)] public int GuildNumber { get; set; }    // 4
        [WZMember(2)] public byte btGuildType { get; set; }   // 8
        [WZMember(3,8)] public byte[] btUnionName { get; set; }  // 9
        [WZMember(4,8)] public byte[] btGuildName { get; set; }  // 11
        [WZMember(5,32)] public byte[] Mark { get; set; }	// 19

        public string UnionName { get => btUnionName.MakeString(); set => btUnionName = value.GetBytes(); }
        public string GuildName { get => btGuildName.MakeString(); set => btGuildName = value.GetBytes(); }
    }

    [WZContract(LongMessage = true)]
    public class SGuildList : IGuildMessage
    {
        [WZMember(0)] public byte Result { get; set; }    // 4
        [WZMember(1)] public byte Count { get; set; } // 5
        [WZMember(2)] public ushort Padding { get; set; } // 6, 7
        [WZMember(3)] public int TotalScore { get; set; } // 8, 9, A, B
        [WZMember(4)] public byte Score { get; set; } // C
        [WZMember(5,9)] public byte[] szRivalGuild { get; set; }	// D

        [WZMember(6)] public ushort Padding2 { get; set; }

        [WZMember(7, SerializerType = typeof(ArraySerializer))]
        public GuildListDto[] Members { get; set; }

        public SGuildList()
        {
            Members = Array.Empty<GuildListDto>();
            szRivalGuild = Array.Empty<byte>();
        }
    }

    [WZContract(LongMessage = true)]
    public class SGuildListS9 : IGuildMessage
    {
        [WZMember(0)] public byte Result { get; set; }    // 4
        [WZMember(1)] public byte Count { get; set; } // 5
        [WZMember(2)] public ushort Padding { get; set; } // 6, 7
        [WZMember(3)] public int TotalScore { get; set; } // 8, 9, A, B
        [WZMember(4)] public byte Score { get; set; } // C
        [WZMember(5, typeof(BinaryStringSerializer), 8)] public string RivalGuild1 { get; set; }	// D
        [WZMember(6, typeof(BinaryStringSerializer), 8)] public string RivalGuild2 { get; set; }	// D
        [WZMember(7, typeof(BinaryStringSerializer), 8)] public string RivalGuild3 { get; set; }	// D
        [WZMember(8, typeof(BinaryStringSerializer), 8)] public string RivalGuild4 { get; set; }	// D
        [WZMember(9, typeof(BinaryStringSerializer), 8)] public string RivalGuild5 { get; set; }	// D
        [WZMember(10)] public ushort Padding2 { get; set; }
        [WZMember(11)] public byte Padding3 { get; set; }

        [WZMember(12, SerializerType = typeof(ArraySerializer))]
        public GuildListDto[] Members { get; set; }

        public SGuildListS9()
        {
            Members = Array.Empty<GuildListDto>();
        }
    }

    [WZContract]
    public class SGuildResult : IGuildMessage
    {
        [WZMember(0)] public GuildResult Result { get; set; }

        public SGuildResult() { }
        public SGuildResult(GuildResult res) { Result = res; }
    }

    [WZContract]
    public class SGuildSetStatus : IGuildMessage
    {
        [WZMember(0)] public byte Type { get; set; }
        [WZMember(1)] public GuildResult Result { get; set; }
        [WZMember(2, 10)] public byte[] btName { get; set; }

        public string Name => btName.MakeString();

        public SGuildSetStatus() { }

        public SGuildSetStatus(byte type, GuildResult res, string name)
        {
            Type = type;
            Result = res;

            btName = name.GetBytes();
        }
    }

    [WZContract]
    public class SGuildRemoveUser : IGuildMessage
    {
        [WZMember(0)] public GuildResult Result { get; set; }

        public SGuildRemoveUser() { }

        public SGuildRemoveUser(GuildResult res)
        {
            Result = res;
        }
    }
}
