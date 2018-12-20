using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.QuestSystem
{
    [WZContract]
    public class CSetQuestState : IQuestMessage
    {
        [WZMember(0)] public byte Index { get; set; }
        [WZMember(1)] public byte State { get; set; }
    }
}
