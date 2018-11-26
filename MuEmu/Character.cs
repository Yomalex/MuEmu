using MU.DataBase;
using MuEmu.Network.Auth;
using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Util;

namespace MuEmu
{
    public class Character
    {
        private float _hp;
        private float _hpMax;
        private float _sd;
        private float _sdMax;
        private float _mp;
        private float _mpMax;
        private float _bp;
        private float _bpMax;
        private Point _position;
        private ulong _exp;

        public Player Player { get; }
        public Account Account { get; }
        public Quests Quests { get; }
        public Guild Guild { get; set; }
        public Inventory Inventory { get; }
        public Spells Spells { get; }

        // Basic Info
        public HeroClass Class { get; set; }
        public HeroClass BaseClass => (HeroClass)(((int)Class) & 0xF0);
        public CharacterInfo BaseInfo => ResourceCache.Instance.GetDefChar()[BaseClass];
        public string Name { get; set; }
        public ushort Level { get; set; }
        public float Health {
            get => _hp;
            set {
                if (_hp == value)
                    return;

                if(value > _hpMax)
                {
                    value = _hpMax;
                }

                _hp = value;

                HPorSDChanged();
            }
        }
        public float Shield
        {
            get => _sd;
            set
            {
                if (_sd == value)
                    return;

                if (value > _sdMax)
                {
                    value = _sdMax;
                }

                _sd = value;

                HPorSDChanged();
            }
        }
        public float Mana
        {
            get => _mp;
            set
            {
                if (_mp == value)
                    return;

                if (value > _mpMax)
                {
                    value = _mpMax;
                }

                _mp = value;

                MPorBPChanged();
            }
        }
        public float Stamina
        {
            get => _bp;
            set
            {
                if (_bp == value)
                    return;

                if (value > _bpMax)
                {
                    value = _bpMax;
                }

                _bp = value;

                MPorBPChanged();
            }
        }
        public ulong Money { get; set; }

        // Map
        public Maps MapID { get; set; }
        public Point Position { get => _position; set
            {
                if (_position == value)
                    return;

                Map.PositionChanged(_position, value);
                _position = value;
            }
        }
        public byte Direction { get; set; }
        public byte Action { get; set; }

        public MapInfo Map => ResourceCache.Instance.GetMaps()[MapID];

        // Experience
        public ulong Experience { get => _exp;
            set
            {
                if (value == _exp)
                    return;

                if (value >= NextExperience)
                    OnLevelUp();

                _exp = value;
            }
        }

        public ulong NextExperience => (((Level + 9ul) * Level) * Level) * 10ul + ((Level > 255)? ((((ulong)(Level-255) + 9ul) * (Level - 255ul)) * (Level - 255ul)) * 1000ul : 0ul);

        // Points
        public ushort LevelUpPoints { get; set; }
        public ushort Str { get; set; }
        public ushort Agility { get; set; }
        public ushort Vitality { get; set; }
        public ushort Energy { get; set; }
        public ushort Command { get; set; }
        public int TotalPoints => Str + Agility + Vitality + Energy + Command;

        public short AddPoints => 0;
        public short MaxAddPoints => 100;
        public short MinusPoints => 0;
        public short MaxMinusPoints => 100;

        public byte ClientClass => GetClientClass(Class);

        public static byte GetClientClass(HeroClass dbClass)
        {
            int @class = (int)dbClass;
            return (byte)(((@class & 0x70) << 1) | ((@class & 0x03) == 1 ? 0x10 : (((@class & 0x03) == 2) ? 0x18 : 0x00)));
        }

        public Character(Player plr, CharacterDto characterDto)
        {
            Player = plr;
            Account = Player.Account;
            Name = characterDto.Name;
            Class = (HeroClass)characterDto.Class;
            Level = characterDto.Level;
            Inventory = new Inventory(this, characterDto);
            Quests = new Quests(this, characterDto);
            Spells = new Spells(this, characterDto);

            MapID = (Maps)characterDto.Map;
            Position = new Point(characterDto.X, characterDto.Y);
            Map.AddPlayer(this);

            Experience = (ulong)characterDto.Experience;
            Str = characterDto.Str;
            Agility = characterDto.Agility;
            Vitality = characterDto.Vitality;
            Energy = characterDto.Energy;
            Command = characterDto.Command;

            CalcStats();

            Shield = _sdMax;
            Health = characterDto.Life;
            Stamina = _bpMax / 2;
            Mana = characterDto.Mana;

            plr.Session.SendAsync(new SCharacterMapJoin2
            {
                Map = MapID,
                LevelUpPoints = LevelUpPoints,
                Str = Str,
                Agi = Agility,
                Vit = Vitality,
                Ene = Energy,
                Cmd = Command,
                Direccion = Direction,
                Experience = Experience.ShufleEnding(),
                NextExperience = NextExperience.ShufleEnding(),
                Position = Position,
                Life = (ushort)Health,
                MaxLife = (ushort)_hpMax,
                Mana = (ushort)Mana,
                MaxMana = (ushort)_mpMax,
                Shield = (ushort)Shield,
                MaxShield = (ushort)_sdMax,
                Stamina = (ushort)Stamina,
                MaxStamina = (ushort)_bpMax,
                Zen = Money.ShufleEnding(),
                PKLevel = 3,
                AddPoints = AddPoints,
                MaxAddPoints = MaxAddPoints,
                MinusPoints = MinusPoints,
                MaxMinusPoints = MaxMinusPoints,
            });

            Inventory.SendInventory();

            Quests.SendList();

            Spells.SendList();
        }

        private void HPorSDChanged()
        {
            Player.Session.SendAsync(new SHeatlUpdate(RefillInfo.Unk2, (ushort)_hp, (ushort)_sd, false));
        }

        private void MPorBPChanged()
        {
            Player.Session.SendAsync(new SManaUpdate(RefillInfo.Unk2, (ushort)_hp, (ushort)_sd));
        }

        private void OnLevelUp()
        {
            if (Level >= 400)
                return;

            Level++;

            var att = BaseInfo.Attributes;

            _hpMax = (att.Life + att.LevelLife * (Level - 1));
            _mpMax = (att.Mana + att.LevelMana * (Level - 1));

            Player.Session.SendAsync(new SLevelUp
            {
                Level = Level,
                LevelUpPoints = LevelUpPoints,
                MaxLife = (ushort)_hpMax,
                MaxMana = (ushort)_hpMax,
                MaxShield = (ushort)_sdMax,
                MaxBP = (ushort)_bpMax,
                AddPoint = (ushort)AddPoints,
                MaxAddPoint = (ushort)MaxAddPoints,
                MinusPoint = (ushort)MinusPoints,
                MaxMinusPoint = (ushort)MaxMinusPoints,
            });
        }

        private void CalcStats()
        {
            var att = BaseInfo.Attributes;
            _hpMax = (att.Life + att.LevelLife * (Level - 1));
            _mpMax = (att.Mana + att.LevelMana * (Level - 1));
            _bpMax = (att.StrToBP * Str) + (att.AgiToBP * Agility) + (att.VitToBP * Vitality) + (att.EneToBP * Energy);
            _sdMax = TotalPoints * 3 + (Level * Level) / 30/* + Defense*/;
        }
    }
}
