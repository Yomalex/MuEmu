using CSEmu.Network;
using CSEmu.Network.Services;
using Serilog;
using System;
using System.Net;
using WebZen.Handlers;
using WebZen.Network;
using System.Linq;
using System.Threading;
using MuEmu.Entity;
using System.Collections.Generic;

namespace CSEmu
{
    class Program
    {
        public static WZConnectServer server;
        public static WZChatServer WZChatServer;
        public static ClientManager Clients;
        private static readonly Dictionary<int, int> _classSuperior = new Dictionary<int, int>
        {
            { 1, 1 },
            { 5, 2 },
            { 10, 3 },
            { 30, 4 },
            { 50, 5 },
            { 100, 6 },
            { 200, 7 },
            { 300, 8 },
            { 9999, 9 },
        };
        private static readonly Dictionary<int, int> _class = new Dictionary<int, int>
        {
            { 6000, 10 },
            { 3000, 11 },
            { 1500, 12 },
            { 500, 13 },
            { 0, 14 }
        };
        static void Main(string[] args)
        {
            Serilog.Core.Logger logger = new LoggerConfiguration()
                .Destructure.ByTransforming<IPEndPoint>(endPoint => endPoint.ToString())
                .Destructure.ByTransforming<EndPoint>(endPoint => endPoint.ToString())
                .WriteTo.File("ConnectServer.txt")
                .WriteTo.Console(outputTemplate: "[{Level} {SourceContext}] {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();
            Log.Logger = logger;

            var name = Dns.GetHostName();
            var ipaddrs = Dns.GetHostEntry(name).AddressList
                .Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(x => x)
                .ToList();

            var ipaddr = ipaddrs.FirstOrDefault();

            if(ipaddrs.Count() > 1)
            {
                logger.Information("Found {0} Ip's", ipaddrs.Count());

                var id = 0;
                foreach(var i in ipaddrs)
                {
                    logger.Information("{0}). {1}", id++, i.ToString());
                }

                logger.Information("Select:");
                var l = Console.ReadLine();

                ipaddr = ipaddrs[int.Parse(l)];
            }

            var mh = new MessageHandler[] {
                new FilteredMessageHandler<CSSession>()
                    .AddHandler(new MainServices())
            };

            var mf = new MessageFactory[]
            {
                new MainMessageFactory()
            };

            string apiKey;
            if (args.Length > 0)
                apiKey = "api-" + args[0].Substring(0, 10);
            else
            {
                logger.Information("Please write a apiKey for use on ConnectServer auth");
                var line = Console.ReadLine();
                apiKey = "api-" + line.Substring(0, 10);
            }
            logger.Information("API Key for GameServers is {0}", apiKey);
            GameContext.ConnectionString = $"Server={args[1]};port=3306;Database={args[2]};user={args[3]};password={args[4]};Convert Zero Datetime=True;";
            logger.Information("Connection String is {0}", GameContext.ConnectionString);


            var Connip = new IPEndPoint(ipaddr, 44405);
            var Chatip = new IPEndPoint(ipaddr, 55980);
            server = new WZConnectServer(Connip, mh, mf, false);
            WZChatServer = new WZChatServer(Chatip, mh, mf, false);
            ServerManager.Initialize(apiKey);
            Clients = new ClientManager();

            var thread = new Thread(SubSytem);
            thread.Start();

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

            thread.Abort();
        }

        private static void SubSytem()
        {
            while(true)
            {
                Thread.Sleep(60000);
                var now = DateTime.Now;
                if (now.Minute != 0)
                    return;

                switch (now.Hour)
                {
                    case 0:
                    case 2:
                    case 8:
                    case 12:
                    case 18:
                    case 22:
                        Log.Information("Updating Gens Clasification");
                        using (var db = new GameContext())
                        {
                            var duprians = from c in db.Gens
                                           where c.Influence == 1
                                           orderby c.Contribution descending
                                           select c;
                            var vanerts = from c in db.Gens
                                          where c.Influence == 2
                                          orderby c.Contribution descending
                                          select c;

                            var i = 1;
                            foreach (var c in duprians)
                            {
                                if (c.Contribution >= 10000)
                                {
                                    c.Class = _classSuperior.First(x => x.Key >= i).Value;
                                    c.Ranking = i++;
                                }
                                else
                                {
                                    c.Class = _class.First(x => x.Key <= c.Contribution).Value;
                                    c.Ranking = i++;
                                }
                            }

                            db.UpdateRange(duprians);
                            db.SaveChanges();

                            i = 1;
                            foreach (var c in vanerts)
                            {
                                if (c.Contribution >= 10000)
                                {
                                    c.Class = _classSuperior.First(x => x.Key >= i).Value;
                                    c.Ranking = i++;
                                }
                                else
                                {
                                    c.Class = _class.First(x => x.Key <= c.Contribution).Value;
                                    c.Ranking = i++;
                                }
                            }

                            db.UpdateRange(vanerts);
                            db.SaveChanges();
                        }
                        break;
                }
            }
        }
    }
}
