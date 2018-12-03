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
            return logger.ForAccount(session.ID, session.Player?.Account.Nickname??"");
        }

        public static void AnonymousMap(object dest, object src)
        {
            foreach (var ip in src.GetType().GetProperties())
            {
                var pInfo = dest.GetType()
                    .GetProperties()
                    .Where(x => x.Name == ip.Name)
                    .FirstOrDefault();

                if (pInfo != null)
                    pInfo.SetValue(dest, ip.GetValue(src));
            }
        }
    }
}
