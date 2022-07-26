using MU.Network.Event;
using MU.Resources;
using MuEmu.Monsters;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MuEmu.Events.AcheronGuardian
{
    internal class AcheronGuardian : Event
    {
        private List<Point> _obeliskPositions = new List<Point>
        {
            new Point(89,41),
            new Point(188,101),
            new Point(162,54),
        };
        private List<Maps> _obeliskMaps = new List<Maps>
        { 
            Maps.ArkaWar,
            Maps.ArkaWar,
            Maps.ArcaBattle,
        };
        private Monster _obelisk;
        private List<Monster> _cursedMonsters = new List<Monster>();
        private Dictionary<Element, List<ushort>> _cursedMonstersByElement = new Dictionary<Element, List<ushort>>
        {
            /*1*/{ Element.Fire,  new List<ushort>{ 633, 637, } },
            /*2*/{ Element.Water, new List<ushort>{ 632, 638, } },
            /*3*/{ Element.Earth, new List<ushort>{ 635, 640, } },
            /*4*/{ Element.Wind,  new List<ushort>{ 634, 639, } },
            /*5*/{ Element.Dark,  new List<ushort>{ 636, 641, } },
        };
        private DateTime _nextSpawn;
        public MapInfo Map { get; set; }
        public MapInfo Map2 { get; set; }
        public AcheronGuardian()
        {
            _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(AcheronGuardian));
            Map = ResourceCache.Instance.GetMaps()[Maps.ArkaWar];
            Map2 = ResourceCache.Instance.GetMaps()[Maps.ArcaBattle];
            Map.PlayerLeaves += OnPlayerLeave;
            Map2.PlayerLeaves += OnPlayerLeave;
        }
        public override void Initialize()
        {
            base.Initialize();
            ChangeState(EventState.Open);
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
            GetPlayerEventInfo(sender as Player).Eventer = false;
        }

        public override void OnTransition(EventState NextState)
        {
            switch(NextState)
            {
                case EventState.Open:
                    Trigger(EventState.Playing, TimeSpan.FromMinutes(1));
                    _ = Program.NoEventMapAnoucement("Acheron Guardian Will Start in 1min.");
                    break;
                case EventState.Playing:
                    Trigger(EventState.Playing, TimeSpan.FromMinutes(30));
                    _ = Program.NoEventMapAnoucement("Acheron Guardian is Started!.");

                    var obeliskCode = (ushort)(627 + Program.RandomProvider(5));
                    var randObeliskPos = Program.RandomProvider(_obeliskPositions.Count);
                    _obelisk = MonstersMng.Instance.CreateMonster(
                        obeliskCode,
                        ObjectType.Gate,
                        _obeliskMaps[randObeliskPos],
                        _obeliskPositions[randObeliskPos],
                        1,
                        (Element)MonstersMng.Instance.MonsterInfo[obeliskCode].MainAttribute);
                    _obelisk.Die += _obelisk_Die;
                    _logger.Debug("Obelisk Spawned in {0},{1}", _obelisk.Position.X, _obelisk.Position.Y);
                    break;
                case EventState.Closed:
                    if(_obelisk != null)
                    {
                        MonstersMng.Instance.DeleteMonster(_obelisk);
                        foreach(var m in _cursedMonsters)
                        {
                            MonstersMng.Instance.DeleteMonster(m);
                        }
                        _cursedMonsters.Clear();
                    }

                    var now = DateTime.Now;

                    if(now.Hour <= 12)
                        now = now.AddDays(-1);

                    var next = new DateTime(now.Year,now.Month,now.Day).AddDays(1).AddHours(12);

                    if (next.DayOfWeek == DayOfWeek.Wednesday)
                        next.AddDays(1);

                    Trigger(EventState.Open, next-now);
                    _players
                        .Where(x => x.Eventer)
                        .ToList()
                        .ForEach(x => x.Player.Character.WarpTo(27).Wait());

                    _logger.Information("Next Acheron Guardian will start {0}", next);
                    break;
                case EventState.None:
                    break;
            }
        }

        private void _obelisk_Die(object sender, EventArgs e)
        {
            _nextSpawn = DateTime.Now.Add(TimeLeft);
            _obelisk.Active = false;
            _cursedMonsters.ForEach(x => x.State = ObjectState.Dying);
            Trigger(EventState.Closed, TimeSpan.FromMinutes(1));
            var txt = _obelisk.Killer.Character.Name + " killed the " + _obelisk.Info.Name;
            _ = Program.MapAnoucement(Maps.ArkaWar, txt);
            _ = Program.MapAnoucement(Maps.ArcaBattle, txt);
        }

        public override void Update()
        {
            base.Update();

            if (CurrentState != EventState.Playing)
                return;

            if (_nextSpawn > DateTime.Now)
                return;

            _nextSpawn = DateTime.Now.AddMinutes(5);
            var codes = _cursedMonstersByElement[_obelisk.Element];
            for(var i = 0; i < 5; i++)
            {
                var mtype = codes[Program.RandomProvider(codes.Count)];
                var m = MonstersMng.Instance.CreateMonster(
                    mtype,
                    ObjectType.Monster,
                    _obelisk.MapID,
                    new Point(
                        _obelisk.Position.X + Program.RandomProvider(3, -6),
                        _obelisk.Position.Y + Program.RandomProvider(3, -6)
                        ),
                    1,
                    (Element)MonstersMng.Instance.MonsterInfo[mtype].MainAttribute);
                m.Die += Monster_Die;
            }
        }

        private void Monster_Die(object sender, EventArgs e)
        {
            var m = sender as Monster;
            m.Active = _obelisk.Active;
        }

        public override bool TryAdd(Player plr)
        {
            if (CurrentState == EventState.Playing)
            {
                _ = plr.Character.WarpTo(426);

                GetPlayerEventInfo(plr).Eventer = true;
            }
            else
            {
                _ = plr.Session.SendAsync(new SAcheronEventEnter { Result = 1 });
                return false;
            }
            return true;
        }
    }
}
