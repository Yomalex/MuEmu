using MuEmu.Monsters;
using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Events
{
    public class GoldenInvasion
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GoldenInvasion));
        private List<Monster> _goldenMob = new List<Monster>();
        private DateTime _nextInvasion = DateTime.Now;
        private readonly Maps[] dekronMaps = new Maps[] { Maps.Lorencia, Maps.Noria, Maps.Davias };
        private Maps dekronMap;
        private MapInfo dekronMapInfo;
        private int dekronCount;

        public void AddMonster(Monster mob)
        {
            _goldenMob.Add(mob);
            mob.Die += Gold_Die;
        }

        public void Update()
        {
            if(dekronCount == 0 && DateTime.Now >= _nextInvasion)
            {
                _nextInvasion = DateTime.Now.AddHours(2);
                Start();
            }
        }

        public async void Start()
        {
            dekronCount = 0;
            dekronMap = dekronMaps[new Random().Next(3)];
            foreach(var gold in _goldenMob)
            {
                if(gold.Info.Monster == 79) // golden Dekron
                {
                    var p = MonstersMng.Instance.GetSpawn(dekronMap, 0, 255, 0, 255);
                    gold.Warp(dekronMap, (byte)p.X, (byte)p.Y);
                    Logger.Information("Dragon spawn on {0},{1}", (byte)p.X, (byte)p.Y);
                    dekronCount++;
                }

                gold.Active = true;
            }

            dekronMapInfo = ResourceCache.Instance
                .GetMaps()[dekronMap];

            dekronMapInfo.DragonInvasion = true;

            await dekronMapInfo
                .SendAsync(new SEventState(MapEvents.GoldenInvasion, true));

            await Program.NoEventMapAnoucement($"[{dekronMap}] Golden Dragon Invasion on");
        }

        private async void Gold_Die(object sender, EventArgs e)
        {
            var mob = sender as Monster;
            mob.Active = false;
            if (mob.Info.Monster == 79)
                dekronCount--;

            if(dekronCount <= 0)
            {
                foreach (var gold in _goldenMob)
                {
                    gold.Active = false;
                }
                dekronMapInfo.DragonInvasion = false;

                await dekronMapInfo
                    .SendAsync(new SEventState(MapEvents.GoldenInvasion, false));


                await Program.NoEventMapAnoucement("Golden Invasion ends");
            }
        }
    }
}
