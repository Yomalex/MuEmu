using MuEmu.Util;
using Serilog;
using System;
using System.Collections.Generic;
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
    public abstract class Event : StateMachine<EventState>
    {
        protected ILogger _logger;
        protected TimeSpan _closedTime = TimeSpan.FromMinutes(2 * 60);
        protected TimeSpan _openedTime = TimeSpan.FromMinutes(5);
        protected TimeSpan _playingTime = TimeSpan.FromMinutes(15);
        protected List<PlayerEventInfo> _players = new List<PlayerEventInfo>();

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

        public Event(TimeSpan close, TimeSpan open, TimeSpan playing)
        {
            _closedTime = close;
            _openedTime = open;
            _playingTime = playing;
        }

        public override void Initialize()
        {
            _closedTime -= _openedTime + _playingTime;
            Trigger(EventState.Closed);
        }
        public abstract void NPCTalk(Player plr);
        public abstract bool TryAdd(Player plr);
        public abstract void OnPlayerDead(object sender, EventArgs eventArgs);
        public abstract void OnPlayerLeave(object sender, EventArgs eventArgs);
        public abstract void OnMonsterDead(object sender, EventArgs eventArgs);
    }
}
