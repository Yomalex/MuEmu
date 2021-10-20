using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Events.Rummy
{
    public class MuRummy : Event
    {
        public override void Initialize()
        {
            base.Initialize();

            Trigger(EventState.None);
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
            //throw new NotImplementedException();
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }
    }
}
