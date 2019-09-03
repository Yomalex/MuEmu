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
        [WZMember(0)] public int GuildNumber { get; set; }    // 4
        [WZMember(1)] public byte btGuildType { get; set; }   // 8
        [WZMember(2,8)] public byte[] btUnionName { get; set; }  // 9
        [WZMember(3,8)] public byte[] btGuildName { get; set; }  // 11
        [WZMember(4,32)] public byte[] Mark { get; set; }	// 19

        public string UnionName { get => btUnionName.MakeString(); set => btUnionName = value.GetBytes(); }
        public string GuildName { get => btGuildName.MakeString(); set => btGuildName = value.GetBytes(); }
    }

    [WZContract(LongMessage = true)]
    public class SGuildList : IGuildMessage
    {
        [WZMember(0)] public byte Result { get; set; }    // 4
        [WZMember(1)] public byte Count { get; set; } // 5
        [WZMember(2)] public int TotalScore { get; set; } // 8
        [WZMember(3)] public byte Score { get; set; } // C
        [WZMember(4,9)] public byte[] szRivalGuild { get; set; }	// D

        [WZMember(5, SerializerType = typeof(ArraySerializer))]
        public GuildListDto[] Members { get; set; }

        public SGuildList()
        {
            Members = Array.Empty<GuildListDto>();
            szRivalGuild = Array.Empty<byte>();
        }
    }
}
