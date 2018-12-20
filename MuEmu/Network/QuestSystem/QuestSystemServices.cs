using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network.QuestSystem
{
    public class QuestSystemServices : MessageHandler
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(QuestSystemServices));

        [MessageHandler(typeof(CSetQuestState))]
        public void CSetQuestState(GSSession session, CSetQuestState message)
        {
            var quests = session.Player.Character.Quests;
            var result = quests.SetState(message.Index);

            session.SendAsync(new SSetQuestState(message.Index, result, quests.GetByIndex(message.Index).StateByte));
        }
    }
}
