using Serilog;
using Serilog.Core;
using MU.Network.Event;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MuEmu.Monsters;
using MuEmu.Resources.Map;
using MuEmu.Resources;
using System.Drawing;
using System.Threading.Tasks;
using MuEmu.Util;
using MU.Resources;
using MU.Network.Game;

namespace MuEmu.Events.ImperialGuardian
{
    internal class FortressGate
    {
        public ushort Class;
        public int X;
        public int Y;
        public byte Dir;
        public Monster monster;
        public Maps map;
    }

    enum FortressZoneState
    {
        Ready,
        BeginTimeAttack,
        BeginWaitPlayer,
        BeginLootTime,
        AllWarpNextZone,
        MissionFail,
        AllKick,
        MissionClear,
        None,
    }

    internal class ImperialZone : StateMachine<FortressZoneState>
    {
        public ushort Zone { get; }
        public bool IsLastZone => (Zone == 3 && _manager._eventDay == DayOfWeek.Sunday) || (Zone == 2 && _manager._eventDay != DayOfWeek.Sunday);
        public bool AbleUsePortal { get; private set; }
        private ImperialGuardian _manager;
        private List<Player> _players;
        private int _lastNotify;

        public override void Initialize()
        {
            Trigger(FortressZoneState.Ready);
        }

        public ImperialZone(ImperialGuardian manager, ushort zone)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ImperialZone)+ zone.ToString());
            _manager = manager;
            Zone = zone;
            _players = new List<Player>();
        }

        void NotifyZoneClear(uint type)
        {
            var msg = new SImperialNotifyZoneClear
            {
                Type = type,
                RewardExp = 100,
            };

            switch(type)
            {
                case 0:
                    msg.RewardExp = 0;
                    break;
                case 2:
                    break;
            }

            _players.Select(x => x.Session).SendAsync(msg);
        }

        public override void OnTransition(FortressZoneState NextState)
        {
            switch(NextState)
            {
                case FortressZoneState.Ready:
                    AbleUsePortal = false;
                    if (_manager.CurrentState != EventState.Open)
                    {
                        Trigger(FortressZoneState.AllKick);
                    }
                    break;
                case FortressZoneState.BeginTimeAttack:
                    Trigger(FortressZoneState.MissionFail, TimeSpan.FromMinutes(10));
                    _manager.LoadMonsters(Zone);
                    _manager.Zone = Zone;
                    break;
                case FortressZoneState.BeginWaitPlayer:
                    Trigger(FortressZoneState.BeginTimeAttack, TimeSpan.FromMinutes(1));
                    _manager.UnloadMonsters((ushort)(Zone - 1));
                    break;
                case FortressZoneState.BeginLootTime:
                    AbleUsePortal = true;
                    _manager.NextZone?.Trigger(FortressZoneState.BeginWaitPlayer);
                    Trigger(IsLastZone?FortressZoneState.AllKick:FortressZoneState.MissionFail, TimeSpan.FromMinutes(1));

                    _ = _manager._map.SendAsync(new SNotice(NoticeType.Gold, "[IG] BeginLootTime1"));
                    _ = _manager._map.SendAsync(new SNotice(NoticeType.Gold, "[IG] BeginLootTime2"));
                    break;
                case FortressZoneState.AllWarpNextZone:
                    _players.ForEach(x => x.Character.WarpTo(22));
                    //Trigger(FortressZoneState.Ready);
                    break;
                case FortressZoneState.MissionFail:
                    NotifyZoneClear(0);
                    Trigger(FortressZoneState.AllKick);
                    break;
                case FortressZoneState.AllKick:
                    _manager.KickAll();
                    Trigger(FortressZoneState.Ready);
                    break;
                case FortressZoneState.MissionClear:
                    AbleUsePortal = true;
                    NotifyZoneClear(2);
                    Trigger(FortressZoneState.BeginLootTime);
                    break;
            }
        }

        public override void Update()
        {
            var notify = new SImperialNotifyZoneTime
            {
                ZoneIndex = Zone,
                DayOfWeek = (byte)_manager._eventDay,
            };
            notify.RemainTime = (uint)TimeLeft.TotalMilliseconds;
            switch (CurrentState)
            {
                case FortressZoneState.BeginTimeAttack:
                    if((int)TimeLeft.TotalSeconds == 60)
                    {
                        _ = _manager._map.SendAsync(new SNotice(NoticeType.Gold, "Portal will be closed"));
                    }else if(_lastNotify != (int)TimeLeft.TotalMinutes)
                    {
                        _lastNotify = (int)TimeLeft.TotalMinutes;
                        _ = _manager._map.SendAsync(new SNotice(NoticeType.Gold, $"Portal will be closed after {_lastNotify} minutes"));
                    }
                    if(_manager._activeMobs <= 0)
                    {
                        Trigger(IsLastZone? FortressZoneState.MissionClear:FortressZoneState.BeginLootTime);
                    }
                    notify.MsgType = 2;
                    notify.RemainMonster = (uint)_manager._activeMobs;
                    _manager.RunTraps();
                    break;
                case FortressZoneState.BeginLootTime:
                    notify.MsgType = 0;
                    if (Zone == 3)
                        break;
                    break;
                case FortressZoneState.BeginWaitPlayer:
                    notify.MsgType = 1;
                    if (_lastNotify != (int)TimeLeft.TotalMinutes)
                    {
                        _lastNotify = (int)TimeLeft.TotalMinutes;
                        _ = _manager._map.SendAsync(new SNotice(NoticeType.Gold, $"Waiting for players {_lastNotify} minutes"));
                    }                        
                    break;
                default:
                    notify.MsgType = 3;
                    break;
            }
            if(notify.MsgType != 3)
                _ = _players.Select(x => x.Session).SendAsync(notify);

            base.Update();
        }

        internal void AddPlayer(Player plr)
        {
            _players.Add(plr);
        }
        internal Player RemPlayer(Player plr)
        {
            _players.Remove(plr);
            if(_players.Count()==0)
            {
                Trigger(FortressZoneState.AllWarpNextZone);
            }
            return plr;
        }

        internal void Clear()
        {
            AbleUsePortal = false;
            _players.Clear();
            Trigger(FortressZoneState.Ready);
        }
    }

    public class ImperialGuardian : Event
    {
        enum FortressSubState
        {
            None,
            StandBy1,
            Phaze1,
            Standby2,
            Phaze2,
            StandBy3,
            Phaze3,
            StandBy4,
            Phaze4,
            End,
        }

        private List<int> _imperialGates = new List<int>
        {
            322,
            307,
            312,
            317,
            307,
            312,
            317,
        };
        private List<int> _imperialExpReward = new List<int>
        {
            5000000,
            500000,
            600000,
            700000,
            800000,
            900000,
            1000000,
        };
        private List<FortressGate> _DestlerGaliaGates = new List<FortressGate>
        {
            new FortressGate{ Class = 525, X = 234, Y = 29, Dir = 1, map = Maps.ImperialGuardian1},
            new FortressGate{ Class = 524, X = 233, Y = 55, Dir = 1, map = Maps.ImperialGuardian1},
            new FortressGate{ Class = 525, X = 216, Y = 80, Dir = 1, map = Maps.ImperialGuardian1},
            new FortressGate{ Class = 525, X = 194, Y = 25, Dir = 3, map = Maps.ImperialGuardian1},
            new FortressGate{ Class = 524, X = 166, Y = 25, Dir = 3, map = Maps.ImperialGuardian1},
            new FortressGate{ Class = 525, X = 154, Y = 53, Dir = 1, map = Maps.ImperialGuardian1},
            new FortressGate{ Class = 525, X = 180, Y = 79, Dir = 1, map = Maps.ImperialGuardian1}
        };
        private List<FortressGate> _VermontErkanneGates = new List<FortressGate>
        {
            new FortressGate{ Class = 525, X = 75, Y = 67, Dir = 3, map = Maps.ImperialGuardian2},
            new FortressGate{ Class = 524, X = 50, Y = 65, Dir = 3, map = Maps.ImperialGuardian2},
            new FortressGate{ Class = 527, X = 19, Y = 65, Dir = 3, map = Maps.ImperialGuardian2},
            new FortressGate{ Class = 525, X = 37, Y = 93, Dir = 1, map = Maps.ImperialGuardian2},
            new FortressGate{ Class = 524, X = 41, Y = 117, Dir = 1, map = Maps.ImperialGuardian2},
            new FortressGate{ Class = 527, X = 55, Y = 154, Dir = 1, map = Maps.ImperialGuardian2},
            new FortressGate{ Class = 525, X = 107, Y = 112, Dir = 3, map = Maps.ImperialGuardian2}
        };
        private List<FortressGate> _KatoRaymondGates = new List<FortressGate>
        {
            new FortressGate{ Class = 525, X = 146, Y = 191, Dir = 3, map = Maps.ImperialGuardian3},
            new FortressGate{ Class = 527, X = 119, Y = 192, Dir = 3, map = Maps.ImperialGuardian3},
            new FortressGate{ Class = 525, X = 89, Y = 195, Dir = 3, map = Maps.ImperialGuardian3},
            new FortressGate{ Class = 528, X = 222, Y = 134, Dir = 1, map = Maps.ImperialGuardian3},
            new FortressGate{ Class = 524, X = 222, Y = 160, Dir = 1, map = Maps.ImperialGuardian3},
            new FortressGate{ Class = 527, X = 223, Y = 193, Dir = 1, map = Maps.ImperialGuardian3},
            new FortressGate{ Class = 525, X = 167, Y = 217, Dir = 1, map = Maps.ImperialGuardian3}
        };
        private List<FortressGate> _JerintGaionGates = new List<FortressGate>
        {
            new FortressGate{ Class = 528, X = 81, Y = 68, Dir = 3, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 527, X = 50, Y = 69, Dir = 3, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 528, X = 32, Y = 90, Dir = 1, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 528, X = 34, Y = 176, Dir = 1, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 527, X = 52, Y = 191, Dir = 3, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 528, X = 69, Y = 166, Dir = 1, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 528, X = 156, Y = 132, Dir = 3, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 527, X = 197, Y = 132, Dir = 3, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 528, X = 225, Y = 159, Dir = 1, map = Maps.ImperialGuardian4},
            new FortressGate{ Class = 528, X = 214, Y = 21, Dir = 3, map = Maps.ImperialGuardian4}
        };
        private List<FortressGate> _fortressGates;

        private List<Rectangle> __JerintGaionAtt = new List<Rectangle>
        {
            new Rectangle(77, 67, 5, 2),
            new Rectangle(46, 68, 5, 2),
            new Rectangle(31, 89, 2, 5),
            new Rectangle(33, 175, 2, 5),
            new Rectangle(51, 190, 5, 2),
            new Rectangle(68, 162, 2, 5),
            new Rectangle(155, 131, 5, 2),
            new Rectangle(195, 131, 6, 2),
            new Rectangle(224, 158, 2, 5),
            new Rectangle(210, 23, 5, 2),
        };
        private List<Rectangle> _fortressAtt;

        private List<Monster> _traps;

        private List<ImperialZone> _zones;

        internal int Zone { get; set; }
        internal ImperialZone CurrentZone => _zones[Zone];
        internal ImperialZone NextZone => CurrentZone.IsLastZone == false ? _zones[Zone + 1] : null;

        internal DayOfWeek _eventDay;
        private int _baseGroup;
        private DateTime _subStateEnd;
        internal MapInfo _map;
        internal int _activeMobs;

        private SImperialEnterResult _eventState = new SImperialEnterResult();

        public Maps Map;

        public ImperialGuardian()
            : base(TimeSpan.Zero, TimeSpan.FromDays(100), TimeSpan.FromMinutes(10))
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ImperialGuardian));
            _zones = new List<ImperialZone>();

            for(var i = 0; i < 4; i++)
            {
                _zones.Add(new ImperialZone(this, (ushort)i));
            }
        }
        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Open);

            _fortressGates = new List<FortressGate>();
            _fortressGates.AddRange(_DestlerGaliaGates);
            _fortressGates.AddRange(_VermontErkanneGates);
            _fortressGates.AddRange(_KatoRaymondGates);
            _fortressGates.AddRange(_JerintGaionGates);
            foreach (var g in _fortressGates)
            {
                g.monster = new Monster(g.Class, ObjectType.NPC, g.map, new System.Drawing.Point(g.X, g.Y), g.Dir) { Index = MonstersMng.Instance.GetNewIndex() };
                MonstersMng.Instance.Monsters.Add(g.monster);
                g.monster.Die += OnMonsterDead;
                g.monster.Regen += Monster_Regen;
            }

            _zones.ForEach(x => x.Initialize());
        }

        private void Monster_Regen(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            mob.Active = false;
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        private async void SetGateState(Monster mob, bool enable)
        {
            if (_map == null)
                return;

            Rectangle rect = new Rectangle(mob.Position.X - 3, mob.Position.Y - 2, 3, 4);
            switch(mob.Direction)
            {
                case 1:
                    rect = new Rectangle(mob.Position.X - 2, mob.Position.Y, 4, 3);
                    break;
                case 5:
                    rect = new Rectangle(mob.Position.X - 2, mob.Position.Y - 3, 4, 3);
                    break;
                case 3:
                    rect = new Rectangle(mob.Position.X - 3, mob.Position.Y - 2, 3, 4);
                    break;
                case 7:
                    rect = new Rectangle(mob.Position.X, mob.Position.Y - 2, 3, 4);
                    break;
            }
            if(enable)
                await _map.AddAttribute(MapAttributes.Unknow, rect);
            else
                await _map.RemoveAttribute(MapAttributes.Unknow, rect);
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;

            _logger.Information("Monster {0} dead, left {1} mobs", mob.Info.Name, _activeMobs);
            mob.Active = false;

            var result = _fortressGates.FindIndex(x => x.monster == mob)+1;
            if(result > 0)
            {
                SetGateState(mob, false);

                _ = _map.RemoveAttribute(MapAttributes.Unknow, _fortressAtt[result-1]);

                if(result < _fortressGates.Count() && result < (Zone+1)*3)
                    _fortressGates[result].monster.Type = ObjectType.Gate;

                return;
            }

            _activeMobs--;
            if(_activeMobs == 0)
            {
                _fortressGates[1].monster.Type = ObjectType.Gate;
            }
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var info = _players.First(x => x.Player == (sender as Character).Player);
            OnPlayerLeave(sender, eventArgs);
            _ = info.Player.Character.WarpTo(267);
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            var info = _players.First(x => x.Player == (sender as Character).Player);
            info.Eventer = false;
            info.Player.Character.CharacterDie -= OnPlayerDead;
            info.Player.Character.MapChanged -= OnPlayerLeave;
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Open:
                    foreach (var g in _fortressGates)
                    {
                        g.monster.Active = false;
                        g.monster.Type = ObjectType.NPC;
                    }

                    _map?.Players.ForEach(x => _ = x.WarpTo(267));
                    _zones.ForEach(x => x.Clear());
                    Zone = 0;
                    break;
                case EventState.Playing:
                    _subStateEnd = DateTime.Now;
                    _eventDay = DateTime.Now.DayOfWeek;
                    _eventDay = _eventDay == DayOfWeek.Sunday ? (DayOfWeek)7 : _eventDay;
                    _baseGroup = (int)_eventDay * 3 + 21;
                    _eventState.Day = (byte)_eventDay;
                    _eventState.Unk = 3;
                    _eventState.State = 1;

                    switch (DateTime.Now.DayOfWeek)
                    {
                        case DayOfWeek.Monday://Destler
                        case DayOfWeek.Thursday://Galia
                            _fortressGates = _DestlerGaliaGates;
                            _fortressAtt = __JerintGaionAtt;
                            Map = Maps.ImperialGuardian1;
                            _baseGroup = 24;
                            break;
                        case DayOfWeek.Tuesday://Vermont
                        case DayOfWeek.Friday://Erkanne
                            _fortressGates = _VermontErkanneGates;
                            _fortressAtt = __JerintGaionAtt;
                            Map = Maps.ImperialGuardian2;
                            _baseGroup = 27;
                            break;
                        case DayOfWeek.Wednesday://Kato
                        case DayOfWeek.Saturday://Raymond
                            _fortressGates = _KatoRaymondGates;
                            _fortressAtt = __JerintGaionAtt;
                            Map = Maps.ImperialGuardian3;
                            _baseGroup = 30;
                            break;
                        case DayOfWeek.Sunday://Jerint y Gaion Kharein
                            _fortressGates = _JerintGaionGates;
                            _fortressAtt = __JerintGaionAtt;
                            Map = Maps.ImperialGuardian4;
                            _baseGroup = 36;
                            break;
                    }
                    _map = ResourceCache.Instance.GetMaps()[Map];
                    _traps = _map.Monsters.Where(x => x.Info.Monster == 523).ToList();
                    break;
            }
        }

        public override bool TryAdd(Player plr)
        {
            var result = new SImperialEnterResult
            {
                Day = (byte)DateTime.Now.DayOfWeek,
                Index = plr.ID,
                Result = 0,
                State = 1,
                Unk = 0,
                EntryTime = 0,
            };

            /*if(plr.Character.Party == null)
            {
                result.Result = 6;
                _ = plr.Session.SendAsync(result);
                return false;
            }*/

            var itNum = ItemNumber.FromTypeIndex(14, (ushort)(result.Day == (byte)DayOfWeek.Sunday ? 109 : 102));
            var invitation = plr.Character.Inventory.FindAll(itNum);
            if(invitation.Length <= 0)
            {
                result.Result = 2;
                _ = plr.Session.SendAsync(result);
                return false;
            }

            if (
                CurrentState != EventState.Open && 
                (_players.First().Player.Character.Party != plr.Character.Party ||
                CurrentZone.CurrentState != FortressZoneState.BeginWaitPlayer)
                )
            {
                result.Result = 1;
                _ = plr.Session.SendAsync(result);
                return false;
            }

            _ = plr.Character.Inventory.Delete(invitation.First());
            plr.Window = null;
            _ = plr.Character.WarpTo(_imperialGates[(int)result.Day]);

            _players.Add(new PlayerEventInfo { Eventer = true, Player = plr });
            plr.Character.MapChanged += OnPlayerLeave;
            plr.Character.CharacterDie += OnPlayerDead;

            if(CurrentState == EventState.Open)
            {
                _zones[0].Trigger(FortressZoneState.BeginWaitPlayer);
                _zones[0].AddPlayer(plr);
            }
            else
            {
                CurrentZone.AddPlayer(plr);
            }

            return true;
        }

        public void RunTraps()
        {
            foreach (var t in _traps)
                foreach (var p in _players.Where(x => x.Eventer))
                {
                    if (t.Position.Substract(p.Player.Character.Position).LengthSquared() <= 5)
                    {
                        DamageType dmg;
                        Spell spell;

                        var att = t.MonsterAttack(out dmg, out spell);
                        _ = p.Player.Character.GetAttacked(t.Index, t.Direction, 120, att, dmg, spell, 0);
                    }
                }
        }

        internal void StandBy(ushort Zone, bool enable)
        {
            var newType = enable ? ObjectType.NPC : ObjectType.Gate;
            _fortressGates[Zone * 3].monster.Type = newType;
            /*if (!_zones[Zone].IsLastZone)
            {
                _fortressGates[Zone * 3 + 1].monster.Type = newType;
                _fortressGates[Zone * 3 + 2].monster.Type = newType;
            }*/
        }

        internal void LoadMonsters(ushort Zone)
        {
            _activeMobs = MonsterIA.InitGroup(_baseGroup+ Zone, OnMonsterDead);
            StandBy(Zone, false);
        }

        internal void UnloadMonsters(ushort Zone)
        {
            if (Zone == 0xffff)
            {
                foreach (var g in _fortressGates)
                {
                    g.monster.Active = true;
                    SetGateState(g.monster, true);
                }
            }
            else
            {
                MonsterIA.DelGroup(_baseGroup + Zone);
            }
        }

        internal void UpdateState(Player plr)
        {
            _eventState.State = (byte)(CurrentZone.Zone + 1);
            _eventState.EntryTime = (ushort)CurrentZone.TimeLeft.TotalSeconds;
            _eventState.Index = plr.ID;
            _ = _map.SendAsync(_eventState);
        }

        public override void Update()
        {
            switch (CurrentState)
            {
                case EventState.Open:
                    if (_players.Where(x => x.Eventer).Count() <= 0)
                        break;
                    Trigger(EventState.Playing);                    
                    break;
                case EventState.Playing:
                    if (_players.Where(x => x.Eventer).Count() == 0)
                    {
                        Trigger(EventState.Open);
                        break;
                    }
                    break;
            }
            _zones.ForEach(x => x.Update());
            base.Update();
        }

        internal async Task UsePortal(Character @char, ushort moveNumber)
        {
            if(!CurrentZone.AbleUsePortal)
            {
                return;
            }

            CurrentZone.RemPlayer(@char.Player);
            NextZone?.AddPlayer(@char.Player);
            UpdateState(@char.Player);

            await @char.WarpTo(moveNumber);
        }

        internal void KickAll()
        {
            _players.ForEach(x => x.Player.Character.WarpTo(22).Wait());
            _players.Clear();
        }
    }
}
