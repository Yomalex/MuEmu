﻿using System;
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
            Register<CSetQuestState>(QuestOpCode.SetQuestStateRes);
            Register<CQuestEXP>(QuestOpCode.QuestSwitchListNPC);
            Register<CQuestEXPProgressList>(QuestOpCode.QuestExpProgressList);
            Register<CQuestEXPProgress>(QuestOpCode.QuestExpInfo);
            Register<CQuestEXPComplete>(QuestOpCode.QuestExpComplete); 
            Register<CNewQuestInfo>(QuestOpCode.QuestEXPProgress);
            Register<CQuestEXPEventItemEPList>(QuestOpCode.QuestEXPEventItemEPList);

            Register<CQuestNPCTalk>(QuestOpCode.QuestMUTalk);
            Register<SQuestNPCTalk>(QuestOpCode.QuestMUTalk);

            Register<CQuestNPCAccept>(QuestOpCode.QuestMUAccept);
            Register<SQuestNPCAccept>(QuestOpCode.QuestMUAccept);

            Register<CCentTestStart>(QuestOpCode.CentQuestTest);
            Register<CCentBattleStart>(QuestOpCode.CentQuestSummon);
            Register<CCentMove>(QuestOpCode.CentQuestMove);

            // S2C
            Register<SSetQuest>(QuestOpCode.SetQuest);
            Register<SMonsterKillS16>(QuestOpCode.QuestKillCount);
            Register<SSendQuestPrize>(QuestOpCode.QuestPrize);
            Register<SSetQuestState>(QuestOpCode.SetQuestState);
            Register<SQuestSwitchListNPC>(QuestOpCode.QuestSwitchListNPC);
            Register<SQuestSwitchListEvent>(QuestOpCode.QuestSwitchListEvent);
            Register<SQuestSwitchListItem>(QuestOpCode.QuestSwitchListItem);
            Register<SQuestEXP>(QuestOpCode.QuestExp);
            Register<SQuestEXPProgressList>(QuestOpCode.QuestExpProgressList);
            Register<SSendQuestEXPProgress>(QuestOpCode.QuestExpInfo);
            Register<SSendQuestEXPProgressAsk>(QuestOpCode.QuestExpInfoAsk);
            Register<SSendQuestEXPInfo>(QuestOpCode.QuestEXPProgress);
            Register<SSendQuestEXPComplete>(QuestOpCode.QuestExpComplete);
            Register<SQuestSurvivalTime>(QuestOpCode.QuestSurvivalTime);
        }
    }
}
