using MU.Network.Game;
using MU.Resources;
using MU.Resources.XML;
using MuEmu.Monsters;
using MuEmu.Resources.Map;
using MuEmu.Util;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.Raklion
{
    public class BattleOfSelupan : Event
    {
        private bool firstSkill;
        private Monster _selupan;
        private List<Monster> _spiderEggs;
        private List<Monster> _summonMonster;
        private MapInfo _map;

        private DateTime _nextSkill;
        private List<List<MonsterSpell>> _selupanPatters = new List<List<MonsterSpell>>();

        public BattleOfSelupan()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(BattleOfSelupan));

            _map = Resources.ResourceCache.Instance.GetMaps()[Maps.Selupan];
            _map.PlayerJoins += BattleOfSelupan_PlayerJoins;
            _map.PlayerLeaves += BattleOfSelupan_PlayerLeaves;

            var xml = Resources.ResourceLoader.XmlLoader<PatternsDto>(  Program.XMLConfiguration.Files.SelupanPatterns);
            foreach(var x in xml.Pattern.OrderBy(x => x.Number))
            {
                _selupanPatters.Add(x.Skill.ToList());
            }
        }

        private void BattleOfSelupan_PlayerLeaves(object sender, EventArgs e)
        {
            var plr = sender as Player;
            var info = _players.Find(x => x.Player == plr);
            if (info != null)
            {
                info.Eventer = false;
            }
        }

        private void BattleOfSelupan_PlayerJoins(object sender, EventArgs e)
        {
            var plr = sender as Player;

            var info = _players.Find(x => x.Player == plr);
            if(info != null)
            {
                info.Eventer = true;
            }
            else
            {
                _players.Add(new PlayerEventInfo { Player = plr, Eventer = true, Score = 0 });
            }
            if(_players.Count(x => x.Eventer == true) == 1)
                Trigger(EventState.Playing, TimeSpan.FromSeconds(60));
        }

        public override void Initialize()
        {
            base.Initialize();
            Trigger(EventState.Closed);
        }

        private void LinkMonsters()
        {
            if (_selupan != null)
                return;

            _selupan = _map.Monsters.Find(x => x.Info.Monster == 459);
            _selupan.Die += _selupan_Die;
            _selupan.Active = false;
            _selupan.Type = ObjectType.NPC;

            _summonMonster = _map.Monsters.FindAll(x => (x.Info.Monster == 457 || x.Info.Monster == 458));
            _spiderEggs = _map.Monsters.FindAll(x => x.Info.Monster >= 460 && x.Info.Monster <= 462);
            foreach (var mon in _summonMonster)
            {
                mon.Active = false;
                mon.Die += OnMonsterDead;
            }
            foreach (var mon in _spiderEggs)
            {
                mon.Active = false;
                mon.Die += spiderEggs_Die;
            }
        }

        private void spiderEggs_Die(object sender, EventArgs e)
        {
            var mon = (Monster)sender;
            mon.Active = false;
            if (_spiderEggs.Count(x => x.Active) == 0)
            {
                _selupan.Type = ObjectType.Monster;
                _selupan.Active = true;
            }
        }

        private void _selupan_Die(object sender, EventArgs e)
        {
            _selupan.Active = false;
            Trigger(EventState.Closed);
        }

        public override void NPCTalk(Player plr)
        {
            throw new NotImplementedException();
        }

        public override void OnMonsterDead(object sender, EventArgs eventArgs)
        {
            var mon = sender as Monster;
            mon.Active = false;
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
                    LinkMonsters();
                    _summonMonster.ForEach(x => x.Active = false);
                    _spiderEggs.ForEach(x => x.Active = false);
                    _selupan.Active = false;
                    Trigger(EventState.Open, TimeSpan.FromSeconds(60));
                    break;
                case EventState.Open:
                    break;
                case EventState.Playing:
                    firstSkill = false;
                    _spiderEggs.ForEach(x => x.Active = true);
                    break;
            }
        }

        public override bool TryAdd(Player plr)
        {
            throw new NotImplementedException();
        }

        private void UpdateSelupan()
        {
            if (!_selupan.Active || _nextSkill > DateTime.Now)
                return;

            var currPatter = (int)((1.0f - _selupan.Life / _selupan.MaxLife) * _selupanPatters.Count);
            var patter = _selupanPatters[currPatter];
            var randomSkill = Program.RandomProvider(patter.Count);
            var skill = patter[randomSkill];

            if (!firstSkill)
            {
                skill = MonsterSpell.Fall;
                firstSkill = true;
            }

            _=_selupan.ViewPort.Select(x => x.Session).SendAsync(new SMonsterSkillS9Eng
                {
                MonsterSkillNumber = (ushort)skill,
                ObjIndex = _selupan.Index,
                TargetObjIndex = _selupan.Target?.ID??0xffff
            });

            switch(skill)
            {
                case MonsterSpell.Recall:
                    _summonMonster.ForEach(x => x.Active = true);
                    break;
                case MonsterSpell.Invincible:
                    _spiderEggs.ForEach(x => x.Active = true);
                    _selupan.Type = ObjectType.NPC;
                    break;
                case MonsterSpell.PoisonBall:
                    _selupan.Spells.AttackSend(Spell.SelupanPoison, _selupan.Target?.ID??0xffff, true);
                    _selupan.Target?.Character.Spells.SetBuff(SkillStates.Poison, TimeSpan.FromSeconds(10), _selupan);
                    break;
                case MonsterSpell.FrostStorm:
                    _selupan.Spells.AttackSend(Spell.SelupanIceStorm, _selupan.Target?.ID ?? 0xffff, true);
                    _selupan.Target?.Character.Spells.SetBuff(SkillStates.Ice, TimeSpan.FromSeconds(10), _selupan);
                    break;
                case MonsterSpell.FrostShock:
                    _selupan.Spells.AttackSend(Spell.SelupanIceStrike, _selupan.Target?.ID ?? 0xffff, true);
                    _selupan.Target?.Character.Spells.SetBuff(SkillStates.Ice, TimeSpan.FromSeconds(10), _selupan);
                    break;
                case MonsterSpell.Fall:
                    _selupan.Spells.AttackSend(Spell.SelupanFirstSkill, _selupan.Target?.ID ?? 0xffff, true);
                    break;
                case MonsterSpell.Healing:
                    _selupan.Life += (_selupan.MaxLife-_selupan.Life)*0.1f;
                    break;
                case MonsterSpell.Teleportation:

                    _selupan.Warp(Maps.Selupan,
                        (byte)(147 + Program.RandomProvider(6, -6)),
                        (byte)(29 + Program.RandomProvider(6, -6))
                        );
                    break;
            }

            var maxDelay = (int)MathF.Max(10000.0f * _selupan.Life / _selupan.MaxLife, 3000.0f);
            var delay = Program.RandomProvider(maxDelay, 300);
            _nextSkill = DateTime.Now.AddMilliseconds(delay);

            _logger.Debug("Selupan use {0},{2},{3}, next attack in {1}ms", skill, delay, _selupan.Position.X, _selupan.Position.Y);
        }

        public override void Update()
        {
            base.Update();
            switch(CurrentState)
            {
                case EventState.Open:
                    break;
                case EventState.Playing:
                    UpdateSelupan();
                    if (!_players.Any(x => x.Eventer))
                        Trigger(EventState.Closed);
                    break;
            }
        }
    }
}
