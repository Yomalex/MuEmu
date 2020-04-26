using MuEmu.Network.Event;
using MuEmu.Network.Game;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.Kanturu
{
    public class Kanturu : Event
    {
        private enum KanturuBattleState
        {
            BattleNone,
            BattleOfMayaRight,
            BattleStandBy1,
            BattleOfMayaLeft,
            BattleStandBy2,
            BattleOfMaya,
            BattleStandBy3,
            BattleOfNightmare,
            BattleEnd,
        }

        private KanturuBattleState _battleState;
        private DateTimeOffset _lastAnouncement = DateTimeOffset.Now;
        private TimeSpan _battleTimer;
        private int _mobKill;

        public const int MaxPlayers = 15;

        public Kanturu()
            : base(TimeSpan.FromDays(1), TimeSpan.FromMinutes(2), TimeSpan.FromHours(1))
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Kanturu));
        }

        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Open);
        }

        public override void NPCTalk(Player plr)
        {
            var msg = new SKanturuStateInfo
            {
                State = KanturuState.None,
                btDetailState = 0,
                btEnter = 0,
                btUserCount = 0,
                iRemainTime = 100,
            };
            plr.Session.SendAsync(msg)
                .Wait();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var plr = sender as Player;
            //plr.Character.WarpTo();
            if (!CanRun())
                FailState();
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            if (!CanRun())
                FailState();
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Closed:
                    break;
                case EventState.Open:
                    Trigger(EventState.Playing, _openTime);
                    break;
                case EventState.Playing:
                    if (_players.Count == 0)
                        Trigger(EventState.Open);
                    break;
            }
        }

        private bool CanRun()
        {
            return _players.Any(x => x.Eventer) || EventState.Closed == CurrentState;
        }

        private void FailState()
        {

        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case EventState.Closed:
                    if (DateTimeOffset.Now.Hour == 0)
                        Trigger(EventState.Open);
                    break;
                case EventState.Open:
                    _battleState = KanturuBattleState.BattleNone;
                    if (_lastAnouncement > DateTimeOffset.Now)
                        break;

                    _lastAnouncement = DateTimeOffset.Now.AddMinutes(2);

                    Program.MapAnoucement(Maps.Kantru2, "[Refinery Tower] Can enter").Wait();
                    break;
                case EventState.Playing:
                    _battleTimer -= TimeSpan.FromSeconds(1);
                    switch (_battleState)
                    {
                        case KanturuBattleState.BattleNone:
                            _battleState = KanturuBattleState.BattleOfMayaRight;
                            _battleTimer = TimeSpan.FromMinutes(15);
                            break;
                        case KanturuBattleState.BattleOfMayaRight:
                            if(_battleTimer == TimeSpan.Zero)
                            {
                                FailState();
                                break;
                            }
                            if (_mobKill >= 40)
                            {
                                _battleState = KanturuBattleState.BattleStandBy1;
                                _battleTimer = TimeSpan.FromMinutes(2);
                            }
                            break;
                        case KanturuBattleState.BattleStandBy1:
                            if (_battleTimer == TimeSpan.Zero)
                            {
                                _battleState = KanturuBattleState.BattleOfMayaLeft;
                                _battleTimer = TimeSpan.FromMinutes(15);
                            }
                            break;
                        case KanturuBattleState.BattleOfMayaLeft:
                            if (_battleTimer == TimeSpan.Zero)
                            {
                                FailState();
                                break;
                            }
                            if (_mobKill >= 40)
                            {
                                _battleState = KanturuBattleState.BattleStandBy2;
                                _battleTimer = TimeSpan.FromMinutes(2);
                            }
                            break;
                        case KanturuBattleState.BattleStandBy2:
                            if (_battleTimer == TimeSpan.Zero)
                            {
                                _battleState = KanturuBattleState.BattleOfMaya;
                                _battleTimer = TimeSpan.FromMinutes(20);
                            }
                            break;
                        case KanturuBattleState.BattleOfMaya:
                            if (_battleTimer == TimeSpan.Zero)
                            {
                                FailState();
                                break;
                            }
                            if (_mobKill >= 20)
                            {
                                _battleState = KanturuBattleState.BattleStandBy3;
                                _battleTimer = TimeSpan.FromMinutes(2);
                            }
                            break;
                        case KanturuBattleState.BattleStandBy3:
                            if (_battleTimer == TimeSpan.Zero)
                            {
                                _battleState = KanturuBattleState.BattleOfNightmare;
                                _battleTimer = TimeSpan.FromMinutes(10);
                            }
                            break;
                        case KanturuBattleState.BattleOfNightmare:
                            if (_battleTimer == TimeSpan.Zero)
                            {
                                FailState();
                                break;
                            }
                            if (_mobKill >= 1)
                            {
                                _battleState = KanturuBattleState.BattleStandBy3;
                                _battleTimer = TimeSpan.FromMinutes(2);
                            }
                            break;
                    }
                    break;
            }
            base.Update();
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }
    }
}
