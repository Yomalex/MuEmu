using BlubLib.Serialization;
using MuEmu.Network;
using MuEmu.Network.Auth;
using MuEmu.Network.CashShop;
using MuEmu.Network.ConnectServer;
using MuEmu.Network.Game;
using MuEmu.Network.Global;
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
using MuEmu.Network.Event;
using WebZen.Util;
using MuEmu.Network.QuestSystem;
using MuEmu.Events.LuckyCoins;
using MuEmu.Events.EventChips;
using MuEmu.Events.BloodCastle;
using System.Threading.Tasks;
using MuEmu.Entity;
using System.Linq;

namespace MuEmu
{
    class Program
    {
        private static CommandHandler<GSSession> handler;
        public static WZGameServer server;
        public static CSClient client;
        public static string ConnectionString { get; set; }
        public static bool AutoRegistre { get; set; }
        public static ushort ServerCode { get; set; }

        static void Main(string[] args)
        {
            Predicate<GSSession> MustNotBeLoggedIn = session => session.Player.Status == LoginStatus.NotLogged;
            Predicate<GSSession> MustBeLoggedIn = session => session.Player.Status == LoginStatus.Logged;
            Predicate<GSSession> MustBePlaying = session => session.Player.Status == LoginStatus.Playing;

            var xml = ResourceLoader.XmlLoader<ServerInfoDto>("./Server.xml");

            Log.Logger = new LoggerConfiguration()
                .Destructure.ByTransforming<IPEndPoint>(endPoint => endPoint.ToString())
                .Destructure.ByTransforming<EndPoint>(endPoint => endPoint.ToString())
                .WriteTo.File("GameServer.txt")
                .WriteTo.Console(outputTemplate: "[{Level} {SourceContext}][{AID}:{AUser}] {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();

            Console.Title = $"GameServer .NetCore2 [{xml.Code}]{xml.Name} Client:{xml.Version}#!{xml.Serial} DB:"+ xml.DataBase;

            ConnectionString = $"Server={xml.DBIp};port=3306;Database={xml.DataBase};user={xml.BDUser};password={xml.DBPassword};Convert Zero Datetime=True;";
            
            SimpleModulus.LoadDecryptionKey("Dec1.dat");
            SimpleModulus.LoadEncryptionKey("Enc2.dat");

            var ip = new IPEndPoint(IPAddress.Parse(xml.IP), xml.Port);
            var csIP = new IPEndPoint(IPAddress.Parse(xml.ConnectServerIP), 44405);
            AutoRegistre = xml.AutoRegistre;
            ServerCode = (ushort)xml.Code;

            var mh = new MessageHandler[] {
                new FilteredMessageHandler<GSSession>()
                    .AddHandler(new AuthServices())
                    .AddHandler(new GlobalServices())
                    .AddHandler(new GameServices())
                    .AddHandler(new CashShopServices())
                    .AddHandler(new EventServices())
                    .AddHandler(new QuestSystemServices())
                    .RegisterRule<CIDAndPass>(MustNotBeLoggedIn)
                    .RegisterRule<CCharacterList>(MustBeLoggedIn)
                    .RegisterRule<CCharacterMapJoin>(MustBeLoggedIn)
                    .RegisterRule<CCharacterMapJoin2>(MustBeLoggedIn)
            };

            var mf = new MessageFactory[]
            {
                new AuthMessageFactory(),
                new GlobalMessageFactory(),
                new GameMessageFactory(),
                new CashShopMessageFactory(),
                new EventMessageFactory(),
                new QuestSystemMessageFactory(),
            };
            server = new WZGameServer(ip, mh, mf);
            server.ClientVersion = xml.Version;
            server.ClientSerial = xml.Serial;

            var cmh = new MessageHandler[]
            {
                new FilteredMessageHandler<CSClient>()
                .AddHandler(new CSServices())
            };

            var cmf = new MessageFactory[]
            {
                new CSMessageFactory()
            };

            ResourceCache.Initialize(".\\Data");
            MonstersMng.Initialize();
            MonstersMng.Instance.LoadMonster("./Data/Monsters/Monster.txt");
            EventInitialize();

            MonstersMng.Instance.LoadSetBase("./Data/Monsters/MonsterSetBase.txt");

            SubSystem.Initialize();

            try
            {
                client = new CSClient(csIP, cmh, cmf, (ushort)xml.Code, server, (byte)xml.Show);
            }catch(Exception)
            {
                Log.Error("Connect Server Unavailable");
            }

            Log.Information("Disconnecting Accounts");
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
                Log.Error("MySQL unavailable.");
                Task.Delay(15000);
                return;
            }

            Log.Information("Server Ready");

            handler = new CommandHandler<GSSession>();
            handler.AddCommand(new Command<GSSession>("exit", (object a, EventArgs b) => Environment.Exit(0)))
                .AddCommand(new Command<GSSession>("quit", (object a, EventArgs b) => Environment.Exit(0)))
                .AddCommand(new Command<GSSession>("stop", (object a, EventArgs b) => Environment.Exit(0)))
                .AddCommand(new Command<GSSession>("reload")
                    .AddCommand(new Command<GSSession>("shops", (object a, EventArgs b) => ResourceCache.Instance.ReloadShops()))
                    .AddCommand(new Command<GSSession>("gates", (object a, EventArgs b) => ResourceCache.Instance.ReloadGates())))
                .AddCommand(new Command<GSSession>("db")
                    .AddCommand(new Command<GSSession>("migrate", Migrate))
                    .AddCommand(new Command<GSSession>("create", Create))
                    .AddCommand(new Command<GSSession>("delete", Delete)));

            while (true)
            {
                var input = Console.ReadLine();
                if (input == null)
                    break;

                handler.ProcessCommands(null, input);
            }
        }

        static void EventInitialize()
        {
            LuckyCoins.Initialize();
            EventChips.Initialize();
            BloodCastles.Initialize();
        }

        public static async Task GlobalAnoucement(string text)
        {
            await server.SendAll(new SNotice(NoticeType.Gold, text));
            Log.Information("Global Announcement: " + text);
        }

        public static void Create(object a, EventArgs b)
        {
            Log.Information("Creating DB");
            using (var db = new GameContext())
                db.Database.EnsureCreated();
        }

        public static void Migrate(object a, EventArgs b)
        {
            using (var db = new GameContext())
            {
                Log.Information("Dropping DB");
                db.Database.EnsureDeleted();
                Log.Information("Creating DB");
                db.Database.EnsureCreated();
            }
        }

        public static void Delete(object a, EventArgs b)
        {
            Log.Information("Dropping DB");
            using (var db = new GameContext())
                db.Database.EnsureDeleted();
        }
    }
}
