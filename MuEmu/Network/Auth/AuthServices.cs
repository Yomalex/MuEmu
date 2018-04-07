using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network.Auth
{
    public class AuthServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(AuthServices));

        [MessageHandler(typeof(CIDAndPass))]
        public void CIDAndPass(GSSession session, CIDAndPass message)
        {
            BuxDecode.Decode(message.Account);
            BuxDecode.Decode(message.Password);
            var Account = Encoding.ASCII.GetString(message.Account).TrimEnd((char)0);
            var Password = Encoding.ASCII.GetString(message.Password).TrimEnd((char)0);

            Logger.Debug("ID:{account} PW:{password}", Account, Password);
        }
    }
}
