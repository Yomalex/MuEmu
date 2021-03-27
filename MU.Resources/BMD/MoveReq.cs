using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Serialization;
using WebZen.Util;

namespace MuEmu.Resources.BMD
{
    [WZContract]
    public class MoveReqBMD
    {
        [WZMember(0)]
        public int MoveNumber { get; set; }

        [WZMember(1, 32)]
        public byte[] btServerName { get; set; }

        [WZMember(2, 32)]
        public byte[] btClientName { get; set; }

        [WZMember(3)]
        public int Level { get; set; }

        [WZMember(4)]
        public int Zen { get; set; }

        [WZMember(5)]
        public int Gate { get; set; }

        public string ServerName { get => btServerName.MakeString(); set => btServerName = value.GetBytes(); }
        public string ClientName { get => btClientName.MakeString(); set => btClientName = value.GetBytes(); }
    }
}
