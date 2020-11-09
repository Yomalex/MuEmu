using Microsoft.EntityFrameworkCore.Internal;
using MuEmu.Monsters;
using MuEmu.Network;
using MuEmu.Network.Event;
using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using WebZen.Util;

namespace MuEmu.Events.Crywolf
{
    internal class CrywolfAltar
    {
        public enum CrywolfAltarStates
        {
            Free,
            Contracted = 1<<4,
            AttemptToContract = 2<<4,
            Broken = 3<<4,
        }

        public Monster Altar;
        public Player Elf;
        public int Left;
        public DateTime ValidContractTime;
        public Point Position;
        public bool ValidContract;
        public DateTime ActiveTime;
        public CrywolfAltarStates State => (ValidContract) ? CrywolfAltarStates.Contracted : (Elf != null) ? CrywolfAltarStates.AttemptToContract : (Left > 0) ? CrywolfAltarStates.Free : CrywolfAltarStates.Broken;
        public byte StateByte => (byte)(Left | (byte)State);
    }
    public class Crywolf : Event
    {
        public enum OccupationState
        {
            Success,
            Fail,
            Battel,
        };
        public enum CrywolfState
        {
            None,
            Notify1,
            Notify2,
            Ready,
            Start,
            End,
            EndCycle,
        };

        private DateTime _nextNotify;
        private Dictionary<int, CrywolfAltar> _altar;

        private DateTime _standBy;
        private DateTime _stage;
        private DateTime _contract;
        private DateTime _contractEnd;
        private DateTime _army;
        private DateTime _balgas;
        private DateTime _end;

        private MapInfo _mapInfo;
        private OccupationState _occupation;
        public OccupationState Occupation
        {
            get => _occupation;
            private set
            {
                if (_occupation == value)
                    return;

                _occupation = value;

                _mapInfo.SendAsync(new SCrywolfState
                {
                    Occupation = (byte)value,
                    State = (byte)_crywolfState
                }).Wait();
            }
        }
        private CrywolfState _crywolfState;
        public CrywolfState State
        {
            get => _crywolfState;
            private set
            {
                if (_crywolfState == value)
                    return;

                _logger.Information("SubState {0}->{1}", _crywolfState, value);
                _crywolfState = value;

                _mapInfo.SendAsync(new SCrywolfState
                {
                    Occupation = (byte)_occupation,
                    State = (byte)value
                }).Wait();
            }
        }

        private double _statueHPMax;
        private double _statueHP;
        private Monster _statue;
        private List<Monster> _armyList;
        private Monster _balgass;
        private List<SkillStates> _statueStates = new List<SkillStates>
        {
            0,
            SkillStates.Poison,
            SkillStates.Ice,
            SkillStates.Attack,
            SkillStates.Defense,
            SkillStates.SoulBarrier,
        };
        private Dictionary<byte, int> _scoreRank = new Dictionary<byte, int>
        {
            { 0, 1000 },
            { 1, 3000 },
            { 2, 5000 },
            { 3, 10000 },
            { 4, int.MaxValue },
        };
        private List<ulong> _bonusExp = new List<ulong>
        {
            0,
            100000,
            400000,
            600000,
            900000,
        };
        private List<int> _monsterGroups = new List<int> { 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13 };
        private List<int> _bossGroups = new List<int> { 5 };

        public Crywolf():base(TimeSpan.FromDays(6), TimeSpan.FromDays(1), TimeSpan.FromDays(1))
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Crywolf));
            _mapInfo = ResourceCache.Instance.GetMaps()[Maps.Crywolf];
            _mapInfo.PlayerLeaves += OnPlayerLeave;
            _mapInfo.PlayerJoins += _mapInfo_PlayerJoins;
            _mapInfo.MonsterAdd += _mapInfo_MonsterAdd;
            _altar = new Dictionary<int, CrywolfAltar>
            {
                { 205, new CrywolfAltar{ Elf = null, Left = 2 } },
                { 206, new CrywolfAltar{ Elf = null, Left = 2 } },
                { 207, new CrywolfAltar{ Elf = null, Left = 2 } },
                { 208, new CrywolfAltar{ Elf = null, Left = 2 } },
                { 209, new CrywolfAltar{ Elf = null, Left = 2 } },
            };

            _armyList = new List<Monster>();
        }

        private void _mapInfo_MonsterAdd(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            if(mob.Info.Monster == 204)
            {
                _statue = mob;
            }else
            if(mob.Info.Monster >= 205 && mob.Info.Monster <= 209)
            {
                _altar[mob.Info.Monster].Altar = mob;
            }else
            {
                switch (mob.Info.Monster)
                {
                    case 349://Balgass
                    case 340://Dark Elf
                    case 348://Ballista
                    case 341://Soram
                    case 344://Balram
                    case 345://Death Spirit
                        mob.Die += OnMonsterDead;
                        mob.Active = false;
                        _armyList.Add(mob);
                        if (mob.Info.Monster == 349)
                            _balgass = mob;
                        break;
                }
            }
        }

        internal async void SendBenefit(GSSession session)
        {
            await session.SendAsync(new SCrywolfBenefit(0));
        }

        internal async void SendState(GSSession session)
        {
            await session.SendAsync(new SCrywolfState {
                Occupation = (byte)_occupation,
                State = (byte)_crywolfState
            });
        }

        private void _mapInfo_PlayerJoins(object sender, EventArgs e)
        {
            if(CurrentState == EventState.Open)
                _players.Add(new PlayerEventInfo
                {
                    Eventer = false,
                    Player = (sender as Player),
                    Score = 0,
                });
        }

        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Closed);
        }

        public override void Update()
        {
            base.Update();
            if (State >= CrywolfState.Ready)
            {
                var hp = 0.0;
                var changed = false;
                foreach (var a in _altar.Values)
                {
                    if (a.Elf != null && !a.ValidContract && a.ValidContractTime <= DateTime.Now)
                    {
                        a.ValidContract = true;
                        a.Elf.Character.Spells.SetBuff(SkillStates.AltarValidContract, TimeSpan.FromDays(1));
                        var altar = a.Altar.Info.Monster - 204;
                        a.Elf.Session.SendAsync(new SNotice(NoticeType.Blue, $"You contracted {altar} Altar Successfuly")).Wait();
                        Program.GlobalAnoucement($"{a.Elf.Character.Name} contracted Altar Nr. {altar}").Wait();
                        a.Altar.Spells.ClearAll();
                        a.Altar.Spells.SetBuff(SkillStates.AltarValidContract, TimeSpan.FromDays(1));
                        changed = true;
                    }

                    if (a.ValidContract)
                    {
                        hp += a.Elf.Character.Health;
                    }

                    if (a.Elf != null && a.Position != a.Elf.Character.Position)
                    {
                        OnPlayerLeave(a.Elf, new EventArgs());
                    }
                }

                if (_statue != null && changed)
                {
                    _statue.Spells.ClearAll();
                    _statue.Spells.SetBuff(
                        _statueStates[_altar.Values.Count(x => x.ValidContract)],
                        TimeSpan.FromDays(1),
                        _altar.Values
                            .Select(x => x.Elf)
                            .FirstOrDefault(x => x != null)?.Character ?? null
                        );
                }
                _statueHP = hp;
            }

            switch (CurrentState)
            {
                case EventState.None:
                    break;
                case EventState.Closed:
                    {
                        State = CrywolfState.None;
                        if(_standBy <= DateTime.Now)
                        {
                            Trigger(EventState.Open);
                        }
                    }
                    break;
                case EventState.Open:
                    { 
                        switch (State)
                        {
                            case CrywolfState.Notify1:
                                if (_nextNotify <= DateTime.Now)
                                {
                                    Program.GlobalAnoucement("[Crywolf] La estatua perdio su proteccion").Wait();
                                    _nextNotify = DateTime.Now.AddSeconds(70);
                                }
                                if (_stage <= DateTime.Now)
                                    State = CrywolfState.Notify2;
                                break;
                            case CrywolfState.Notify2:
                            case CrywolfState.Ready:
                                if (_nextNotify <= DateTime.Now)
                                {
                                    var left = _army - DateTime.Now;
                                    if (Math.Floor(left.TotalMinutes) <= 1)
                                    {
                                        Program.GlobalAnoucement($"[Crywolf] El ejercito de balgass atacara en {(int)left.TotalSeconds}seg").Wait();
                                    }
                                    else
                                    {
                                        Program.GlobalAnoucement($"[Crywolf] El ejercito de balgass atacara en {(int)left.TotalMinutes}min").Wait();
                                    }
                                    _nextNotify = DateTime.Now.AddSeconds(70);
                                }
                                if (_contract <= DateTime.Now && State != CrywolfState.Ready)
                                {
                                    State = CrywolfState.Ready;
                                    _mapInfo.SendAsync(new SCrywolfStatueAndAltarInfo
                                    {
                                        AltarState = _altar.Values.Select(x => x.StateByte).ToArray(),
                                        StatueHP = (int)(_statueHPMax > 0 ? _statueHP / _statueHPMax : 0.0),
                                    }).Wait();
                                    _monsterGroups.ForEach(x => MonsterIA.Group(x, true));
                                }

                                if(State == CrywolfState.Ready)
                                {
                                    _mapInfo.SendAsync(new SCrywolfStatueAndAltarInfo
                                    {
                                        AltarState = _altar.Values.Select(x => x.StateByte).ToArray(),
                                        StatueHP = (int)(_statueHPMax > 0 ? _statueHP / _statueHPMax : 0.0),
                                    }).Wait();
                                }

                                if(_army <= DateTime.Now)
                                    Trigger(EventState.Playing);
                                break;
                        }
                    }
                    break;
                case EventState.Playing:
                    {
                        if (_nextNotify <= DateTime.Now)
                        {
                            //20s
                            _mapInfo.SendAsync(new SCrywolfLeftTime { TimeLeft = _end - DateTime.Now }).Wait();
                            //2s
                            _mapInfo.SendAsync(new SCrywolfStatueAndAltarInfo
                            {
                                AltarState = _altar.Values.Select(x => x.StateByte).ToArray(),
                                StatueHP = (int)(_statueHPMax > 0 ? _statueHP / _statueHPMax : 0.0),
                            }).Wait();
                            //5s
                            _mapInfo.SendAsync(new SCrywolfBossMonsterInfo
                            {
                                Monster = (byte)_armyList.Count(x => x.Active && x.Info.Monster == 340),
                                MonsterHP = (int)((_balgass?.Active??false) ? _balgass.Life / (_balgass.MaxLife + 1) : -1.0),
                            }).Wait();

                            if (_balgass != null && !_balgass.Active && _balgas <= DateTime.Now)
                            {
                                _bossGroups.ForEach(x => MonsterIA.Group(x, true));
                            }

                            _nextNotify = DateTime.Now.AddSeconds(2);
                        }

                        if (_altar.Values.Count(x => x.ValidContract) <= 0)
                            Close(OccupationState.Fail);

                        if (_end <= DateTime.Now)
                            Close(OccupationState.Success);
                    }
                    break;
            }
        }

        public override void NPCTalk(Player plr)
        {
            var mob = plr.Window as Monster;
            if(
                mob.Info.Monster >= 205 && mob.Info.Monster <= 209 &&
                _contract <= DateTime.Now && _contractEnd >= DateTime.Now
                )
            {
                var a = _altar[mob.Info.Monster];
                if (a.Elf == null && a.Left > 0 && a.ActiveTime <= DateTime.Now)
                {
                    a.ValidContractTime = DateTime.Now.AddSeconds(5);
                    a.Elf = plr;
                    a.Left--;
                    a.Altar.Spells.ClearAll();
                    a.Altar.Spells.SetBuff(SkillStates.AltarOfWolfCA, TimeSpan.FromDays(1));
                    a.Elf.Character.Spells.SetBuff(SkillStates.AltarOfWolfCA, TimeSpan.FromSeconds(5));
                    a.Position = a.Elf.Character.Position;

                    _statueHP += plr.Character.Health;
                    _statueHPMax += plr.Character.MaxHealth;

                    _logger.ForAccount(plr.Session).Information("[Altar Op.] Attempt to contract Altar[{0}]", mob.Info.Monster-204);
                }
            }
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
            var killer = _players.Find(x => x.Player == mob.Killer);
            if(killer == null)
            {
                killer = new PlayerEventInfo
                {
                    Player = mob.Killer
                };
                _players.Add(killer);
            }    
            switch(mob.Info.Monster)
            {
                case 349://Balgass
                    killer.Score += 7000;
                    Close(OccupationState.Success);
                    break;
                case 340://Dark Elf
                    killer.Score += 3000; 
                    break;
                case 348://Ballista
                    killer.Score += 1000;
                    break;
                case 341://Soram
                    killer.Score += 700;
                    break;
                case 344://Balram
                    killer.Score += 600;
                    break;
                case 345://Death Spirit
                    killer.Score += 600;
                    break;
            }
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            var @char = sender as Character;

            if (@char == null)
                return;

            var result = _altar.Values.FirstOrDefault(x => x.Elf == @char.Player);
            if (result == null)
                return;

            var sp = result.Altar.Spells;
            sp.ClearAll();
            if (result.Left > 0)
            {
                sp.SetBuff(SkillStates.AltarCanContract, TimeSpan.FromDays(1));
            }else
            {
                sp.SetBuff(SkillStates.AltarCantContract, TimeSpan.FromDays(1));
            }

            result.ActiveTime = DateTime.Now.AddSeconds(10);
            result.ValidContract = false;
            result.Elf.Character.Spells.ClearAll();
            result.Elf = null;

            double _StatueHP = 0;
            double _StatueHPMax = 0;
            foreach(var a in _altar.Values.Where(x => x.Elf != null))
            {
                _StatueHP += a.Elf.Character.Health;
                _StatueHPMax += a.Elf.Character.MaxHealth;
            }

            _statueHP = _StatueHP;
            _statueHPMax = _StatueHPMax;

            _logger.ForAccount(@char.Player.Session).Information("[Altar Op.] Remove contract Altar[{0}]", result.Altar.Info.Monster - 204);

            if (_statue != null)
            {
                _statue.Spells.ClearAll();
                _statue.Spells.SetBuff(_statueStates[_altar.Values.Count(x => x.ValidContract)], TimeSpan.FromDays(1));
            }
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            OnPlayerDead((sender as Player).Character, eventArgs);
        }

        public override void OnTransition(EventState NextState)
        {
            _logger.Information("State:{0}->{1}", CurrentState, NextState);

            _mapInfo.SendAsync(new SCrywolfState
            {
                Occupation = (byte)_occupation,
                State = (byte)_crywolfState,
            }).Wait();

            switch (NextState)
            {
                case EventState.None:
                    State = CrywolfState.None;
                    break;
                case EventState.Closed:
                    {
                        _monsterGroups.ForEach(x => MonsterIA.Group(x, false));
                        _bossGroups.ForEach(x => MonsterIA.Group(x, false));
                        foreach (var a in _altar.Values)
                        {
                            a.Left = 2;
                            if (a.Altar != null)
                            {
                                a.Altar.Spells.ClearAll();
                            }
                            else
                            {
                                Trigger(EventState.None);
                            }
                        }

                        State = CrywolfState.None;
                        var eventDay = DateTime.Now.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Wednesday);
                        if (eventDay < DateTime.Now)
                            eventDay = eventDay.AddDays(7);

                        _standBy = new DateTime(eventDay.Year, eventDay.Month, eventDay.Day, 09, 24, 00);
                        _stage = _standBy.AddMinutes(16);
                        _contract = _stage.AddMinutes(5);
                        _contractEnd = _contract.AddMinutes(2.5);
                        _army = _contract.AddMinutes(5);
                        _balgas = _army.AddMinutes(5);
                        _end = _balgas.AddMinutes(5);

                        _logger.Information("Start at {0}", _standBy);
                        _players.Clear();
                    }
                    break;
                case EventState.Open:
                    State = CrywolfState.Notify1;
                    foreach(var a in _altar.Values)
                    {
                        a.Altar.Spells.ClearAll();
                        a.Altar.Spells.SetBuff(SkillStates.AltarCanContract, TimeSpan.FromDays(1));
                    }
                    break;
                case EventState.Playing:
                    State = CrywolfState.Start;
                    foreach (var m in _armyList)
                    {
                        m.Active = true;
                    }
                    break;
            }
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }

        private void SendRank()
        {
            var orderList = _players
                .OrderByDescending(x => x.Score)
                .Where(x => x.Player.Status == LoginStatus.Playing && !_altar.Values.Any(y => y.Elf == x.Player))
                .ToList();

            var dto = new List<CrywolfHeroDto>();

            var count = 5;
            foreach(var plr in orderList)
            {
                var rank = _scoreRank.First(x => x.Value > plr.Score).Key;
                var exp = _bonusExp[rank];

                if (_occupation == OccupationState.Fail)
                    exp = (ulong)(exp * 0.1f);

                var @char = plr.Player.Character;
                @char.Experience += exp;
                plr.Player.Session
                    .SendAsync(new SKillPlayer(0xffff, (ushort)exp, 0))
                    .Wait();

                dto.Add(new CrywolfHeroDto
                {
                    Rank = rank,
                    Score = plr.Score,
                    Class = @char.Class,
                    btName = @char.Name.GetBytes()
                });

                if(count-- > 0 && Occupation == OccupationState.Success)
                {
                    _mapInfo.AddItem(
                        @char.Position.X,
                        @char.Position.Y,
                        new Item(ItemNumber.FromTypeIndex(14, 13)),// Jewel of Bless
                        @char
                        );
                }
            }

            orderList
                .Select(x => x.Player.Session)
                .SendAsync(new SCrywolfHeroList { Heros = dto.ToArray() })
                .Wait();

            _players.Clear();
            if(Occupation == OccupationState.Success)
            {
                foreach(var a in _altar.Values.Where(x => x.Elf!=null && x.ValidContract))
                {
                    var @char = a.Elf.Character;
                    _mapInfo.AddItem(
                        @char.Position.X,
                        @char.Position.Y,
                        new Item(ItemNumber.FromTypeIndex(14, 13)),// Jewel of Bless
                        @char
                        );                    
                }
            }
        }

        private void Close(OccupationState state)
        {
            Occupation = state;
            State = CrywolfState.End;
            SendRank();
            Trigger(EventState.Closed);
        }
    }
}
