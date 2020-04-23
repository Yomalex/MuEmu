using MuEmu.Monsters;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Network.QuestSystem
{
    public class Marlon
    {
        internal static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Marlon));

        private static Marlon s_instance;

        private readonly TimeSpan r_TeleportTime = TimeSpan.FromSeconds(900);

        private Monster _marlon;
        private DateTimeOffset _nextWarp;

        private Marlon()
        {
            _marlon = MonstersMng.Instance.Monsters.Find(x => x.Info.Monster == (ushort)229);
            _nextWarp = DateTimeOffset.Now;
        }

        public static void Initialize()
        {
            s_instance = new Marlon();
        }

        public static void Run()
        {
            if ((s_instance?._marlon??null) == null)
                return;

            if(s_instance._nextWarp < DateTimeOffset.Now)
            {
                s_instance._nextWarp = DateTimeOffset.Now.Add(s_instance.r_TeleportTime);
                switch(new Random().Next(4))
                {
                    case 0:
                        s_instance._marlon.Warp(Maps.Davias, 198, 47);
                        s_instance._marlon.Direction = 2;
                        _logger.Information("Warp to Davias");
                        break;
                    case 1:
                        s_instance._marlon.Warp(Maps.Lorencia, 137, 87);
                        s_instance._marlon.Direction = 1;
                        _logger.Information("Warp to Lorencia");
                        break;
                    case 2:
                        s_instance._marlon.Warp(Maps.Noria, 169, 89);
                        s_instance._marlon.Direction = 2;
                        _logger.Information("Warp to Noria");
                        break;
                    case 3:
                        s_instance._marlon.Warp(Maps.Atlans, 17, 25);
                        s_instance._marlon.Direction = 2;
                        _logger.Information("Warp to Atlans");
                        break;
                }
            }
        }
    }
}
