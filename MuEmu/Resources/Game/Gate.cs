using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Resources.Game
{
    public class Gate
    {
        public int Number { get; set; }

        public GateType GateType { get; set; }

        public Maps Map { get; set; }


        public Rectangle Door { get; set; }
        //public byte X1 { get; set; }

        //public byte Y1 { get; set; }

        //public byte X2 { get; set; }

        //public byte Y2 { get; set; }

        public int Target { get; set; }

        public byte Dir { get; set; }

        public ushort ReqLevel { get; set; }

        public ushort ReqZen { get; set; }
    }
}
