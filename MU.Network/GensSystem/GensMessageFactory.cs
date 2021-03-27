using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Network;

namespace MU.Network.GensSystem
{
    public interface IGensMessage { }
    public class GensMessageFactory : MessageFactory<GensOpCode, IGensMessage>
    {
        public GensMessageFactory()
        {
            Register<CRequestJoin>(GensOpCode.RequestJoin);
            Register<SRequestJoin>(GensOpCode.RequestJoin);
            Register<SGensSendInfoS9>(GensOpCode.SendGensInfo);
            Register<SViewPortGens>(GensOpCode.ViewPortGens);
            Register<SRegMember>(GensOpCode.RegMember);
        }
    }
}
