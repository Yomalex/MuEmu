using BlubLib.Serialization;
using MuEmu.Network;
using MuEmu.Network.Auth;
using MuEmu.Network.Global;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Net;
using WebZen.Handlers;
using WebZen.Network;
using WebZen.Serialization;

namespace MuEmu
{
    class Program
    {
        public static WZGameServer server;
        static void Main(string[] args)
        {
            Predicate<GSSession> MustNotBeLoggedIn = session => session.status == LoginStatus.NotLogged;
            Predicate<GSSession> MustBeLoggedIn = session => session.status == LoginStatus.Logged;
            Predicate<GSSession> MustBePlaying = session => session.status == LoginStatus.Playing;

            Log.Logger = new LoggerConfiguration()
                .Destructure.ByTransforming<IPEndPoint>(endPoint => endPoint.ToString())
                .Destructure.ByTransforming<EndPoint>(endPoint => endPoint.ToString())
                .WriteTo.File("GameServer.txt")
                .WriteTo.Console(outputTemplate: "[{Level} {SourceContext}] {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();
            
            SimpleModulus.LoadDecryptionKey("Dec1.dat");
            SimpleModulus.LoadEncryptionKey("Enc2.dat");

            var ip = new IPEndPoint(IPAddress.Parse("192.168.10.155"), 55901);

            var mh = new MessageHandler[] {
                new FilteredMessageHandler<GSSession>()
                    .AddHandler(new AuthServices())
                    .AddHandler(new GlobalServices())
                    .RegisterRule<CIDAndPass>(MustNotBeLoggedIn)
            };

            var mf = new MessageFactory[]
            {
                new AuthMessageFactory(),
                new GlobalMessageFactory()
            };

            server = new WZGameServer(ip, mh, mf);

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
