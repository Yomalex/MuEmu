using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.QuestSystem
{
    public interface IQuestMessage
    { }

    public class QuestSystemMessageFactory : MessageFactory<QuestOpCode, IQuestMessage>
    {
        public QuestSystemMessageFactory()
        {
            // C2S
            Register<CSetQuestState>(QuestOpCode.SetQuestState);
            Register<CQuestEXP>(QuestOpCode.QuestSwitchListNPC);
            Register<CQuestEXPProgress>(QuestOpCode.QuestExpInfo);
            Register<CQuestEXPComplete>(QuestOpCode.QuestExpComplete); 
            Register<CNewQuestInfo>(QuestOpCode.QuestEXPProgress);

            // S2C
            Register<SSetQuest>(QuestOpCode.SetQuest);
            Register<SSendQuestPrize>(QuestOpCode.QuestPrize);
            Register<SSetQuestState>(QuestOpCode.SetQuestState);
            Register<SQuestSwitchListNPC>(QuestOpCode.QuestSwitchListNPC);
            Register<SQuestEXP>(QuestOpCode.QuestExp);
            Register<SSendQuestEXPProgress>(QuestOpCode.QuestExpInfo);
            Register<SSendQuestEXPProgressAsk>(QuestOpCode.QuestExpInfoAsk);
            Register<SSendQuestEXPInfo>(QuestOpCode.QuestEXPProgress);
            Register<SSendQuestEXPComplete>(QuestOpCode.QuestExpComplete);
        }
    }
}
