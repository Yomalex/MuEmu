using CSEmu.Network;
using CSEmu.Network.Services;
using Serilog;
using System;
using System.Net;
using WebZen.Handlers;
using WebZen.Network;

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

            if(args.Length != 1)
            {
                Log.Logger.Error("Se requiere la IP como argumento\nPresione cualquier tecla para continuar");
                var input = Console.ReadLine();
                return;
            }

            var ip = new IPEndPoint(IPAddress.Parse(args[0]), 44405);

            var mh = new MessageHandler[] {
                new FilteredMessageHandler<CSSession>()
                    .AddHandler(new MainServices())
            };

            var mf = new MessageFactory[]
            {
                new MainMessageFactory()
            };

            server = new WZConnectServer(ip, mh, mf);

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
