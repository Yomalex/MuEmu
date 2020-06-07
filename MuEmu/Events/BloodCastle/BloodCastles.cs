using MuEmu.Network.Game;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.BloodCastle
{    internal struct BloodCastleInfo
    {
        public byte Bridge { get; set; }
        public ushort MinLevel { get; set; }
        public ushort MaxLevel { get; set; }
    }
    public class BloodCastles : Event
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(BloodCastles));

        private BloodCastle[] _bridges;

        public BloodCastles()
            :base(TimeSpan.FromHours(2), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(17))
        {
            _eventLevelReqs = new List<EventLevelReq>
            {
                new EventLevelReq{ Min = 15, Max = 80 },
                new EventLevelReq{ Min = 81, Max = 130 },
                new EventLevelReq{ Min = 131, Max = 180 },
                new EventLevelReq{ Min = 181, Max = 230 },
                new EventLevelReq{ Min = 231, Max = 280 },
                new EventLevelReq{ Min = 281, Max = 330 },
                new EventLevelReq{ Min = 331, Max = 400 },
                new EventLevelReq{ Min = 0, Max = 0 },
            };

            _bridges = new BloodCastle[8];
            for (var i = 0; i < _bridges.Length; i++)
            {
                _bridges[i] = new BloodCastle(this, i, _closedTime, _openTime, _playingTime);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            foreach (var bridge in _bridges)
                bridge.Initialize();
        }

        private static IEnumerable<Player> PlayersOut()
        {
            return Program.server.Clients
                                .Where(x => x.Player != null)
                                .Where(x => x.Player.Character != null)
                                .Select(x => x.Player)
                                .Where(x => (x.Character.MapID < Maps.BloodCastle1 || x.Character.MapID > Maps.BloodCastle7) && x.Character.MapID != Maps.BloodCastle8);
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case EventState.Closed:
                    break;
                case EventState.Open:
                    if (((int)TimeLeft.TotalSeconds) % 60 == 0 && TimeLeft.TotalSeconds >= 60)
                        Program.NoEventMapAnoucement(Program.ServerMessages.GetMessage(Messages.BC_Open, (int)Math.Ceiling(TimeLeft.TotalMinutes))).Wait();
                    break;
                case EventState.Playing:
                    Program.NoEventMapAnoucement(Program.ServerMessages.GetMessage(Messages.BC_Closed)).Wait();
                    break;
            }

            foreach(var bridge in _bridges)
            {
                bridge.Update();
            }

            base.Update();
        }
        
        public void MessengerAngelTalk(Player plr)
        {
            if (CurrentState != EventState.Open)
            {
                plr.Session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.RunningBC)).Wait();
                return;
            }

            plr.Session.SendAsync(new STalk { Result = NPCWindow.MessengerAngel }).Wait();
        }

        public void AngelKingTalk(Player plr)
        {
            var session = plr.Session;
            var bridge = _bridges.FirstOrDefault(x => x.Players.Any(y => y == plr));

            if (bridge == null)
            {
                session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC)).Wait();
                return;
            }

            if(CurrentState == EventState.Playing && TimeLeft > TimeSpan.FromMinutes(4))
            {
                //await session.SendAsync(new SNotice(NoticeType.Blue, $"When the clock has 3:00 for finish give the weapon, not now."));
                session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC)).Wait();
                return;
            }

            if((int)TimeLeft.TotalSeconds == 60)
            {
                session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.CompletedBC)).Wait();
                return;
            }

            var item = plr.Character.Inventory.FindAllItems(new ItemNumber(13, 19)).FirstOrDefault();

            if (item == null)
            {
                session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC)).Wait();
                return;
            }

            plr.Character.Inventory.Delete(item).Wait();

            session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.SucceedBC)).Wait();
            bridge.Winner = plr;
        }

        public override bool TryAdd(Player plr)
        {
            var bridge = GetEventNumber(plr);

            return _bridges[bridge - 1].TryAdd(plr);
        }

        public override void NPCTalk(Player plr)
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

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
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
                    Trigger(EventState.Playing, _openTime);
                    break;

                case EventState.Playing:
                    Trigger(EventState.Closed, _playingTime);
                    break;
            }
        }
    }
}
