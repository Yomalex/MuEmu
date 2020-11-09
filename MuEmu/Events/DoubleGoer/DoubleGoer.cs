using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Events.DoubleGoer
{
    public class DoubleGoer : Event
    {
        private List<Point> _player;
        private List<Point> _monster;

        public DoubleGoer()
            :base(TimeSpan.Zero, TimeSpan.FromDays(100), TimeSpan.FromMinutes(10))
        {
            _player = new List<Point>
            {
                new Point{ X = 197, Y = 30 },
                new Point{ X = 133, Y = 68 },
                new Point{ X = 110, Y = 60 },
                new Point{ X = 95, Y = 15 },
            };

            _monster = new List<Point>
            {
                new Point{ X = 224, Y = 100 },
                new Point{ X = 113, Y = 180 },
                new Point{ X = 110, Y = 150 },
                new Point{ X = 43, Y = 108 },
            };
        }

        public override void Initialize()
        {
            base.Initialize();
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

        public override void Update()
        {
            base.Update();
            switch(CurrentState)
            {
                case EventState.Open:
                    if (_players.Count > 0)
                        Trigger(EventState.Playing);
                    break;
                case EventState.Playing:
                    if (_players.Count <= 0)
                        Trigger(EventState.Closed);


                    break;
            }
        }
    }
}
