using CSEmu.Network;
using CSEmu.Network.Services;
using Serilog;
using System;
using System.Net;
using WebZen.Handlers;
using WebZen.Network;
using System.Net;
using System.Linq;

namespace CSEmu
{
    class Program
    {
        public static WZConnectServer server;
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
            var ipaddr = Dns.GetHostEntry(name).AddressList
                .Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(x => x)
                .FirstOrDefault();

            var ip = new IPEndPoint(ipaddr, 44405);

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

            server = new WZConnectServer(ip, mh, mf, false);
            ServerManager.Initialize(apiKey);

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
