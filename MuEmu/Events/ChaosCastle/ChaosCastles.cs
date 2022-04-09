using MU.Network.Game;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MuEmu.Events.ChaosCastle
{
    public class ChaosCastles : Event
    {
        private List<ChaosCastle> _castles = new List<ChaosCastle>();
        public const int MaxPlayers = 70;
        public static Rectangle Ground = new Rectangle(23, 75, 21, 33);
        public static int[] EntryFee = new int[]
        {
            25000, 80000, 150000, 250000, 400000, 650000, 1000000
        };
        public static int[] PlayerKillExp = new int[]
        {
            0x1F4, 0x3E8, 0x5DC, 0x7D0, 0x9C4, 0xBB8, 0xFA0
        };
        public static int[] MonsterKillExp = new int[]
        {
            0x3E8, 0x5DC, 0x7D0, 0x9C4, 0xBB8, 0xDAC, 0x1194
        };
        public static List<ItemNumber[]> Rewards = new List<ItemNumber[]>
        {
            new ItemNumber[] { new ItemNumber(14,13), new ItemNumber(14,14) }, //1
            new ItemNumber[] { new ItemNumber(14,22), new ItemNumber(14,16), new ItemNumber(ItemNumber.Invalid), }, //2
            new ItemNumber[] { new ItemNumber(14,22), new ItemNumber(14,16), new ItemNumber(ItemNumber.Invalid), }, //3
            new ItemNumber[] { new ItemNumber(14,22), new ItemNumber(14,16), new ItemNumber(ItemNumber.Invalid), }, //4
            new ItemNumber[] { new ItemNumber(14,22), new ItemNumber(14,16), new ItemNumber(ItemNumber.Invalid), }, //5
            new ItemNumber[] { new ItemNumber(14,22), new ItemNumber(14,16), new ItemNumber(ItemNumber.Invalid), }, //6
            new ItemNumber[] { new ItemNumber(14,22), new ItemNumber(14,16), new ItemNumber(ItemNumber.Invalid), }, //7
        };
        public ChaosCastles()
            : base(TimeSpan.FromHours(4), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(11))
        {
            _eventLevelReqs = new List<EventLevelReq>
            {
                new EventLevelReq{ Min = 15, Max = 49 },
                new EventLevelReq{ Min = 50, Max = 119 },
                new EventLevelReq{ Min = 120, Max = 179 },
                new EventLevelReq{ Min = 180, Max = 239 },
                new EventLevelReq{ Min = 240, Max = 299 },
                new EventLevelReq{ Min = 300, Max = 400 },
                new EventLevelReq{ Min = 0, Max = 0 },
            };

            for(var i = 0; i < 7; i++)
            {
                _castles.Add(new ChaosCastle(i, _closedTime, _openTime, _playingTime));
            }
        }
        public override void Initialize()
        {
            foreach (var c in _castles)
                c.Initialize();

            base.Initialize();
            Trigger(EventState.Open);
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
            switch(NextState)
            {
                case EventState.Closed:
                    Trigger(EventState.Open, _closedTime);
                    break;
                case EventState.Open:
                    Program.NoEventMapAnoucement($"{nameof(ChaosCastle)} is open")
                        .Wait();
                    Trigger(EventState.Playing, _openTime);
                    foreach (var cc in _castles)
                        cc.Trigger(EventState.Open);
                    break;
                case EventState.Playing:
                    Trigger(EventState.Closed, _playingTime);
                    break;
            }
            Program.NoEventMapSendAsync(new SEventNotificationS16Kor
            {
                Active = (byte)(NextState == EventState.Open?1:0),
                EventID = EventIcon.ChaosCastle
            });
        }

        public override void Update()
        {
            switch (CurrentState)
            {
                case EventState.Open:
                    if (((int)TimeLeft.TotalSeconds) % 60 == 0 && (int)TimeLeft.TotalSeconds > 0)
                    {
                        Program
                            .NoEventMapAnoucement($"Chaos Castle Will start in {(int)TimeLeft.TotalMinutes} minute(s).")
                            .Wait();
                        Program.NoEventMapSendAsync(new SEventNotificationS16Kor
                        {
                            Active = 1,
                            EventID = EventIcon.ChaosCastle
                        });
                    }
                    break;
            }

            foreach (var cc in _castles)
            {
                cc.Update();
            }
            base.Update();
        }

        public override bool TryAdd(Player plr)
        {
            var CCnumber = GetEventNumber(plr);

            if (CCnumber == 0)
                return false;

            var item = plr.Character.Inventory.FindAllItems(new ItemNumber(13, 29)).FirstOrDefault();
            if (item == null)
                return false;

            plr.Character.Inventory
                .Delete(item)
                .Wait();

            return _castles[CCnumber - 1].TryAdd(plr);
        }
    }
}
