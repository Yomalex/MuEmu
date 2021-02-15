using CSEmu.Network;
using CSEmu.Network.Services;
using Serilog;
using System;
using System.Net;
using WebZen.Handlers;
using WebZen.Network;
using System.Linq;

namespace CSEmu
{
    class Program
    {
        public static WZConnectServer server;
        public static WZChatServer WZChatServer;
        public static ClientManager Clients;
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


            var Connip = new IPEndPoint(ipaddr, 44405);
            var Chatip = new IPEndPoint(ipaddr, 55980);
            server = new WZConnectServer(Connip, mh, mf, false);
            WZChatServer = new WZChatServer(Chatip, mh, mf, false);
            ServerManager.Initialize(apiKey);
            Clients = new ClientManager();

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
