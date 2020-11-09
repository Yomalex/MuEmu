using Serilog;
using Serilog.Core;
using MuEmu.Network.Event;
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
            500000,
            600000,
            700000,
            800000,
            900000,
            1000000,
            5000000,
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

        private FortressSubState _subState;
        private DayOfWeek _eventDay;
        private int _baseGroup;
        private DateTime _subStateEnd;
        private MapInfo _map;
        private int _lastNotify;
        private int _activeMobs;

        private SImperialEnterResult _eventState = new SImperialEnterResult();

        public Maps Map;

        public ImperialGuardian()
            : base(TimeSpan.Zero, TimeSpan.FromDays(100), TimeSpan.FromMinutes(10))
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ImperialGuardian));
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

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;

            for (var i = 0; i < _fortressGates.Count; i++)
            {
                if (_fortressGates[i].monster == mob)
                {
                    _ = _map.AddAttribute(MapAttributes.Unknow, _fortressAtt[i]);

                    if (i != 0)
                        _fortressGates[i + 1].monster.Type = ObjectType.Gate;

                    return;
                }
            }


            mob.Active = false;
            _activeMobs--;
            _logger.Information("Monster {0} dead, left {1} mobs", mob.Info.Name, _activeMobs);
            if(_activeMobs == 0)
            {
                _fortressGates[1].monster.Type = ObjectType.Gate;
            }
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var info = _players.First(x => x.Player == (sender as Character).Player);
            info.Eventer = false;
            info.Player.Character.CharacterDie -= OnPlayerDead;
            info.Player.Character.MapChanged -= OnPlayerLeave;
            _ = info.Player.Character.WarpTo(267);
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            OnPlayerDead(sender, eventArgs);
        }

        public override void OnTransition(EventState NextState)
        {
            _logger.Information("State {0}->{1}", CurrentState, NextState);
            switch(NextState)
            {
                case EventState.Open:
                    _subState = FortressSubState.None;
                    foreach (var g in _fortressGates)
                        g.monster.Active = false;

                    _map?.Players.ForEach(x => _ = x.WarpTo(267));

                    break;
                case EventState.Playing:
                    _subStateEnd = DateTime.Now;
                    _eventDay = DateTime.Now.DayOfWeek;
                    _eventDay = _eventDay == DayOfWeek.Sunday ? (DayOfWeek)7 : _eventDay;
                    _baseGroup = (int)_eventDay * 3 + 21;
                    _eventState.Day = _eventDay;
                    _eventState.Unk = 3;
                    _eventState.State = 1;

                    switch (DateTime.Now.DayOfWeek)
                    {
                        case DayOfWeek.Monday://Destler
                        case DayOfWeek.Thursday://Galia
                            _fortressGates = _DestlerGaliaGates;
                            //_fortressAtt = __JerintGaionAtt;
                            Map = Maps.ImperialGuardian1;
                            _baseGroup = 24;
                            break;
                        case DayOfWeek.Tuesday://Vermont
                        case DayOfWeek.Friday://Erkanne
                            _fortressGates = _VermontErkanneGates;
                            //_fortressAtt = __JerintGaionAtt;
                            Map = Maps.ImperialGuardian2;
                            _baseGroup = 27;
                            break;
                        case DayOfWeek.Wednesday://Kato
                        case DayOfWeek.Saturday://Raymond
                            _fortressGates = _KatoRaymondGates;
                            //_fortressAtt = __JerintGaionAtt;
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
                Day = DateTime.Now.DayOfWeek,
                Index = plr.ID,
                Result = 0,
                State = 1,
                Unk = 0,
                EntryTime = 0,
            };

            if(plr.Character.Party == null)
            {
                result.Result = 6;
                _ = plr.Session.SendAsync(result);
                return false;
            }

            var itNum = ItemNumber.FromTypeIndex(14, (ushort)(result.Day == DayOfWeek.Sunday ? 109 : 102));
            var invitation = plr.Character.Inventory.FindAll(itNum);
            if(invitation.Length <= 0)
            {
                result.Result = 2;
                _ = plr.Session.SendAsync(result);
                return false;
            }

            if (CurrentState != EventState.Open && _players.First().Player.Character.Party != plr.Character.Party)
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

            return true;
        }

        public override void Update()
        {
            base.Update();
            switch(CurrentState)
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
                        return;
                    }
                    switch(_subState)
                    {
                        case FortressSubState.Phaze1:
                        case FortressSubState.Phaze2:
                        case FortressSubState.Phaze3:
                        case FortressSubState.Phaze4:
                            var ts = _subStateEnd - DateTime.Now;
                            if (_lastNotify != (int)ts.TotalMinutes)
                            {
                                _lastNotify = (int)ts.TotalMinutes;
                                _ = Program.MapAnoucement((Maps)_map.Map, $"Portal will be closed after {_lastNotify} minutes");
                                _ = _map.SendAsync(_eventState);
                            }

                            foreach(var t in _traps)
                                foreach(var p in _players.Where(x => x.Eventer))
                                {
                                    if(t.Position.Substract(p.Player.Character.Position).LengthSquared() <= 5)
                                    {
                                        DamageType dmg;
                                        Spell spell;

                                        var att = t.MonsterAttack(out dmg, out spell);
                                        _ = p.Player.Character.GetAttacked(t.Index, t.Direction, 120, att, dmg, spell);
                                    }
                                }

                            break;
                    }

                    if (_subStateEnd <= DateTime.Now)
                    {
                        if(_subState == FortressSubState.End)
                        {
                            _subState = FortressSubState.None;
                            Trigger(EventState.Open);
                            return;
                        }
                        _logger.Information("Fortress state {0}->{1}", _subState++, _subState);
                        switch (_subState)
                        {
                            case FortressSubState.None:
                                _subStateEnd = DateTime.Now;
                                break;
                            case FortressSubState.StandBy1:
                                _subStateEnd = DateTime.Now.AddMinutes(1);
                                foreach (var g in _fortressGates)
                                {
                                    g.monster.Active = true;
                                }
                                _eventState.State = 1;
                                break;
                            case FortressSubState.Phaze1:
                                _subStateEnd = DateTime.Now.AddMinutes(10);
                                _fortressGates[0].monster.Type = ObjectType.Gate;
                                _activeMobs = MonsterIA.Group(_baseGroup, true, OnMonsterDead);
                                break;
                            case FortressSubState.Standby2:
                                _subStateEnd = DateTime.Now.AddMinutes(3);
                                MonsterIA.Group(_baseGroup, false, OnMonsterDead);
                                _eventState.State = 2;
                                break;
                            case FortressSubState.Phaze2:
                                _subStateEnd = DateTime.Now.AddMinutes(10);
                                _fortressGates[3].monster.Type = ObjectType.Gate;
                                _activeMobs = MonsterIA.Group(_baseGroup + 1, true, OnMonsterDead);
                                break;
                            case FortressSubState.StandBy3:
                                _subStateEnd = DateTime.Now.AddMinutes(3);
                                MonsterIA.Group(_baseGroup + 1, false, OnMonsterDead);
                                _eventState.State = 3;
                                break;
                            case FortressSubState.Phaze3:
                                _subStateEnd = DateTime.Now.AddMinutes(10);
                                _fortressGates[6].monster.Type = ObjectType.Gate;
                                _activeMobs = MonsterIA.Group(_baseGroup + 2, true, OnMonsterDead);
                                break;
                            case FortressSubState.StandBy4:
                                if (_eventDay != (DayOfWeek)7)
                                    _subState = FortressSubState.End;

                                _subStateEnd = DateTime.Now.AddMinutes(3);
                                MonsterIA.Group(_baseGroup + 2, false, OnMonsterDead);
                                _eventState.State = 4;
                                break;
                            case FortressSubState.Phaze4:
                                _subStateEnd = DateTime.Now.AddMinutes(10);
                                _fortressGates[9].monster.Type = ObjectType.Gate;
                                _activeMobs = MonsterIA.Group(_baseGroup + 3, true, OnMonsterDead);
                                break;
                            case FortressSubState.End:
                                _subStateEnd = DateTime.Now.AddMinutes(3);
                                MonsterIA.Group(_baseGroup + 2, false, OnMonsterDead);
                                MonsterIA.Group(_baseGroup + 3, false, OnMonsterDead);
                                break;

                        }
                        _ = _map.SendAsync(_eventState);
                    }
                    break;
            }
        }

        internal async Task UsePortal(Character @char, ushort moveNumber)
        {
            if(_subState != FortressSubState.Phaze4)
            {
                return;
            }

            await @char.WarpTo(moveNumber);
        }
    }
}
