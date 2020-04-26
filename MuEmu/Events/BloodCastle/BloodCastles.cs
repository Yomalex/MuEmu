using MuEmu.Network.Game;
using MuEmu.Resources.Map;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Events.BloodCastle
{
    internal enum BCState
    {
        Close,
        Open,
        BeforeStart,
        Started,
        Ended,
    }
    internal struct BloodCastleInfo
    {
        public byte Bridge { get; set; }
        public ushort MinLevel { get; set; }
        public ushort MaxLevel { get; set; }
    }
    public class BloodCastles
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(BloodCastles));

        private static readonly BloodCastleInfo[][] _BCLevel =
            new BloodCastleInfo[2][]
            {
                new BloodCastleInfo[7]
                {
                    new BloodCastleInfo{ Bridge = 1, MinLevel = 15, MaxLevel = 80},
                    new BloodCastleInfo{ Bridge = 2, MinLevel = 81, MaxLevel = 130},
                    new BloodCastleInfo{ Bridge = 3, MinLevel = 131, MaxLevel = 180},
                    new BloodCastleInfo{ Bridge = 4, MinLevel = 181, MaxLevel = 230},
                    new BloodCastleInfo{ Bridge = 5, MinLevel = 231, MaxLevel = 280},
                    new BloodCastleInfo{ Bridge = 6, MinLevel = 281, MaxLevel = 330},
                    new BloodCastleInfo{ Bridge = 7, MinLevel = 331, MaxLevel = 400}
                },
                new BloodCastleInfo[7]
                {
                    new BloodCastleInfo{ Bridge = 1, MinLevel = 15, MaxLevel = 60},
                    new BloodCastleInfo{ Bridge = 2, MinLevel = 61, MaxLevel = 110},
                    new BloodCastleInfo{ Bridge = 3, MinLevel = 111, MaxLevel = 160},
                    new BloodCastleInfo{ Bridge = 4, MinLevel = 161, MaxLevel = 210},
                    new BloodCastleInfo{ Bridge = 5, MinLevel = 211, MaxLevel = 260},
                    new BloodCastleInfo{ Bridge = 6, MinLevel = 261, MaxLevel = 310},
                    new BloodCastleInfo{ Bridge = 7, MinLevel = 311, MaxLevel = 400}
                }
            };
        private static readonly TimeSpan r_EnterPeriod = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan r_LeavePeriod = TimeSpan.FromSeconds(30);
        private DateTimeOffset _next;
        private List<DateTimeOffset> _messages;
        private BloodCastle[] _bridges;
        private BCState _state;
        internal BCState State
        {
            get => _state;
            set
            {
                Logger.Information("State: {0} -> {1}", _state, value);
                _state = value;
                foreach (var bridge in Instance._bridges)
                {
                    if (bridge.State < value || value == BCState.Close)
                        bridge.State = value;
                }
            }
        }

        internal static readonly TimeSpan r_Duration = TimeSpan.FromMinutes(5);
        public static BloodCastles Instance { get; set; }

        private BloodCastles()
        {
            _bridges = new BloodCastle[8];
            for (var i = 0; i < _bridges.Length; i++)
            {
                _bridges[i] = new BloodCastle(this, i);
            }

            _state = BCState.Close;
            _next = DateTimeOffset.Now;
            _messages = new List<DateTimeOffset>();
        }

        public static void Initialize()
        {
            if (Instance != null)
                throw new Exception(nameof(BloodCastles) + "Already Initialized");

            Instance = new BloodCastles();
        }

        private static IEnumerable<Player> PlayersOut()
        {
            return Program.server.Clients
                                .Where(x => x.Player != null)
                                .Where(x => x.Player.Character != null)
                                .Select(x => x.Player)
                                .Where(x => (x.Character.MapID < Maps.BloodCastle1 || x.Character.MapID > Maps.BloodCastle7) && x.Character.MapID != Maps.BloodCastle8);
        }

        public static void Update()
        {
            switch(Instance._state)
            {
                case BCState.Close:
                    if (Instance._next <= DateTimeOffset.Now)
                    {
                        Instance.State = BCState.Open;
                        for(var i = TimeSpan.Zero; i < r_EnterPeriod; i += TimeSpan.FromMinutes(1))
                            Instance._messages.Add(Instance._next.Add(i));
                    }
                    break;
                case BCState.Open:
                    var start = Instance._next.Add(r_EnterPeriod);
                    if (start <= DateTimeOffset.Now)
                    {
                        Instance.State = BCState.BeforeStart;

                        var plrs = PlayersOut().ToList();
                        plrs.ForEach(x => x.Session.SendAsync(new SNotice(NoticeType.Gold, "Blood castle Entrance Closed")));
                    }
                    else
                    {
                        try
                        {
                            var m = Instance._messages.First(x => x < DateTimeOffset.Now);
                            Instance._messages.Remove(m);
                            var time = start - DateTimeOffset.Now;
                            Program.GlobalAnoucement($"{(int)Math.Ceiling(time.TotalMinutes)} minute(s) left to enter Blood castle.");
                        }
                        catch(Exception)
                        {

                        }
                    }
                    break;
                case BCState.BeforeStart:
                    break;
                case BCState.Started:
                    break;
                case BCState.Ended:
                    break;
            }

            foreach(var bridge in Instance._bridges)
            {
                bridge.Update();
            }
        }
        
        public static void MessengerAngelTalk(Player plr)
        {
            if (Instance._state != BCState.Open)
            {
                plr.Session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.RunningBC));
                return;
            }

            plr.Session.SendAsync(new STalk { Result = NPCWindow.MessengerAngel });
        }

        public static async void AngelKingTalk(Player plr)
        {
            var session = plr.Session;
            var bridge = Instance._bridges.FirstOrDefault(x => x.Players.Any(y => y == plr));

            if (bridge == null)
            {
                await session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC));
                return;
            }

            if(bridge.State == BCState.BeforeStart || (bridge.State == BCState.Started && bridge._nextStateIn > TimeSpan.FromMinutes(3)))
            {
                //await session.SendAsync(new SNotice(NoticeType.Blue, $"When the clock has 3:00 for finish give the weapon, not now."));
                await session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC));
                return;
            }

            if(bridge.State == BCState.Ended)
            {
                await session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.CompletedBC));
                return;
            }

            var item = plr.Character.Inventory.FindAllItems(new ItemNumber(13, 19)).FirstOrDefault();

            if (item == null)
            {
                await session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.InvalidBC));
                return;
            }

            await plr.Character.Inventory.Delete(item);

            await session.SendAsync(new SCommand(ServerCommandType.EventMsg, (byte)EventMsg.SucceedBC));
            bridge.Winner = plr;
        }

        public static byte RemainTime()
        {
            switch(Instance._state)
            {
                case BCState.Close:
                    return (byte)Math.Ceiling((Instance._next - DateTimeOffset.Now).TotalMinutes);
                case BCState.Open:
                    return 0;
                case BCState.Started:
                    return (byte)Math.Ceiling((Instance._next.AddHours(1) - DateTimeOffset.Now).TotalMinutes);
            }
            return 0;
        }

        public static byte GetNeededLevel(Player plr)
        {
            var @char = plr.Character;

            if (@char.MasterClass)
            {
                return 8;
            }
            else if (@char.BaseClass == HeroClass.MagicGladiator || @char.BaseClass == HeroClass.DarkLord)
            {
                return _BCLevel[1].First(x => x.MinLevel <= @char.Level && x.MaxLevel >= @char.Level).Bridge;
            }
            else
            {
                return _BCLevel[0].First(x => x.MinLevel <= @char.Level && x.MaxLevel >= @char.Level).Bridge;
            }
        }

        public static bool TryAdd(int bridge, Player plr)
        {
            return Instance._bridges[bridge - 1].TryAdd(plr);
        }
    }
}
