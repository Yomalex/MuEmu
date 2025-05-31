using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace CSEmu.Network.Services
{
    public interface IMainMessage
    { }

    internal class MainMessageFactory : MessageFactory<MainOpCode, IMainMessage>
    {
        public MainMessageFactory()
        {
            // C2S
            Register<CServerList>(MainOpCode.ServerList);
            Register<CServerListS0>(MainOpCode.ServerListS0);
            Register<CServerInfo>(MainOpCode.ServerInfo);

            // GS Messages
            Register<CRegistryReq>(MainOpCode.GSJoin);
            Register<CKeepAlive>(MainOpCode.GSKeep);
            Register<SCAdd>(MainOpCode.GSClientAdd);
            Register<SCChat>(MainOpCode.GSClientChat);
            Register<SCRem>(MainOpCode.GSClientRem);

            // S2C
            Register<SConnectResult>(MainOpCode.Join);
            Register<SServerList>(MainOpCode.ServerList);
            Register<SServerListS0>(MainOpCode.ServerList);
            Register<SServerInfo>(MainOpCode.ServerInfo);
        }
    }
}
