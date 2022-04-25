using MU.Resources;
using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MU.Network.QuestSystem
{
    [WZContract]
    public class SSetQuest : IQuestMessage
    {
        [WZMember(0)] public byte Index { get; set; }
        [WZMember(1)] public byte State { get; set; }
    }
    [WZContract]
    public class SSetQuestState : IQuestMessage
    {
        [WZMember(0)] public byte Index { get; set; }
        [WZMember(1)] public byte Result { get; set; }
        [WZMember(2)] public byte State { get; set; }

        public SSetQuestState()
        {

        }

        public SSetQuestState(byte id, byte result, byte state)
        {
            Index = id;
            Result = result;
            State = state;
        }
    }
    [WZContract]
    public class SSendQuestPrize : IQuestMessage
    {
        [WZMember(0)] public ushort Number { get; set; }
        [WZMember(1)] public QuestCompensation Type { get; set; }
        [WZMember(2)] public byte Count { get; set; }

        public SSendQuestPrize()
        {

        }

        public SSendQuestPrize(ushort id, QuestCompensation type, byte count)
        {
            Number = id.ShufleEnding();
            Type = type;
            Count = count;
        }
    }

    [WZContract]
    public class SQuestSwitchListNPC : IQuestMessage
    {
        [WZMember(0)] public ushort NPC { get; set; }
        [WZMember(1, serializerType:typeof(ArrayWithScalarSerializer<ushort>))]
        public uint[] QuestList { get; set; }
    }

    [WZContract]
    public class SQuestSwitchListEvent : IQuestMessage
    {
        [WZMember(0)] public ushort NPC { get; set; }
        [WZMember(1, serializerType: typeof(ArrayWithScalarSerializer<ushort>))]
        public uint[] QuestList { get; set; }
    }

    [WZContract]
    public class SQuestSwitchListItem : IQuestMessage
    {
        [WZMember(0)] public ushort NPC { get; set; }
        [WZMember(1, serializerType: typeof(ArrayWithScalarSerializer<ushort>))]
        public uint[] QuestList { get; set; }
    }

    [WZContract]
    public class SQuestEXP : IQuestMessage
    {
        [WZMember(0)] public byte Result { get; set; }
    }

    [WZContract]
    public class SSendQuestEXPProgress : IQuestMessage
    {
        [WZMember(0)] public uint dwQuestInfoIndexID { get; set; }
        /*[WZMember(1)] public byte AskCnt { get; set; }
        [WZMember(2)] public byte RewardCnt { get; set; }
        [WZMember(3)] public byte RandRewardCnt { get; set; }*/
    }

    [WZContract]
    public class SSendQuestEXPProgressAsk : IQuestMessage
    {
        [WZMember(0)] public uint dwQuestInfoIndexID { get; set; }
        [WZMember(1)] public byte AskCnt { get; set; }
        [WZMember(2)] public byte RewardCnt { get; set; }
        [WZMember(3)] public byte RandRewardCnt { get; set; }
        [WZMember(5, typeof(ArraySerializer))] public AskInfoDto[] Asks { get; set; }
        [WZMember(6, typeof(ArraySerializer))] public RewardInfoDto[] Rewards { get; set; }
    }

    [WZContract]
    public class AskInfoDto
    {
        [WZMember(0)] public AskType Type { get; set; }
        [WZMember(1)] public ushort Index { get; set; }
        [WZMember(2)] public uint Value { get; set; }
        [WZMember(3)] public uint CurrentValue { get; set; }
        [WZMember(4, typeof(BinarySerializer), 12)] public byte[] ItemInfo { get; set; } = Array.Empty<byte>();
    }

    [WZContract]
    public class RewardInfoDto
    {
        [WZMember(0)] public RewardType Type { get; set; }
        [WZMember(1)] public ushort Index { get; set; }
        [WZMember(2)] public uint Value { get; set; }
        [WZMember(4, typeof(BinarySerializer), 12)] public byte[] ItemInfo { get; set; } = Array.Empty<byte>();
    }

    [WZContract]
    public class SSendQuestEXPComplete : IQuestMessage
    {
        [WZMember(0)] public uint dwQuestInfoIndexID { get; set; }
        [WZMember(1)] public byte Result { get; set; }
    }

    [WZContract]
    public class SSendQuestEXPInfo : SSendQuestEXPProgressAsk
    { }

    [WZContract]
    public class SQuestEXPProgressList : IQuestMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<byte>))] public uint[] QuestInfoIndexID { get; set; }
    }

    [WZContract]
    public class QuestNPCTalkDto : IQuestMessage
    {
        [WZMember(0)] public uint QuestInfoIndex { get; set; }
        //[WZMember(1)] public ushort QuestID { get; set; }
        [WZMember(4)] public QuestState State { get; set; }
        [WZMember(5)] public byte unk { get; set; }
    }

    [WZContract]
    public class SQuestNPCTalk : IQuestMessage
    {
        [WZMember(0, typeof(ArrayWithScalarSerializer<ushort>))] public QuestNPCTalkDto[] QuestList { get; set; }
    }

    [WZContract]
    public class SQuestNPCAccept : IQuestMessage
    {
        [WZMember(0)] public uint QuestInfoIndex { get; set; }
        [WZMember(1)] public byte Result { get; set; }
    }
}
