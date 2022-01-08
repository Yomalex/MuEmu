using MU.Network.Event;
using MU.Resources;
using MU.Network.Game;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MuEmu.Util;
using MuEmu.Resources;
using MuEmu.Resources.XML;
using MuEmu.Monsters;

namespace MuEmu.Events.Kanturu
{
    public enum KanturuStandByState
    {
        None,
        Start,
        Notify,
        End,
    };
    public enum KanturuBattleOfMayaState
    {
        None,

        StandBy1,
        Notify1,
        Start1,
        Maya1,
        End1,
        EndCycle1,

        StandBy2,
        //Notify2,
        Start2,
        Maya2,
        End2,
        EndCycle2,

        StandBy3,
        //Notify3,
        Start3,
        Maya3,
        End3,
        EndCycle3,

        End,
        EndCycle,
    };
    public enum KanturuBattleOfNightmareState
    {
        None,
        Idle,
        Notify,
        Start,
        End,
        EndCycle,
    }
    public enum KanturuTowerOfRefinementState
    {
        None,
        Revitalization,
        Notify1,
        Close,
        Notify2,
        End,
    }

    public class KanturuStandBy : StateMachine<KanturuStandByState>
    {
        private Kanturu _manager;
        public KanturuStandBy(Kanturu mng)
        {
            _manager = mng;
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(KanturuStandBy));
        }
        public override void Initialize()
        {
            _manager._state = KanturuState.BattleStandBy;
            Trigger(KanturuStandByState.Start, TimeSpan.FromSeconds(3));
        }

        public override void OnTransition(KanturuStandByState NextState)
        {
            switch(NextState)
            {
                case KanturuStandByState.Start:
                    Trigger(KanturuStandByState.Notify, TimeSpan.FromSeconds(60));//1140
                    break;
                case KanturuStandByState.Notify:
                    Trigger(KanturuStandByState.End, TimeSpan.FromSeconds(60));
                    /*"[Refinery Tower] Can enter in 1 minute"*/
                    Program.MapAnoucement(Maps.Kantru2, ServerMessages.GetMessage(Messages.Kanturu_CoreGateOpens, (int)TimeLeft.TotalMinutes)).Wait();
                    break;
                case KanturuStandByState.End:
                    _manager.Trigger(EventState.Playing);
                    Trigger(KanturuStandByState.None);
                    break;
            }
        }
    }

    public class KanturuBattleOfMaya : StateMachine<KanturuBattleOfMayaState>
    {
        private Kanturu _manager;
        private bool _LeftHand = false;
        private bool _RightHand = false;
        private PatternManager _hand1;
        private PatternManager _hand2;

        public KanturuBattleOfMaya(Kanturu mng)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(KanturuBattleOfMaya));
            _manager = mng;
        }

        public override void Initialize()
        {
            _manager._state = KanturuState.BattleOfMaya;
            Trigger(KanturuBattleOfMayaState.StandBy1, TimeSpan.FromSeconds(3));
        }

        private void DieMonster(object sender, EventArgs args)
        {
            var mob = sender as Monster;
            if (mob.Info.Monster == 362 || mob.Info.Monster == 363)
            {
                _LeftHand |= mob.Info.Monster == 362;
                _RightHand |= mob.Info.Monster == 363;
                mob.Active = false;
            }

            mob.Map.SendAsync(new SNotice(NoticeType.Gold, mob.Info.Name + " killed by " + mob.Killer.Character.Name));

            if(CurrentState == KanturuBattleOfMayaState.Maya1 && _LeftHand)
            {
                Trigger(KanturuBattleOfMayaState.End1, TimeSpan.FromSeconds(5));
            }else if (CurrentState == KanturuBattleOfMayaState.Maya2 && _RightHand)
            {
                Trigger(KanturuBattleOfMayaState.End2, TimeSpan.FromSeconds(5));
            }else if (CurrentState == KanturuBattleOfMayaState.Maya3 && _LeftHand && _RightHand)
            {
                Trigger(KanturuBattleOfMayaState.End3, TimeSpan.FromSeconds(5));
            }
        }

        public override void OnTransition(KanturuBattleOfMayaState NextState)
        {
            _manager.SendAll(new SKanturuStateChange { 
                State = _manager._state, 
                btDetailState = (byte)NextState 
            });

            if(NextState == KanturuBattleOfMayaState.EndCycle1 ||
                NextState == KanturuBattleOfMayaState.EndCycle2 ||
                NextState == KanturuBattleOfMayaState.EndCycle3)
            {
                var str = ServerMessages.GetMessage(Messages.Kanturu_NextBattle);
                Program.MapAnoucement(Maps.Kantru1, str).Wait();
                Program.MapAnoucement(Maps.Kantru2, str).Wait();
                Program.MapAnoucement(Maps.Kantru3, str).Wait();
            }

            switch (NextState)
            {
                // Battle Stage 1
                case KanturuBattleOfMayaState.StandBy1:
                    Trigger(KanturuBattleOfMayaState.Notify1, TimeSpan.FromSeconds(120));
                    break;
                case KanturuBattleOfMayaState.Notify1:
                    if(!_manager.CanRun())
                    {
                        _manager.FailState = true;
                        Trigger(KanturuBattleOfMayaState.End);
                        return;
                    }
                    
                    Trigger(KanturuBattleOfMayaState.Start1, TimeSpan.FromSeconds(15));
                    break;
                case KanturuBattleOfMayaState.Start1:
                    _manager.LoadScene(0);
                    Trigger(KanturuBattleOfMayaState.End1, TimeSpan.FromSeconds(900));
                    _manager.SendAll(new SKanturuBattleTime { BattleTime = (int)TimeLeft.TotalMilliseconds });
                    break;
                case KanturuBattleOfMayaState.Maya1:
                    {
                        _LeftHand = false;
                        _RightHand = false;
                        _manager.LoadScene(3);
                        MonsterIA.InitGroup(20, DieMonster);
                        var h1 = MonstersMng.Instance.Monsters.FirstOrDefault(x => x.Info.Monster == 363 || x.Info.Monster == 362);
                        _hand1 = new PatternManager(h1, Program.XMLConfiguration.Files.MayaLeftHandPatterns);
                        _hand1.UseSkill += _hand_UseSkill;
                    }
                    break;
                case KanturuBattleOfMayaState.End1:
                    _hand1.UseSkill -= _hand_UseSkill;
                    _hand1 = null;
                    MonsterIA.DelGroup(20);
                    if (_manager.MonsterCount()>1 || !_LeftHand)
                    {
                        _manager.FailState = true;
                        Trigger(KanturuBattleOfMayaState.End);
                        return;
                    }
                    Trigger(KanturuBattleOfMayaState.EndCycle1, TimeSpan.FromSeconds(5));
                    break;
                case KanturuBattleOfMayaState.EndCycle1:
                    {
                        Trigger(KanturuBattleOfMayaState.StandBy2, TimeSpan.FromSeconds(3));
                    }
                    break;

                // Battle Stage 2
                case KanturuBattleOfMayaState.StandBy2:
                    Trigger(KanturuBattleOfMayaState.Start2, TimeSpan.FromSeconds(60));
                    break;
                case KanturuBattleOfMayaState.Start2:
                    _manager.LoadScene(0);
                    Trigger(KanturuBattleOfMayaState.End2, TimeSpan.FromSeconds(900));
                    _manager.SendAll(new SKanturuBattleTime { BattleTime = (int)TimeLeft.TotalMilliseconds });
                    break;
                case KanturuBattleOfMayaState.Maya2:
                    {
                        _LeftHand = false;
                        _RightHand = false;
                        MonsterIA.InitGroup(21, DieMonster);
                        _manager.LoadScene(4);
                        var h1 = MonstersMng.Instance.Monsters.FirstOrDefault(x => x.Info.Monster == 363 || x.Info.Monster == 362);
                        _hand2 = new PatternManager(h1, Program.XMLConfiguration.Files.MayaRightHandPatterns);
                        _hand2.UseSkill += _hand_UseSkill;
                    }
                    break;
                case KanturuBattleOfMayaState.End2:
                    _hand2.UseSkill -= _hand_UseSkill;
                    _hand2 = null;
                    MonsterIA.DelGroup(21);
                    if (_manager.MonsterCount() > 1 || !_RightHand)
                    {
                        _manager.FailState = true;
                        Trigger(KanturuBattleOfMayaState.End);
                        return;
                    }
                    Trigger(KanturuBattleOfMayaState.EndCycle2, TimeSpan.FromSeconds(5));
                    break;
                case KanturuBattleOfMayaState.EndCycle2:
                    Trigger(KanturuBattleOfMayaState.StandBy3, TimeSpan.FromSeconds(3));
                    break;

                // Battle Stage 3
                case KanturuBattleOfMayaState.StandBy3:
                    Trigger(KanturuBattleOfMayaState.Start3, TimeSpan.FromSeconds(60));
                    break;
                case KanturuBattleOfMayaState.Start3:
                    _manager.LoadScene(1);
                    Trigger(KanturuBattleOfMayaState.End3, TimeSpan.FromSeconds(900));
                    _manager.SendAll(new SKanturuBattleTime { BattleTime = (int)TimeLeft.TotalMilliseconds });
                    break;
                case KanturuBattleOfMayaState.Maya3:
                    {
                        _LeftHand = false;
                        _RightHand = false;
                        MonsterIA.InitGroup(22, DieMonster);
                        _manager.LoadScene(5);
                        var h1 = MonstersMng.Instance.Monsters.FirstOrDefault(x => x.Info.Monster == 362);
                        var h2 = MonstersMng.Instance.Monsters.FirstOrDefault(x => x.Info.Monster == 363);
                        _hand1 = new PatternManager(h1, Program.XMLConfiguration.Files.MayaLeftHandPatterns);
                        _hand2 = new PatternManager(h2, Program.XMLConfiguration.Files.MayaRightHandPatterns);
                        _hand1.UseSkill += _hand_UseSkill;
                        _hand2.UseSkill += _hand_UseSkill;
                    }
                    break;
                case KanturuBattleOfMayaState.End3:
                    _hand1.UseSkill -= _hand_UseSkill;
                    _hand2.UseSkill -= _hand_UseSkill;
                    _hand1 = null;
                    _hand2 = null;
                    MonsterIA.DelGroup(22);
                    if (_manager.MonsterCount() > 1 || !_LeftHand || !_RightHand)
                    {
                        _manager.FailState = true;
                        Trigger(KanturuBattleOfMayaState.End);
                        return;
                    }
                    Trigger(KanturuBattleOfMayaState.EndCycle3, TimeSpan.FromSeconds(5));
                    break;
                case KanturuBattleOfMayaState.EndCycle3:
                    Trigger(KanturuBattleOfMayaState.End, TimeSpan.FromSeconds(3));
                    break;

                case KanturuBattleOfMayaState.End:
                    Trigger(KanturuBattleOfMayaState.EndCycle, TimeSpan.FromSeconds(3));
                    if(_manager.FailState)
                    {
                        _manager.SendAll(new SNotice(NoticeType.Gold, ServerMessages.GetMessage(Messages.Kanturu_Fail)));
                    }
                    break;
                case KanturuBattleOfMayaState.EndCycle:
                    if (!_manager.FailState)
                    {
                        _manager._battleOfNightmare.Initialize();
                    }
                    else
                    {
                        _manager.Trigger(EventState.Open);
                    }
                    Trigger(KanturuBattleOfMayaState.None);
                    break;
            }
        }

        private void _maya_UseSkill(object sender, UseSkillEventArgs e)
        {
            var monster = sender as Monster;
            var msg0 = new SKanturuWideAttack
            {
                ObjClass = monster.Info.Monster,
                Type = 0,
            };
            _manager.SendAll(msg0);
        }

        private void _hand_UseSkill(object sender, UseSkillEventArgs e)
        {
            var monster = sender as Monster;
            var msg1 = new SKanturuWideAttack
            {
                ObjClass = monster.Info.Monster,
                Type = 1,
            };
            DamageType type;
            Spell spell;
            if (monster.Target != null)
            {
                var attack = monster.MonsterAttack(out type, out spell);
                monster.Target?.Character.GetAttacked(monster.Index, 1, 1, attack, type, spell, 0);
            }
            switch (e.Spell)
            {
                case MonsterSpell.Pressure:
                    //monster.Spells.AttackSend(Spell.pres, monster.Target.ID, true);
                    break;
                case MonsterSpell.PowerWave:
                    monster.Spells.AttackSend(Spell.PowerWave, monster.Target.ID, true);
                    break;
                case MonsterSpell.BrokenShower:
                    _manager.SendAll(msg1);
                    break;
                case MonsterSpell.IceStorm:
                    break;
            }

        }

        public override void Update()
        {
            _hand1?.Update();
            _hand2?.Update();
            switch(CurrentState)
            {
                case KanturuBattleOfMayaState.Maya1:
                case KanturuBattleOfMayaState.Maya2:
                case KanturuBattleOfMayaState.Maya3:
                case KanturuBattleOfMayaState.Notify1:
                case KanturuBattleOfMayaState.Start1:
                case KanturuBattleOfMayaState.Start2:
                case KanturuBattleOfMayaState.Start3:
                    _manager.SendAll(new SKanturuMonsterUserCount { MonsterCount = (byte)(_manager.MonsterCount() - 1), UserCount = _manager.Count });
                    if (!_manager.CanRun())
                    {
                        _manager.FailState = true;
                        Trigger(KanturuBattleOfMayaState.End);
                    }
                    if(
                        (CurrentState == KanturuBattleOfMayaState.Start1 ||
                        CurrentState == KanturuBattleOfMayaState.Start2 ||
                        CurrentState == KanturuBattleOfMayaState.Start3) &&
                        _manager.MonsterCount() <= 1)
                    {
                        Trigger(CurrentState+1);
                    }
                    break;
            }
            base.Update();
        }


    }

    public class KanturuBattleOfNightmare : StateMachine<KanturuBattleOfNightmareState>
    {
        private Kanturu _manager;
        private PatternManager _nightmare;
        public KanturuBattleOfNightmare(Kanturu mng)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(KanturuBattleOfNightmare));
            _manager = mng;
        }

        public override void Initialize()
        {
            _manager._state = KanturuState.BattleOfNightmare;
            Trigger(KanturuBattleOfNightmareState.Idle, TimeSpan.FromSeconds(3));
        }

        public override void OnTransition(KanturuBattleOfNightmareState NextState)
        {
            _manager.SendAll(new SKanturuStateChange
            {
                State = _manager._state,
                btDetailState = (byte)NextState
            });

            switch (NextState)
            {
                case KanturuBattleOfNightmareState.None:
                    break;
                case KanturuBattleOfNightmareState.Idle:
                    _manager.WarpAllTo(134);
                    Trigger(KanturuBattleOfNightmareState.Notify, TimeSpan.FromSeconds(5));
                    break;
                case KanturuBattleOfNightmareState.Notify:
                    Trigger(KanturuBattleOfNightmareState.Start, TimeSpan.FromSeconds(10));
                    _manager.SendAll(new SNotice(NoticeType.Gold, ServerMessages.GetMessage(Messages.Kanturu_NightmareNotify)));
                    break;
                case KanturuBattleOfNightmareState.Start:
                    {
                        _manager.LoadScene(2);
                        MonsterIA.InitGroup(23, DieMonster);
                        var mob = MonstersMng.Instance.Monsters.FirstOrDefault(x => x.Info.Monster == 361);
                        _nightmare = new PatternManager(mob, Program.XMLConfiguration.Files.NightmarePatterns);
                        _nightmare.UseSkill += _nightmare_UseSkill;
                        _manager.FailState = true;
                        Trigger(KanturuBattleOfNightmareState.End, TimeSpan.FromSeconds(1200));
                        _manager.SendAll(new SKanturuBattleTime { BattleTime = (int)TimeLeft.TotalSeconds });
                    }
                    break;
                case KanturuBattleOfNightmareState.End:
                    _nightmare = null;
                    MonsterIA.DelGroup(23);
                    _logger.Information("Battle result:{0}", _manager.FailState?"Fail":"Win");
                    _manager.SendAll(new SKanturuBattleResult { Result = (byte)(_manager.FailState ? 0 : 1) });
                    Trigger(KanturuBattleOfNightmareState.EndCycle, TimeSpan.FromSeconds(5));
                    break;
                case KanturuBattleOfNightmareState.EndCycle:
                    if(_manager.FailState)
                    {
                        _manager.Clear();
                    }else
                    {
                        _manager._towerOfRefinement.Initialize();
                        _manager.Trigger(EventState.Closed);
                    }
                    Trigger(KanturuBattleOfNightmareState.None);
                    break;
            }
        }

        private void _nightmare_UseSkill(object sender, UseSkillEventArgs e)
        {
            var monster = (Monster)sender;
            switch(e.Spell)
            {
                case MonsterSpell.DogAttack:
                    break;
                case MonsterSpell.Potlike:
                    break;
                case MonsterSpell.Summon:
                    break;
                case MonsterSpell.FireJeans:
                    break;
                case MonsterSpell.Warp:
                        monster.Warp(Maps.Kantru3,
                        (byte)(monster.Position.X + Program.RandomProvider(6, -6)),
                        (byte)(monster.Position.Y + Program.RandomProvider(6, -6))
                        );
                    break;
            }
        }

        private void DieMonster(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            mob.Active = false;
            if(mob.Info.Monster == 361)
            {
                _manager.SendAll(new SNotice(NoticeType.Gold, ServerMessages.GetMessage(Messages.Kanturu_Successfull, mob.Killer.Character.Name)));
                _manager.FailState = false;
                Trigger(KanturuBattleOfNightmareState.End);
            }
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case KanturuBattleOfNightmareState.Start:
                    break;
            }
            base.Update();
        }
    }

    public class KanturuTowerOfRefinement : StateMachine<KanturuTowerOfRefinementState>
    {
        private Kanturu _manager;

        public KanturuTowerOfRefinement(Kanturu mng)
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(KanturuTowerOfRefinement));
            _manager = mng;
        }

        public override void Initialize()
        {
            Trigger(KanturuTowerOfRefinementState.Revitalization, TimeSpan.FromSeconds(3));
        }

        public override void OnTransition(KanturuTowerOfRefinementState NextState)
        {
            _manager.SendAll(new SKanturuStateChange
            {
                State = _manager._state,
                btDetailState = (byte)NextState
            });

            switch (NextState)
            {
                case KanturuTowerOfRefinementState.None:
                    _manager.Trigger(EventState.Open);
                    break;
                case KanturuTowerOfRefinementState.Revitalization:
                    Trigger(KanturuTowerOfRefinementState.Notify1, TimeSpan.FromSeconds(82500));
                    break;
                case KanturuTowerOfRefinementState.Notify1:
                    Trigger(KanturuTowerOfRefinementState.Close, TimeSpan.FromSeconds(300));
                    break;
                case KanturuTowerOfRefinementState.Close:
                    Trigger(KanturuTowerOfRefinementState.Notify2, TimeSpan.FromSeconds(2220));
                    break;
                case KanturuTowerOfRefinementState.Notify2:
                    Trigger(KanturuTowerOfRefinementState.End, TimeSpan.FromSeconds(180));
                    break;
                case KanturuTowerOfRefinementState.End:
                    Trigger(KanturuTowerOfRefinementState.None, TimeSpan.FromSeconds(5));
                    break;
            }
        }
    }

    public class Kanturu : Event
    {
        private DateTimeOffset _lastAnouncement = DateTimeOffset.Now;
        private KanturuStagesDto _info;
        private List<Monster> _monsters = new List<Monster>();
        internal KanturuState _state;
        internal KanturuStandBy _standBy;
        internal KanturuBattleOfMaya _battleOfMaya;
        internal KanturuBattleOfNightmare _battleOfNightmare;
        internal KanturuTowerOfRefinement _towerOfRefinement;

        public bool FailState { get; set; }

        public const int MaxPlayers = 15;

        public Kanturu()
            : base(TimeSpan.FromDays(1), TimeSpan.FromMinutes(2), TimeSpan.FromHours(1))
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Kanturu));
            _standBy = new KanturuStandBy(this);
            _battleOfMaya = new KanturuBattleOfMaya(this);
            _battleOfNightmare = new KanturuBattleOfNightmare(this);
            _towerOfRefinement = new KanturuTowerOfRefinement(this);

            _info = ResourceLoader.XmlLoader<KanturuStagesDto>("./Data/Monsters/KanturuMonsterSet.xml");
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
                State = _state,
                btEnter = 0,
                btDetailState = 0,
                btUserCount = (byte)_players.Count,
            };

            switch(_state)
            {
                case KanturuState.BattleStandBy:
                    msg.btDetailState = (byte)_standBy.CurrentState;
                    msg.iRemainTime = (int)_standBy.TimeLeft.TotalSeconds;
                    break;
                case KanturuState.BattleOfMaya:
                    msg.btDetailState = (byte)_battleOfMaya.CurrentState;
                    msg.iRemainTime = (int)_battleOfMaya.TimeLeft.TotalSeconds;
                    switch(_battleOfMaya.CurrentState)
                    {
                        case KanturuBattleOfMayaState.StandBy1:
                        case KanturuBattleOfMayaState.StandBy2:
                        case KanturuBattleOfMayaState.StandBy3:
                            if (_players.Count(x => x.Eventer == true) < MaxPlayers)
                                msg.btEnter = 1;
                            break;
                    }
                    break;
                case KanturuState.BattleOfNightmare:
                    msg.btDetailState = (byte)_battleOfNightmare.CurrentState;
                    msg.iRemainTime = (int)_battleOfNightmare.TimeLeft.TotalSeconds;
                    break;
                case KanturuState.TowerOfRefinery:
                    msg.btDetailState = (byte)_towerOfRefinement.CurrentState;
                    msg.iRemainTime = (int)_towerOfRefinement.TimeLeft.TotalSeconds;
                    switch (_towerOfRefinement.CurrentState)
                    {
                        case KanturuTowerOfRefinementState.Revitalization:
                        case KanturuTowerOfRefinementState.Notify1:
                            msg.btEnter = 1;
                            break;
                    }
                    break;
            }

            plr.Session.SendAsync(msg)
                .Wait();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mob = sender as Monster;
            mob.Active = false;
        }

        public override void OnPlayerDead(object sender, EventArgs eventArgs)
        {
            OnPlayerLeave(sender, eventArgs);
        }

        public override void OnPlayerLeave(object sender, EventArgs eventArgs)
        {
            var info = (from plr in _players
                        where plr.Player == (Player)sender
                        select plr).FirstOrDefault();

            info.Eventer = false;
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.None:
                    _state = KanturuState.None;
                    break;
                case EventState.Closed:
                    _state = KanturuState.TowerOfRefinery;
                    //Trigger(EventState.Open, TimeSpan.FromHours(23));
                    break;
                case EventState.Open:
                    Clear();
                    _standBy.Initialize();
                    _state = KanturuState.BattleStandBy;
                    break;
                case EventState.Playing:
                    _battleOfMaya.Initialize();
                    _state = KanturuState.BattleOfMaya;
                    break;
            }
        }

        internal bool CanRun()
        {
            return _players.Any(x => x.Eventer) || EventState.Closed == CurrentState;
        }

        internal void Clear()
        {
            WarpAllTo(137);
            _players.Clear();
            FailState = false;
        }

        internal void WarpAllTo(int gate)
        {
            _players.ForEach(x => x.Player?.Character?.WarpTo(gate).Wait());
        }

        internal async void SendAll(object message)
        {
            await _players
                .Where(x => x.Eventer == true)
                .Select(x => x.Player.Session)
                .SendAsync(message);
        }

        internal void LoadScene(int number)
        {
            foreach (var x in _monsters)
            {
                x.Die -= OnMonsterDead;
                MonstersMng.Instance.DeleteMonster(x);
            }
            _monsters.Clear();

            var mobs = _info.Stages.First(x => x.Number == number).Monsters;
            _monsters = mobs.Select(x => new Monster(x.Type, ObjectType.Monster, (Maps)x.Map, new System.Drawing.Point(x.PosX, x.PosY), (byte)x.Dir) { Index = MonstersMng.Instance.GetNewIndex() }).ToList();
            foreach (var x in _monsters)
            {
                x.Die += OnMonsterDead;
                MonstersMng.Instance.Monsters.Add(x);
            }
        }

        internal int MonsterCount()
        {
            return _monsters.Where(x => x.Active).Count();
        }

        public override void Update()
        {
            switch(CurrentState)
            {
                case EventState.Closed:
                    break;
                case EventState.Open:
                    break;
                case EventState.Playing:
                    break;
            }
            _standBy.Update();
            _battleOfMaya.Update();
            _battleOfNightmare.Update();
            _towerOfRefinement.Update();
            base.Update();
        }

        public override bool TryAdd(Player plr)
        {
            if(
                CurrentState == EventState.Playing && 
                _state == KanturuState.BattleOfMaya && 
                (_battleOfMaya.CurrentState == KanturuBattleOfMayaState.StandBy1 || _battleOfMaya.CurrentState == KanturuBattleOfMayaState.StandBy2 || _battleOfMaya.CurrentState == KanturuBattleOfMayaState.StandBy3)
                )
            {
                plr.Character.WarpTo(133).Wait();
                plr.Character.CharacterDie += Character_CharacterDie;
                plr.Character.MapChanged += Character_MapChanged;
                var exists = _players.FirstOrDefault(x => x.Player == plr);
                if (exists == null)
                {
                    _players.Add(new PlayerEventInfo
                    {
                        Eventer = true,
                        Player = plr,
                        Score = 0,
                    });
                }
                else
                {
                    exists.Eventer = true;
                }
                return true;
            }else if(CurrentState == EventState.Closed && 
                (_towerOfRefinement.CurrentState == KanturuTowerOfRefinementState.Revitalization || _towerOfRefinement.CurrentState == KanturuTowerOfRefinementState.Notify1))
            {
                plr.Character.WarpTo(135).Wait();

                plr.Session.SendAsync(new SKanturuStateChange
                {
                    State = _state,
                    btDetailState = (byte)_towerOfRefinement.CurrentState
                }).Wait();
                return true;
            }
            return false;
        }

        private void Character_MapChanged(object sender, EventArgs e)
        {
            var @char = sender as Character;
            OnPlayerLeave(@char.Player, e);
            @char.MapChanged -= Character_MapChanged;
        }

        private void Character_CharacterDie(object sender, EventArgs e)
        {
            var @char = sender as Character;
            OnPlayerDead(@char.Player, e);
            @char.CharacterDie -= Character_CharacterDie;
        }
    }
}
