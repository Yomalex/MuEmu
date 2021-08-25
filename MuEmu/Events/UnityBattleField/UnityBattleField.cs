using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Events.UnityBattleField
{
    public class UnityBattleField : Event
    {
        public UnityBattleField()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(UnityBattleField));
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnTransition(EventState NextState)
        {
            throw new NotImplementedException();
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }
    }
}
