using MU.Network;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MuEmu.Network.ConnectServer
{
    public interface ICSMessage
    { }

    public class CSMessageFactory : MessageFactory<ConOpCode, ICSMessage>
    {
        public CSMessageFactory()
        {
            Register<CWelcome>(ConOpCode.CSWelcome);
            Register<CRegistryReq>(ConOpCode.GSJoin);
            Register<CKeepAlive>(ConOpCode.GSKeep);
            Register<SCAdd>(ConOpCode.GSClientAdd);
            Register<SCChat>(ConOpCode.GSClientChat);
            Register<SCRem>(ConOpCode.GSClientRem);
        }
    }
}
