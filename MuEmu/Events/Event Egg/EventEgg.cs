using MU.Network.Event;
using MU.Resources;
using MuEmu.Monsters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Serilog;
using Serilog.Core;

namespace MuEmu.Events.Event_Egg
{
    public class EventEgg : Event
    {
        private IEnumerable<Monster> _monsters;
        private List<TimeSpan> _spans = new List<TimeSpan>
        {
            TimeSpan.FromMinutes(1),//00:01
            TimeSpan.FromMinutes(20),
            TimeSpan.FromMinutes(40),

            TimeSpan.FromMinutes(780),//13:00
            TimeSpan.FromMinutes(800),
            TimeSpan.FromMinutes(820),

            TimeSpan.FromMinutes(1260),//21:00
            TimeSpan.FromMinutes(1280),
            TimeSpan.FromMinutes(1300),

            TimeSpan.FromMinutes(1320),//22:00
            TimeSpan.FromMinutes(1340),
            TimeSpan.FromMinutes(1360),

            TimeSpan.FromMinutes(1380),//23:00
            TimeSpan.FromMinutes(1400),
            TimeSpan.FromMinutes(1420),
        };

        public EventEgg()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(EventEgg));
        }

        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Open);
            Program.server.Connect += Server_Connect;
        }

        private void Server_Connect(object sender, Network.WZServerEventArgs e)
        {
            e.session.Player.OnStatusChange += Player_OnStatusChange;
        }

        private void Player_OnStatusChange(object sender, EventArgs e)
        {
            var plr = sender as Player;
            if(plr.Status == LoginStatus.Playing && CurrentState != EventState.None)
            {
                _ = plr.Session.SendAsync(new SSendBanner { Type = BannerType.EvenInven });
            }
            if(plr.Status == LoginStatus.NotLogged)
            {
                plr.OnStatusChange -= Player_OnStatusChange;
            }
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private TimeSpan GetTimeToNextSpaw()
        {
            var currentTime = DateTime.Now - DateTime.Today;
            try
            {
                var nextTime = _spans.First(x => x > currentTime);
                return DateTime.Today.Add(nextTime) - DateTime.Now;
            }catch(Exception)
            {
                return DateTime.Today.Add(_spans.First()) - DateTime.Now;
            }
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Open:
                    _monsters = from mob in MonstersMng.Instance.Monsters
                                where mob.Info.Monster == 674 || mob.Info.Monster == 675 || mob.Info.Monster == 676
                                select mob;
                    foreach(var mob in _monsters)
                    {
                        mob.Die += OnMonsterDead;
                    }
                    Trigger(EventState.Playing, GetTimeToNextSpaw());
                    break;
                case EventState.Playing:
                    {
                        foreach(var mob in _monsters)
                        {
                            mob.Active = true;
                        }
                        Trigger(EventState.Closed, TimeSpan.FromMinutes(19));
                    }
                    break;
                case EventState.Closed:
                    Trigger(EventState.Playing, GetTimeToNextSpaw());
                    break;
            }
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }
    }
}
