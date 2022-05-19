using MU.Network.Game;
using MuEmu.Network;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Util
{
    public static class SessionExtensions
    {
        public static async void Exception(this GSSession session, Exception exception, string messageTemplate = "")
        {
            await session.SendAsync(new SNotice(MU.Resources.NoticeType.Blue, exception.Message));
            Log.Logger.ForAccount(session).Error(exception, messageTemplate);
        }
    }
}
