using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;

namespace MuEmu.Network.QuestSystem
{
    public class QuestSystemServices : MessageHandler
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(QuestSystemServices));

        [MessageHandler(typeof(CSetQuestState))]
        public async Task CSetQuestState(GSSession session, CSetQuestState message)
        {
            var quests = session.Player.Character.Quests;
            var result = quests.SetState(message.Index);
            var serverState = quests.GetByIndex(message.Index).StateByte;

            await session.SendAsync(new SSetQuestState(message.Index, result, serverState));
        }
    }
}
