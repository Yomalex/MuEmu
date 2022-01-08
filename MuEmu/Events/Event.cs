using MU.Resources;
using MuEmu.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events
{
    public enum EventState
    {
        None,
        Closed,
        Open,
        Playing,
    }
    public class PlayerEventInfo
    {
        public Player Player { get; set; }
        public int Score { get; set; }
        public bool Eventer { get; set; }
    }
    public class EventLevelReq
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
    public abstract class Event : StateMachine<EventState>
    {
        //protected ILogger _logger;
        protected TimeSpan _closedTime = TimeSpan.FromMinutes(2 * 60);
        protected TimeSpan _openTime = TimeSpan.FromMinutes(5);
        protected TimeSpan _playingTime = TimeSpan.FromMinutes(15);
        protected List<PlayerEventInfo> _players = new List<PlayerEventInfo>();
        protected List<EventLevelReq> _eventLevelReqs;
        protected Random _random = new Random();

        public byte RemainTime { get
            {
                switch(CurrentState)
                {
                    case EventState.Closed:
                        return (byte)TimeLeft.TotalMinutes;
                    case EventState.Open:
                        return 0;
                    case EventState.Playing:
                        return (byte)Math.Min((TimeLeft.TotalMinutes + _closedTime.TotalMinutes),255);
                }

                return 0;
            }
        }

        public byte Count => (byte)_players.Count(x => x.Eventer);
        public Event()
        { }

        public Event(TimeSpan close, TimeSpan open, TimeSpan playing)
        {
            _closedTime = close;
            _openTime = open;
            _playingTime = playing;
        }

        public override void Initialize()
        {
            _closedTime -= _openTime + _playingTime;
            Trigger(EventState.Closed);
        }
        public abstract void NPCTalk(Player plr);
        public abstract bool TryAdd(Player plr);
        public abstract void OnPlayerDead(object sender, EventArgs eventArgs);
        public abstract void OnPlayerLeave(object sender, EventArgs eventArgs);
        public abstract void OnMonsterDead(object sender, EventArgs eventArgs);

        public int GetEventNumber(Player plr)
        {
            var lvl = plr.Character.Level;
            if (plr.Character.MasterClass)
                return _eventLevelReqs.Count;

            if (plr.Character.BaseClass == HeroClass.MagicGladiator || plr.Character.BaseClass == HeroClass.DarkLord)
            {
                lvl = (ushort)(lvl * 3.0f / 2.0f);
                lvl = Math.Min(lvl, (ushort)400);
            }

            return _eventLevelReqs.FindIndex(x => x.Min <= lvl && x.Max >= lvl)+1;
        }

        public virtual Item GetItem(ushort mobLevel, Maps map)
        {
            return null;
        }

        public PlayerEventInfo GetPlayerEventInfo(Player plr)
        {
            if (plr == null)
                throw new ArgumentNullException(nameof(plr));

            var info = _players.FirstOrDefault(x => x.Player == plr);
            if (info == null)
            {
                info = new PlayerEventInfo { Player = plr };
                _players.Add(info);
            }

            return info;
        }
    }
}
