using MuEmu.Network;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public static class Extensions
    {
        public static ILogger ForAccount(this ILogger logger, int id, string user)
        {
            return logger
                .ForContext("AID", id)
                .ForContext("AUser", user);
        }

        public static ILogger ForAccount(this ILogger logger, GSSession session)
        {
            var plr = session.Player;
            var acc = plr?.Account??null;
            return logger.ForAccount(session.ID, acc?.Nickname??"");
        }

        public static T AnonymousMap<T>(T dest, object src)
        {
            foreach (var ip in src.GetType().GetProperties())
            {
                var pInfo = dest.GetType()
                    .GetProperties()
                    .Where(x => x.Name == ip.Name)
                    .FirstOrDefault();

                try
                {
                    if (pInfo != null)
                        pInfo.SetValue(dest, ip.GetValue(src));
                }catch(Exception)
                { }
            }

            return dest;
        }
    }
}
