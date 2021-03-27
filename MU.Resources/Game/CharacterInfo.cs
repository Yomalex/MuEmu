using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Resources.Game
{
    public class StatsInfo
    {
        public int Str { get; set; }
        public int Agi { get; set; }
        public int Vit { get; set; }
        public int Ene { get; set; }
        public int Cmd { get; set; }
    }

    public class AttriInfo
    {
        public float Life { get; set; }
        public float Mana { get; set; }
        public float LevelLife { get; set; }
        public float LevelMana { get; set; }
        public float VitalityToLife { get; set; }
        public float EnergyToMana { get; set; }

        public float StrToBP { get; set; }
        public float AgiToBP { get; set; }
        public float VitToBP { get; set; }
        public float EneToBP { get; set; }
        public float CmdToBP { get; set; }
    }
}
