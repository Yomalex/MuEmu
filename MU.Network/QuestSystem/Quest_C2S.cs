using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MU.Network.QuestSystem
{
    [WZContract]
    public class CSetQuestState : IQuestMessage
    {
        [WZMember(0)] public byte Index { get; set; }
        [WZMember(1)] public byte State { get; set; }
    }
    [WZContract]
    public class CQuestEXP : IQuestMessage
    {
        [WZMember(0)] public uint Index { get; set; }
        [WZMember(1)] public byte Result { get; set; }
    }
    [WZContract]
    public class CQuestEXPProgress : IQuestMessage
    {
        [WZMember(0)] public uint Index { get; set; }
        [WZMember(1)] public byte Result { get; set; }
    }
    [WZContract]
    public class CQuestEXPComplete : IQuestMessage
    {
        [WZMember(0)] public uint Index { get; set; }
    }

    [WZContract]
    public class CQuestEXPProgressList : IQuestMessage
    { }

    [WZContract]
    public class CQuestEXPEventItemEPList : IQuestMessage
    { }

    [WZContract]
    public class CNewQuestInfo : IQuestMessage
    {
        [WZMember(0)]
        public uint dwQuestInfoIndexID { get; set; }
    }

    [WZContract]
    public class CQuestNPCTalk : IQuestMessage
    {
        [WZMember(0)] public ushort NPC { get;set; }
    }
}
