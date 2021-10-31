using BlubLib.Serialization;
using MuEmu.Resources;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Net;
using WebZen.Handlers;
using WebZen.Network;
using WebZen.Serialization;
using MuEmu.Resources.XML;
using MuEmu.Monsters;
using WebZen.Util;
using MuEmu.Events.LuckyCoins;
using MuEmu.Events.EventChips;
using MuEmu.Events.BloodCastle;
using System.Threading.Tasks;
using MuEmu.Entity;
using System.Linq;
using MuEmu.Events.DevilSquare;
using MuEmu.Events;
using MuEmu.Util;
using MuEmu.Events.Kanturu;
using MuEmu.Events.ChaosCastle;
using MuEmu.Resources.BMD;
using Serilog.Sinks.File;
using MuEmu.Resources.Game;
using Serilog.Core;
using MuEmu.Events.Crywolf;
using MuEmu.Events.ImperialGuardian;
using MuEmu.Events.DoubleGoer;
using MuEmu.Network;
using MU.Network.CashShop;
using MU.Network.Auth;
using MU.Network.Global;
using MU.Network.Game;
using MU.Network.Event;
using MU.Network.Guild;
using MU.Network.AntiHack;
using MU.Network.PCPShop;
using MU.Network.GensSystem;
using MU.Network.QuestSystem;
using MU.Network;
using MU.Resources;
using MU.Resources.Game;
using MU.Resources.BMD;
using MuEmu.Network.ConnectServer;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;
using MuEmu.Events.MoonRabbit;
using MuEmu.Game;
using MuEmu.Events.WhiteWizard;
using MuEmu.Network.GameServices;
using MuEmu.Events.Event_Egg;
using MuEmu.Events.Rummy;
using MuEmu.Events.CastleSiege;

namespace MuEmu
{
    class Program
    {
        private static Random s_rand = new Random();

        public static CommandHandler<GSSession> Handler { get; } = new CommandHandler<GSSession>();
        public static WZGameServer server;
        public static CSClient client;
        public static string ConnectionString { get; set; }
        public static bool AutoRegistre { get; set; }
        public static ushort ServerCode { get; set; }
        public static int ServerGroup => ServerCode / 20;
        //public static float Experience { get; set; }
        public static ExpManagement Experience { get; } = new ExpManagement();
        public static float Zen { get; set; }
        public static int DropRate { get; set; }
        public static ServerSeason Season { get; set; }
        public static string Name { get; set; }

        public static EventManagement EventManager;
        public static GlobalEvents GlobalEventsManager;
        public static GoldenInvasion GoldenInvasionManager;

        private static bool NewEncode(ServerSeason season) => season switch
        {
            ServerSeason.Season6Kor => false,
            ServerSeason.Season9Eng => true,
            _ => throw new NotImplementedException()
        };

        static void Main(string[] args)
        {
            Predicate<GSSession> MustNotBeLoggedIn = session => session.Player.Status == LoginStatus.NotLogged;
            Predicate<GSSession> MustBeLoggedIn = session => session.Player.Status == LoginStatus.Logged;
            Predicate<GSSession> MustBePlaying = session => session.Player.Status == LoginStatus.Playing;
            Predicate<GSSession> MustBeLoggedOrPlaying = session => session.Player.Status == LoginStatus.Logged || session.Player.Status == LoginStatus.Playing;
            Predicate<GSSession> MustBeGameMaster = session => (session.Player.Character.CtlCode&ControlCode.GameMaster) != 0;

            string output = "{Timestamp: HH:mm:ss} [{Level} {SourceContext}][{AID}:{AUser}] {Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .Destructure.ByTransforming<IPEndPoint>(endPoint => endPoint.ToString())
                .Destructure.ByTransforming<EndPoint>(endPoint => endPoint.ToString())
                .WriteTo.File("GameServer_.txt", outputTemplate: output, rollingInterval: RollingInterval.Day)
                .WriteTo.Console(outputTemplate: output)
                .MinimumLevel.Debug()
                .CreateLogger();

            ServerMessages.Initialize();
            ServerMessages.LoadMessages("./Data/Lang/ServerMessages(es).xml");

            if (!File.Exists("./Server.xml"))
            {
                Log.Logger.Error(ServerMessages.GetMessage(Messages.Server_Cfg));
                ResourceLoader.XmlSaver("./Server.xml", new ServerInfoDto());
                Task.Delay(10000);
                return;
            }

            var xml = ResourceLoader.XmlLoader<ServerInfoDto>("./Server.xml");
            ServerMessages.LoadMessages($"./Data/Lang/ServerMessages({xml.Lang}).xml");

            Name = xml.Name;
            Console.Title = ServerMessages.GetMessage(Messages.Server_Title, xml.Code, xml.Name, xml.Client.Version, xml.Client.Serial, xml.Database.DataBase);

            ConnectionString = $"Server={xml.Database.DBIp};port=3306;Database={xml.Database.DataBase};user={xml.Database.BDUser};password={xml.Database.DBPassword};Convert Zero Datetime=True;";

            GameContext.ConnectionString = ConnectionString;
            SimpleModulus.LoadDecryptionKey("./Data/Dec1.dat");
            SimpleModulus.LoadEncryptionKey("./Data/Enc2.dat");
            byte[] key = { 0x44, 0x9D, 0x0F, 0xD0, 0x37, 0x22, 0x8F, 0xCB, 0xED, 0x0D, 0x37, 0x04, 0xDE, 0x78, 0x00, 0xE4, 0x33, 0x86, 0x20, 0xC2, 0x79, 0x35, 0x92, 0x26, 0xD4, 0x37, 0x37, 0x30, 0x98, 0xEF, 0xA4, 0xDE };
            PacketEncrypt.Initialize(key);

            var ip = new IPEndPoint(IPAddress.Parse(xml.Connection.IP), xml.Connection.Port);
            var csIP = new IPEndPoint(IPAddress.Parse(xml.Connection.ConnectServerIP), 44405);
            AutoRegistre = xml.AutoRegister;
            ServerCode = (ushort)xml.Code;
            Experience.BaseExpRate = xml.GamePlay.Experience / 100.0f;
            Experience.GoldChannel = xml.GamePlay.GoldExperience / 100.0f;
            Zen = xml.GamePlay.Zen;
            DropRate = xml.GamePlay.DropRate;
            Season = xml.Season;

            VersionSelector.Initialize(xml.Season);

            var mh = new MessageHandler[] {
                new FilteredMessageHandler<GSSession>()
                    .AddHandler(new AuthServices())
                    .AddHandler(new GlobalServices())
                    .AddHandler(new GameServices())
                    .AddHandler(new CashShopServices())
                    .AddHandler(new EventServices())
                    .AddHandler(new QuestSystemServices())
                    .AddHandler(new GuildServices())
                    .AddHandler(new AntiHackServices())
                    .AddHandler(new PCPShopServices())
                    .AddHandler(new GensServices())
                    .RegisterRule<CIDAndPass>(MustNotBeLoggedIn)
                    .RegisterRule<CCharacterList>(MustBeLoggedIn)
                    .RegisterRule<CCharacterMapJoin>(MustBeLoggedIn)
                    .RegisterRule<CCharacterMapJoin2>(MustBeLoggedIn)
                    .RegisterRule<CCloseWindow>(MustBePlaying)
                    .RegisterRule<CDataLoadOK>(MustBePlaying)
                    .RegisterRule<CAction>(MustBePlaying)
            };
            var mf = new MessageFactory[]
            {
                new AuthMessageFactory(Season),
                new GlobalMessageFactory(),
                new GameMessageFactory(Season),
                new CashShopMessageFactory(Season),
                new EventMessageFactory(),
                new QuestSystemMessageFactory(),
                new GuildMessageFactory(),
                new AntiHackMessageFactory(),
                new PCPShopMessageFactory(),
                new GensMessageFactory(),
            };
            server = new WZGameServer(ip, mh, mf, NewEncode(Season));
            server.ClientVersion = xml.Client.Version;
            server.ClientSerial = xml.Client.Serial;

            var cmh = new MessageHandler[]
            {
                new FilteredMessageHandler<CSClient>()
                .AddHandler(new CSServices())
            };

            var cmf = new MessageFactory[]
            {
                new CSMessageFactory()
            };

            try
            {
                ResourceCache.Initialize(".\\Data");
                MasterLevel.Initialize();
                GuildManager.Initialize();
                PartyManager.Initialzie(xml.GamePlay.MaxPartyLevelDifference);
                DuelSystem.Initialize();
                CashShop.Initialize(xml.Client.CashShopVersion.Split(".").Select(x => ushort.Parse(x)).ToArray());
                Pentagrama.Initialize();
                // Event Config
                EventConfig(xml);
                MonstersMng.Initialize();
                MonstersMng.Instance.LoadMonster(xml.Files.Monsters);
                MonsterIA.Initialize("./Data/Monsters/");
                EventInitialize();

                MapServerManager.Initialize(xml.Files.MapServer);
                MonstersMng.Instance.LoadSetBase(xml.Files.MonsterSetBase);
                SubSystem.Initialize();
                Marlon.Initialize();
            }
            catch(MySql.Data.MySqlClient.MySqlException ex)
            {
                Log.Error(ex, ServerMessages.GetMessage(Messages.Server_MySQL_Error));
                //Migrate(null, new EventArgs());
                //Log.Information("Server needs restart to reload all changes");
                Task.Delay(10000);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ServerMessages.GetMessage(Messages.Server_Error));
            }

            SubSystem.CSSystem(csIP, cmh, cmf, (byte)xml.Show, xml.Connection.APIKey);

            Log.Information(ServerMessages.GetMessage(Messages.Server_Disconnecting_Accounts));
            try
            {
                using (var db = new GameContext())
                {
                    var accs = from acc in db.Accounts
                               where acc.IsConnected && acc.ServerCode == xml.Code
                               select acc;

                    foreach (var acc in accs)
                        acc.IsConnected = false;

                    db.Accounts.UpdateRange(accs);
                    db.SaveChanges();
                }
            }catch(Exception)
            {
                Log.Error("MySQL unavailable. please use \"db create\" or \"db migrate\" command");
                Task.Delay(15000);
            }

            Log.Information(ServerMessages.GetMessage(Messages.Server_Ready));

            Handler
                .AddCommand(new Command<GSSession>("help", Help, help:"Use help <cmd> for get help on any command"))
                .AddCommand(new Command<GSSession>("exit", Close, autority: MustBeGameMaster, help: "Close game server"))
                .AddCommand(new Command<GSSession>("quit", Close, autority: MustBeGameMaster, help: "Close game server"))
                .AddCommand(new Command<GSSession>("stop", Close, autority: MustBeGameMaster, help: "Close game server"))
                .AddCommand(new Command<GSSession>("reload", autority:MustBeGameMaster, help: "Reload GameServer section")
                    .AddCommand(new Command<GSSession>("shops", (object a, CommandEventArgs b) => ResourceCache.Instance.ReloadShops(), help:"Reload shop List"))
                    .AddCommand(new Command<GSSession>("gates", (object a, CommandEventArgs b) => ResourceCache.Instance.ReloadGates(), help:"Reload gate list")))
                .AddCommand(new Command<GSSession>("create", autority: MustBeGameMaster, help:"Create client side files")
                    .AddCommand(new Command<GSSession>("movereq", DumpMoveReq, null, "Create MoveRequest file for client")))
                .AddCommand(new Command<GSSession>("db", autority: MustBeGameMaster, help:"Generate db changes, use help")
                    .AddCommand(new Command<GSSession>("migrate", Migrate, help:"delete DB and create it again"))
                    .AddCommand(new Command<GSSession>("create", Create, help:"Create DB"))
                    .AddCommand(new Command<GSSession>("delete", Delete, help:"Delete DB")))
                .AddCommand(new Command<GSSession>("!", (object a, CommandEventArgs b) => GlobalAnoucement(b.Argument).Wait(), MustBeGameMaster).SetPartial())
                .AddCommand(new Command<GSSession>("/").SetPartial()
                    .AddCommand(new Command<GSSession>("p", PostCommand))//Post
                    .AddCommand(new Command<GSSession>("add").SetPartial()
                        .AddCommand(new Command<GSSession>("str", Character.AddStr))
                        .AddCommand(new Command<GSSession>("agi", Character.AddAgi))
                        .AddCommand(new Command<GSSession>("vit", Character.AddVit))
                        .AddCommand(new Command<GSSession>("ene", Character.AddEne))
                        .AddCommand(new Command<GSSession>("cmd", Character.AddCmd)))
                    .AddCommand(new Command<GSSession>("set", autority:MustBeGameMaster)
                        .AddCommand(new Command<GSSession>("hp", (object a, CommandEventArgs b) => ((GSSession)a).Player.Character.Health = float.Parse(b.Argument)))
                        .AddCommand(new Command<GSSession>("zen", (object a, CommandEventArgs b) => ((GSSession)a).Player.Character.Money = uint.Parse(b.Argument))
                        .AddCommand(new Command<GSSession>("exp", (object a, CommandEventArgs b) => ((GSSession)a).Player.Character.Experience = uint.Parse(b.Argument)))))
                    .AddCommand(new Command<GSSession>("levelup", LevelUp, MustBeGameMaster, "Level up current character, use: '/levelup 100' add 100 levels to current character"))
                    .AddCommand(new Command<GSSession>("reset", Character.Reset, null, "Resets current Character"))
                    .AddCommand(new Command<GSSession>("drop", CreateItem, MustBeGameMaster, "Create item <Number>")))
                //.AddCommand(new Command<GSSession>("~").SetPartial())
                /*.AddCommand(new Command<GSSession>("]").SetPartial())*/;

            while (true)
            {
                var input = Console.ReadLine();
                if (input == null)
                    break;

                Handler.ProcessCommands(null, input);
            }
        }

        private static void CreateItem(object sender, CommandEventArgs e)
        {
            var session = sender as GSSession;
            var @char = session.Player.Character;
            if (session == null)
                return;

            var map = session.Player.Character.Map;
            try
            {
                var item = new Item(ushort.Parse(e.Argument));
                map.AddItem(@char.Position.X, @char.Position.Y, item);
            }catch(Exception ex)
            {
                session.Exception(ex);
            }
        }

        private static void EventConfig(ServerInfoDto xml)
        {
            GlobalEventsManager = new GlobalEvents();
            GoldenInvasionManager = new GoldenInvasion();

            if (xml.Events == null)
                return;

            foreach(var e in xml.Events)
            {
                var ev = new GlobalEvent(GlobalEventsManager)
                { 
                    Active = e.active, 
                    Rate = e.rate,
                    Duration = TimeSpan.FromSeconds(e.duration),
                    Start = DateTime.Parse(e.start),
                    RepeatType = e.repeat,
                    ExpAdd = e.experienceAdd
                };

                foreach (var c in e.Conditions)
                    ev.AddRange(new Item((ItemNumber)c.item, Options: new { Plus = c.itemLevel }), c.mobMinLevel, c.mobMaxLevel, c.map);

                if(ev.Active)
                    GlobalEventsManager.AddEvent(e.name, ev);
            }
        }

        public static int RandomProvider(int Max, int Min = 0)
        {
            return s_rand.Next(Min, Max);
        }

        private static void MakeXOR(byte[] data, int offset, int length)
        {
            var xor = new byte[] { 0xFC, 0xCF, 0xAB };
            for(var i = 0; i < data.Length- offset; i++)
            {
                data[i+ offset] ^= xor[(i % length) % xor.Length];
            }
        }

        private static void DumpMoveReq(object sender, CommandEventArgs e)
        {
            var gates = ResourceCache.Instance.GetGates();
            var moves = gates
                .Where(x => x.Value.Move != -1)
                .OrderBy(x => x.Value.Move)
                .Select(x => x.Value);

            using(var fs = new FileStream("./MoveReq.bmd", FileMode.Create))
            {
                var num = BitConverter.GetBytes(moves.Count());
                fs.Write(num, 0, 4);

                var data = new byte[0xFA0];
                using (var ms = new MemoryStream(data))
                {
                    foreach (var mov in moves)
                    {
                        var bmdData = new MoveReqBMD
                        {
                            Gate = mov.Number,
                            Level = mov.ReqLevel,
                            MoveNumber = mov.Move,
                            Zen = mov.ReqZen,
                            ClientName = mov.Name,
                            ServerName = mov.Name,
                        };
                        Serializer.Serialize(ms, bmdData);
                    }
                }

                MakeXOR(data, 0, 80);
                fs.Write(data, 0, data.Length);
            }

            Log.Information("MoveReq.bmd Created");

            using (var fs = new FileStream("./Gate.bmd", FileMode.Create))
            {
                var num = BitConverter.GetBytes(moves.Count());
                //fs.Write(num, 0, 4);

                var data = new byte[7168];
                using (var ms = new MemoryStream(data))
                {
                    foreach (var mov in moves)
                    {
                        var bmdData = new GateBMD
                        {
                            Flag = mov.GateType,
                            Map = mov.Map,
                            Dir = mov.Dir,
                            Level = mov.ReqLevel,
                            X1 = (byte)mov.Door.Left,
                            Y1 = (byte)mov.Door.Top,
                            X2 = (byte)mov.Door.Right,
                            Y2 = (byte)mov.Door.Bottom,
                            GateNumber = (ushort)mov.Number,
                            BZLevel = 400,
                            BZone = 1,
                        };
                        Serializer.Serialize(ms, bmdData);
                    }
                }

                MakeXOR(data, 0, 14);
                fs.Write(data, 0, data.Length);
            }
            Log.Information("Gate.bmd Created");
        }

        static void EventInitialize()
        {
            EventManager = new EventManagement();
            EventManager
                .AddEvent(Events.Events.BloodCastle, new BloodCastles())
                .AddEvent(Events.Events.DevilSquared, new DevilSquares())
                .AddEvent(Events.Events.Kanturu, new Kanturu())
                .AddEvent(Events.Events.ChaosCastle, new ChaosCastles())
                .AddEvent(Events.Events.Crywolf, new Crywolf())
                .AddEvent(Events.Events.ImperialGuardian, new ImperialGuardian())
                .AddEvent(Events.Events.MoonRabbit, new MoonRabbit())
                .AddEvent(Events.Events.WhiteWizard, new WhiteWizard())
                .AddEvent(Events.Events.EventEgg, new EventEgg())
                .AddEvent(Events.Events.MuRummy, new MuRummy())
                .AddEvent(Events.Events.CastleSiege, new CastleSiege())
                //.AddEvent(Events.Events.DoubleGoer, new DoubleGoer())
                ;
            LuckyCoins.Initialize();
            EventChips.Initialize();
        }

        public static async Task GlobalAnoucement(string text)
        {
            await server.SendAll(new SNotice(NoticeType.Gold, text));
            Log.Information(ServerMessages.GetMessage(Messages.Server_GlobalAnnouncement, text));
        }

        public static async Task MapAnoucement(Maps map, string text)
        {
            await ResourceCache.Instance
                .GetMaps()[map]
                .SendAsync(new SNotice(NoticeType.Gold, text));
            Log.Information(ServerMessages.GetMessage(Messages.Server_MapAnnouncement) + text, map);
        }

        public static async Task NoEventMapSendAsync(object message)
        {
            await server
                .Clients
                .Where(x => x.Player.Status == LoginStatus.Playing && !x.Player.Character.Map.IsEvent)
                .SendAsync(message);
        }
        public static async Task NoEventMapAnoucement(string text)
        {
            await NoEventMapSendAsync(new SNotice(NoticeType.Gold, text));
            Log.Information(ServerMessages.GetMessage(Messages.Server_NoEventMapAnnouncement, text));
        }

        public static async void PostCommand(object a, CommandEventArgs b)
        {
            var session = a as GSSession;
            await server.SendAll(new SChatNickName(session.Player.Character.Name, $"~# {b.Argument}"));
        }

        public static void Help(object a, CommandEventArgs b)
        {
            string output = "";
            if (string.IsNullOrWhiteSpace(b.Argument))
            {
                var list = Handler.GetCommandList().Select(x => x.FullName());
                output = "Command List:\n\t"+string.Join("\n\t", list);
            }
            else
            {
                var cmd = Handler.FindCommand(a as GSSession, b.Argument);
                if (cmd != null)
                    output = cmd.Help();
                else
                    output = "Invalid Command";
            }

            if(a==null)
            {
                Log.Information(output);
            }else
            {
                var session = a as GSSession;
                var outputs = output.Split("\n\t");
                foreach(var o in outputs)
                {
                    session.SendAsync(new SNotice(NoticeType.Blue, o)).Wait();
                }
            }
        }

        public static void Close(object a, EventArgs b)
        {
            if (a != null)
                return;

            GlobalAnoucement(ServerMessages.GetMessage(Messages.Server_Close)).Wait();

            Task.Delay(30000);

            Environment.Exit(0);
        }

        public static void Create(object a, EventArgs b)
        {
            if (a != null)
                return;

            Log.Information("Creating DB");
            using (var db = new GameContext())
                db.Database.EnsureCreated();
            Log.Information("Created DB");
        }

        public static void Migrate(object a, EventArgs b)
        {
            if (a != null)
                return;

            using (var db = new GameContext())
            {
                Log.Information("Dropping DB");
                db.Database.EnsureDeleted();
                Log.Information("Creating DB");
                db.Database.EnsureCreated();
                Log.Information("Created DB");
            }
        }

        public static void Delete(object a, EventArgs b)
        {
            if (a != null)
                return;

            Log.Information("Dropping DB");
            using (var db = new GameContext())
                db.Database.EnsureDeleted();
            Log.Information("Dropped DB");
        }
        public static void LevelUp(object a, CommandEventArgs b)
        {
            var session = a as GSSession;

            if(!string.IsNullOrWhiteSpace(b.Argument))
            {
                var lvls = uint.Parse(b.Argument);
                while(lvls-- > 0)
                {
                    session.Player.Character.Experience = session.Player.Character.NextExperience;
                }
            }else
            {
                session.Player.Character.Experience = session.Player.Character.NextExperience;
            }
        }
    }
}
