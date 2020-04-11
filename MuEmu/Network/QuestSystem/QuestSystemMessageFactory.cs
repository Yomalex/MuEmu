using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.QuestSystem
{
    public interface IQuestMessage
    { }

    public class QuestSystemMessageFactory : MessageFactory<QuestOpCode, IQuestMessage>
    {
        public QuestSystemMessageFactory()
        {
            // C2S
            Register<CSetQuestState>(QuestOpCode.SetQuestState);

            // S2C
            Register<SSetQuest>(QuestOpCode.SetQuest);
            Register<SSendQuestPrize>(QuestOpCode.QuestPrize);
            Register<SSetQuestState>(QuestOpCode.SetQuestState);
        }
    }
}
