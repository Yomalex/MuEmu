using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Network.QuestSystem
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
}
