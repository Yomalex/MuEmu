using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network.ConnectServer
{
    public class CSServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CSServices));
    }
}
