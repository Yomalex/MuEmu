using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Resources.Game
{
    public class CharacterInfo
    {
        public HeroClass Class { get; set; }
        public Maps Map { get; set; }
        public ushort Level { get; set; }
        public StatsInfo Stats { get; set; }
        public AttriInfo Attributes { get; set; }
        public Dictionary<ushort, Item> Equipament { get; set; }
        public Spell[] Spells { get; set; }
    }

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
