using MU.Network.Auth;
using MU.Resources;
using MuEmu.Entity;
using MuEmu.Resources;
using MuEmu.Resources.XML;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebZen.Util;

namespace MuEmu.Network
{
    internal class MapServerManager
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(MapServerManager));

        private MapServerDto mapServerDto;
        private static MapServerManager _instance;

        private MapServerManager(string file)
        {
            mapServerDto = ResourceLoader.XmlLoader<MapServerDto>(file);
        }

        private MSGameServerDto _CantGo(MSGroupDto dto, ushort PreviousGS, Maps map)
        {
            var GS = dto.GameServers
                .Where(x => x.Maps.Any(y => y.ID == map && y.MoveAbleOption) 
                || (x.Default == 1 && !x.Maps.Any(y => y.ID == map)))
                .ToList();

            var pGS = GS.FirstOrDefault(x => x.Code == PreviousGS);
            if (pGS != null)
                return pGS;

            return GS[Program.RandomProvider(GS.Count)];
        }

        private MSGameServerDto _CheckMapServerMove(GSSession session, Maps map)
        {
            var group = mapServerDto.Groups
                .Where(x => x.GameServers.Any(y => y.Code == Program.ServerCode))
                .First();

            var GS = group.GameServers.Where(y => y.Code == Program.ServerCode).First();

            var data = GS.Maps?.FirstOrDefault(x => x.ID == map)??null;

            if(data == null)
            {
                switch(GS.Default)
                {
                    case 0:
                        return _CantGo(group, session.PreviousCode, map);
                    case 1:
                        return GS;
                }
            }
            
            if(data.MoveAbleOption)
                return GS;

            return _CantGo(group, session.PreviousCode, map);            
        }

        public static void Initialize(string file)
        {
            if (_instance != null)
                throw new InvalidOperationException();

            _instance = new MapServerManager(file);
        }

        public static bool CheckMapServerMove(GSSession session, Maps map)
        {
            var NextGS = _instance._CheckMapServerMove(session, map);
            if (NextGS.Code != Program.ServerCode)
            {
                Logger.Information("Map {2} disabled, Moving to other server, [{0}]{3}->[{1}]{4}:{5}", 
                    Program.ServerCode, 
                    NextGS.Code, 
                    map,
                    Program.server.IPAddress,
                    NextGS.IP,
                    NextGS.Port);
                uint[] Auth = {
                    (uint)Program.RandomProvider(int.MaxValue),
                    (uint)Program.RandomProvider(int.MaxValue),
                    (uint)Program.RandomProvider(int.MaxValue),
                    (uint)Program.RandomProvider(int.MaxValue),
                };
                using (var db = new GameContext())
                {
                    var token = $"{Auth[0]:X8}{Auth[1]:X8}{Auth[2]:X8}{Auth[3]:X8}";
                    var acc = db.Accounts.Find(session.Player.Account.ID);
                    acc.AuthToken = token;
                    db.Accounts.Update(acc);
                    db.SaveChanges();
                }
                session.SendAsync(new SServerMove
                {
                    IpAddress = NextGS.IP.GetBytes(),
                    ServerCode = NextGS.Code,
                    ServerPort = NextGS.Port,
                    AuthCode1 = Auth[0],
                    AuthCode2 = Auth[1],
                    AuthCode3 = Auth[2],
                    AuthCode4 = Auth[3],
                }).Wait();
                //session.Disconnect();
                return false;
            }

            return true;
        }
    }
}
