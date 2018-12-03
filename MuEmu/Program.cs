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

namespace MuEmu
{
    class Program
    {
        public static WZGameServer server;
        public static CSClient client;
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
            
            SimpleModulus.LoadDecryptionKey("Dec1.dat");
            SimpleModulus.LoadEncryptionKey("Enc2.dat");

            var ip = new IPEndPoint(IPAddress.Parse(xml.IP), xml.Port);
            var csIP = new IPEndPoint(IPAddress.Parse(xml.ConnectServerIP), 44405);

            var mh = new MessageHandler[] {
                new FilteredMessageHandler<GSSession>()
                    .AddHandler(new AuthServices())
                    .AddHandler(new GlobalServices())
                    .AddHandler(new GameServices())
                    .AddHandler(new CashShopServices())
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
                new CashShopMessageFactory()
            };
            server = new WZGameServer(ip, mh, mf);
            server.ClientVersion = xml.Version;

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
            MonstersMng.Instance.LoadSetBase("./Data/Monsters/MonsterSetBase.txt");

            SubSystem.Initialize();

            try
            {

                if(xml.Show != 0)
                    client = new CSClient(csIP, cmh, cmf, (ushort)xml.Code, server);
            }catch(Exception)
            {
                Log.Error("Connect Server Unavailable");
            }

            Log.Information("Server Ready");

            while (true)
            {
                var input = Console.ReadLine();
                if (input == null)
                    break;

                if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase) ||
                    input.Equals("quit", StringComparison.InvariantCultureIgnoreCase) ||
                    input.Equals("stop", StringComparison.InvariantCultureIgnoreCase))
                    break;
            }
        }
    }
}
