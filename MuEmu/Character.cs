using MU.DataBase;
using MuEmu.Data;
using MuEmu.Entity;
using MuEmu.Monsters;
using MuEmu.Network.Auth;
using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Util;

namespace MuEmu
{
    public class Character
    {
        private int _characterId;
        private float _hp;
        private float _hpMax;
        private float _hpAdd;
        private float _sd;
        private float _sdMax;
        private float _sdAdd;
        private float _mp;
        private float _mpMax;
        private float _mpAdd;
        private float _bp;
        private float _bpMax;
        private float _bpAdd;
        private Point _position;
        private ulong _exp;
        private ushort _str;
        private ushort _strAdd;
        private ushort _agi;
        private ushort _agiAdd;
        private ushort _vit;
        private ushort _vitAdd;
        private ushort _ene;
        private ushort _eneAdd;
        private ushort _cmd;
        private ushort _cmdAdd;
        private uint _zen;
        private Maps _map;
        private bool _needSave;
        private HeroClass _class;
        private ushort _level;
        private byte _pkLevel;
        private ushort _levelUpPoints;
        private readonly Random _rand = new Random();

        public int Id => _characterId;
        public Player Player { get; }
        public Account Account => Player.Account;
        public Quests Quests { get; }
        public Guild Guild { get; set; }
        public Inventory Inventory { get; }
        public Spells Spells { get; }
        public bool Change { get; set; }

        public List<ushort> MonstersVP { get; set; }
        public List<Player> PlayersVP { get; set; }
        public List<ushort> ItemsVP { get; set; }

        // Basic Info
        public HeroClass Class { get => _class; set { _class = value; _needSave = true; BaseInfo = ResourceCache.Instance.GetDefChar()[BaseClass]; } }
        public HeroClass BaseClass => (HeroClass)(((int)Class) & 0xF0);
        public bool Changeup {
            get => (((byte)Class) & 0x0F) == 1;
            set
            {
                Class &= (HeroClass)(0xF0);
                Class |= (HeroClass)(value ? 1 : 0);
            }
        }
        public bool MasterClass
        {
            get => (((byte)Class) & 0x0F) > 1;
            set
            {
                Class &= (HeroClass)(0xF0);
                Class |= (HeroClass)(value ? 2 : 0);
            }
        }
        public CharacterInfo BaseInfo { get; private set; }
        public string Name { get; set; }
        public ushort Level { get => _level; set { _level = value; _needSave = true; } }
        public float Health {
            get => _hp;
            set {
                if (value > _hpMax)
                {
                    value = _hpMax;
                }

                if (_hp == value)
                    return;

                _hp = value;

                if (_hp <= 0)
                {
                    _hp = 0;
                    OnDead();
                }

                HPorSDChanged();
            }
        }
        public float MaxHealth
        {
            get => _hpMax;
            set
            {
                if (value == _hpMax + _hpAdd)
                    return;

                _hpAdd = value - _hpMax;

                HPorSDMaxChanged();
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
        public float MaxShield
        {
            get => _sdMax + _sdAdd;
            set
            {
                if (value == _sdMax + _sdAdd)
                    return;

                _sdAdd = value - _hpMax;

                HPorSDMaxChanged();
            }
        }
        public float Mana
        {
            get => _mp;
            set
            {
                if (value > _mpMax)
                {
                    value = _mpMax;
                }

                if (_mp == value)
                    return;

                _mp = value;

                MPorBPChanged();
            }
        }
        public float MaxMana
        {
            get => _mpMax + _mpAdd;
            set
            {
                if (value == _mpMax + _mpAdd)
                    return;

                _mpAdd = value - _mpMax;

                MPorBPMaxChanged();
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
        public float MaxStamina
        {
            get => _bpMax + _bpAdd;
            set
            {
                if (value == _bpMax + _bpAdd)
                    return;

                _bpAdd = value - _bpMax;

                MPorBPMaxChanged();
            }
        }
        public uint Money
        {
            get => _zen;
            set
            {
                if (value == _zen)
                    return;

                _zen = value;
                _needSave = true;
                OnMoneyChange();
            }
        }
        public byte PKLevel { get => _pkLevel; set { _pkLevel = value; _needSave = true; } }

        // Map
        public Maps MapID {
            get => _map;
            set {
                if (_map == value)
                    return;

                _map = value;
                Map?.DelPlayer(this);
                MapChanged?.Invoke(this, new EventArgs());
                Map = ResourceCache.Instance.GetMaps()[_map];
                Map.AddPlayer(this);
                _needSave = true;
            }
        }
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

        public MapInfo Map { get; private set; }

        public event EventHandler MapChanged;

        // Experience
        public ulong Experience { get => _exp;
            set
            {
                if (value == _exp)
                    return;

                _exp = value;

                if (_exp >= NextExperience)
                    OnLevelUp();

                if (_exp < BaseExperience)
                    _exp = BaseExperience;

                _needSave = true;
            }
        }
        public ulong BaseExperience => GetExperienceFromLevel((ushort)(Level - 1));
        public ulong NextExperience => GetExperienceFromLevel(Level);

        // Points
        public ushort LevelUpPoints { get => _levelUpPoints;
            set
            {
                if (_levelUpPoints == value)
                    return;

                _levelUpPoints = value;
                _needSave = true;
            }
        }

        public ushort Strength
        {
            get => _str;
            set
            {
                if (value == _str)
                    return;

                _str = value;
                CalcStats();
            }
        }
        public ushort StrengthAdd
        {
            get => _strAdd;
            set
            {
                if (value == _strAdd)
                    return;

                _strAdd = value;
                CalcStats();
            }
        }
        public ushort StrengthTotal => (ushort)(_str + _strAdd);

        public ushort Agility
        {
            get => _agi;
            set
            {
                if (value == _agi)
                    return;

                _agi = value;
                CalcStats();
            }
        }
        public ushort AgilityAdd
        {
            get => (_agiAdd);
            set
            {
                if (value == _agiAdd)
                    return;

                _agiAdd = value;
                CalcStats();
            }
        }
        public ushort AgilityTotal => (ushort)(_agi + _agiAdd);

        public ushort Vitality
        {
            get => _vit;
            set
            {
                if (value == _vit)
                    return;

                _vit = value;
                CalcStats();
            }
        }
        public ushort VitalityAdd
        {
            get => _vitAdd;
            set
            {
                if (value == _vitAdd)
                    return;

                _vitAdd = value;
                CalcStats();
            }
        }
        public ushort VitalityTotal => (ushort)(_vit + _vitAdd);


        public ushort Energy
        {
            get => _ene;
            set
            {
                if (value == _ene)
                    return;

                _ene = value;
                CalcStats();
            }
        }
        public ushort EnergyAdd
        {
            get => _eneAdd;
            set
            {
                if (value == _eneAdd)
                    return;

                _eneAdd = value;
                CalcStats();
            }
        }
        public ushort EnergyTotal => (ushort)(_ene + _eneAdd);

        public ushort Command
        {
            get => _cmd;
            set
            {
                if (value == _cmd)
                    return;

                _cmd = value;
                CalcStats();
            }
        }
        public ushort CommandAdd
        {
            get => _cmdAdd;
            set
            {
                if (value == _cmdAdd)
                    return;

                _cmdAdd = value;
                CalcStats();
            }
        }
        public ushort CommandTotal => (ushort)(_cmd + _cmdAdd);

        public int TotalPoints => _str + _agi + _vit + _ene + _cmd + LevelUpPoints;

        public short AddPoints => (short)(TotalPoints - (BaseInfo.Stats.Str + BaseInfo.Stats.Agi + BaseInfo.Stats.Vit + BaseInfo.Stats.Ene + BaseInfo.Stats.Cmd + (Level - 1) * 5));
        public short MaxAddPoints => 100;
        public short MinusPoints => 0;
        public short MaxMinusPoints => 100;

        private float _leftAttackMin = 0.0f;
        private float _leftAttackMax = 0.0f;
        private float _rightAttackMin = 0.0f;
        private float _rightAttackMax = 0.0f;
        
        // Battle
        //public int Attack => Inventory.Get((byte)Equipament.LeftRing).BasicInfo.
        //public int Defense => Inventory.Player

        public byte ClientClass => GetClientClass(Class);

        public static byte GetClientClass(HeroClass dbClass)
        {
            int @class = (int)dbClass;
            return (byte)(((@class & 0x70) << 1) | ((@class & 0x03) == 1 ? 0x10 : (((@class & 0x03) == 2) ? 0x18 : 0x00)));
        }

        public Character(Player plr, CharacterDto characterDto)
        {
            _characterId = characterDto.CharacterId;
            Player = plr;
            Name = characterDto.Name;
            Class = (HeroClass)characterDto.Class;
            Level = characterDto.Level;
            Inventory = new Inventory(this, characterDto);
            Quests = new Quests(this, characterDto);
            Spells = new Spells(this, characterDto);
            MonstersVP = new List<ushort>();
            ItemsVP = new List<ushort>();
            PlayersVP = new List<Player>();

            MapID = (Maps)characterDto.Map;
            Map = ResourceCache.Instance.GetMaps()[_map];
            Map.AddPlayer(this);
            _position = new Point(characterDto.X, characterDto.Y);

            Experience = (ulong)characterDto.Experience;
            _str = characterDto.Str;
            _agi = characterDto.Agility;
            _vit = characterDto.Vitality;
            _ene = characterDto.Energy;
            _cmd = characterDto.Command;
            _levelUpPoints = characterDto.LevelUpPoints;

            CalcStats();

            Shield = _sdMax;
            Health = characterDto.Life;
            Stamina = _bpMax / 2;
            Mana = characterDto.Mana;
            _zen = characterDto.Money;

            var StatsInfo = new SCharacterMapJoin2
            {
                Map = MapID,
                LevelUpPoints = LevelUpPoints,
                Str = StrengthTotal,
                Agi = AgilityTotal,
                Vit = VitalityTotal,
                Ene = EnergyTotal,
                Cmd = CommandTotal,
                Direccion = Direction,
                Experience = Experience.ShufleEnding(),
                NextExperience = NextExperience.ShufleEnding(),
                Position = Position,
                Life = (ushort)Health,
                MaxLife = (ushort)_hpMax,
                Mana = (ushort)Mana,
                MaxMana = (ushort)_mpMax,
                Shield = (ushort)Shield,
                MaxShield = (ushort)MaxShield,
                Stamina = (ushort)Stamina,
                MaxStamina = (ushort)_bpMax,
                Zen = Money,
                PKLevel = 3,
                AddPoints = AddPoints,
                MaxAddPoints = MaxAddPoints,
                MinusPoints = MinusPoints,
                MaxMinusPoints = MaxMinusPoints,
            };

            plr.Session.SendAsync(StatsInfo).Wait();

            Inventory.SendInventory();

            Quests.SendList();

            Spells.SendList();
        }

        public async Task SendV2Message(object message)
        {
            foreach (var plr in PlayersVP)
                await plr.Session.SendAsync(message);
        }

        private async void HPorSDChanged()
        {
            Console.WriteLine($"HP Changed {_hp} {_sd}");
            await Player.Session.SendAsync(new SHeatlUpdate(RefillInfo.Update, (ushort)_hp, (ushort)_sd, false));
        }
        private async void HPorSDMaxChanged()
        {
            await Player.Session.SendAsync(new SHeatlUpdate(RefillInfo.MaxChanged, (ushort)MaxHealth, (ushort)MaxShield, false));
        }
        private async void MPorBPChanged()
        {
            await Player.Session.SendAsync(new SManaUpdate(RefillInfo.Update, (ushort)_mp, (ushort)_bp));
        }
        private async void MPorBPMaxChanged()
        {
            await Player.Session.SendAsync(new SManaUpdate(RefillInfo.MaxChanged, (ushort)MaxMana, (ushort)MaxStamina));
        }
        private async void OnLevelUp()
        {
            if (Level >= 400)
                return;

            var curLevel = Level;

            do
            {
                Level++;
            } while (_exp >= NextExperience);

            var att = BaseInfo.Attributes;

            _hpMax = (att.Life + att.LevelLife * (Level - 1));
            _mpMax = (att.Mana + att.LevelMana * (Level - 1));

            var levelPoint = BaseClass == HeroClass.MagicGladiator || BaseClass == HeroClass.DarkLord ? 7 : 5;
            levelPoint += MasterClass ? 1 : 0;
            LevelUpPoints += (ushort)(levelPoint * (Level - curLevel));

            await Player.Session.SendAsync(new SLevelUp
            {
                Level = Level,
                LevelUpPoints = LevelUpPoints,
                MaxLife = (ushort)MaxHealth,
                MaxMana = (ushort)MaxMana,
                MaxShield = (ushort)MaxShield,
                MaxBP = (ushort)MaxStamina,
                AddPoint = (ushort)AddPoints,
                MaxAddPoint = (ushort)MaxAddPoints,
                MinusPoint = (ushort)MinusPoints,
                MaxMinusPoint = (ushort)MaxMinusPoints,
            });

            await Player.Session.SendAsync(new SEffect((ushort)Player.Session.ID, ClientEffect.LevelUp));
        }
        private void CalcStats()
        {
            var att = BaseInfo.Attributes;
            _hpMax = (att.Life + att.LevelLife * (Level - 1));
            _mpMax = (att.Mana + att.LevelMana * (Level - 1));
            _bpMax = (att.StrToBP * StrengthTotal) + (att.AgiToBP * AgilityTotal) + (att.VitToBP * VitalityTotal) + (att.EneToBP * EnergyTotal);
            _sdMax = TotalPoints * 3 + (Level * Level) / 30/* + Defense*/;
            ObjCalc();
        }
        private void ObjCalc()
        {

            var right = Inventory.Get(Equipament.RightHand);
            var left = Inventory.Get(Equipament.LeftHand);

            switch(BaseClass)
            {
                //case HeroClass.DarkWizard:
                //    break;
                case HeroClass.DarkKnight:
                    _leftAttackMin = (StrengthTotal / 6);
                    _leftAttackMax = (StrengthTotal / 4);
                    _rightAttackMin = (StrengthTotal / 6);
                    _rightAttackMax = (StrengthTotal / 4);
                    break;
                case HeroClass.FaryElf:
                    if ((right?.Number.Type ?? ItemType.Invalid) == ItemType.BowOrCrossbow || (left?.Number.Type ?? ItemType.Invalid) == ItemType.BowOrCrossbow)
                    {
                        _leftAttackMin = AgilityTotal / 8;
                        _leftAttackMax = AgilityTotal / 4;
                        _rightAttackMin = AgilityTotal / 8;
                        _rightAttackMax = AgilityTotal / 4;
                    }
                    else
                    {
                        _leftAttackMin = (StrengthTotal + AgilityTotal) / 7;
                        _leftAttackMax = (StrengthTotal + AgilityTotal) / 4;
                        _rightAttackMin = (StrengthTotal + AgilityTotal) / 7;
                        _rightAttackMax = (StrengthTotal + AgilityTotal) / 4;
                    }
                    break;
                case HeroClass.MagicGladiator:
                    _leftAttackMin = (StrengthTotal / 6) + (EnergyTotal / 12);
                    _leftAttackMax = (StrengthTotal / 4) + (EnergyTotal / 8);
                    _rightAttackMin = (StrengthTotal / 6) + (EnergyTotal / 12);
                    _rightAttackMax = (StrengthTotal / 4) + (EnergyTotal / 8);
                    break;
                case HeroClass.DarkLord:
                    _leftAttackMin = (StrengthTotal / 7) + (EnergyTotal / 14);
                    _leftAttackMax = (StrengthTotal / 5) + (EnergyTotal / 10);
                    _rightAttackMin = (StrengthTotal / 7) + (EnergyTotal / 14);
                    _rightAttackMax = (StrengthTotal / 5) + (EnergyTotal / 10);
                    break;
                //case HeroClass.Summoner:
                //    break;
                //case HeroClass.RageFighter:
                //    break;
                //case HeroClass.GrowLancer:
                //    break;
                default:
                    _leftAttackMin = (StrengthTotal / 8);
                    _leftAttackMax = (StrengthTotal / 4);
                    _rightAttackMin = (StrengthTotal / 8);
                    _rightAttackMax = (StrengthTotal / 4);
                    break;
            }

            

            if (right?.Attack ?? false)
            {
                _rightAttackMin += right.AttackMin;
                _rightAttackMax += right.AttackMax;
            }

            if (left?.Attack ?? false)
            {
                _leftAttackMin += left.AttackMin;
                _leftAttackMax += left.AttackMax;
            }

            if (BaseClass == HeroClass.BladeKnight || BaseClass == HeroClass.MagicGladiator)
            {
                if ((right?.Number ?? ItemNumber.Invalid) == (left?.Number ?? ItemNumber.Invalid) && (right?.Number ?? ItemNumber.Invalid) != ItemNumber.Invalid)
                {
                    _rightAttackMin *= 0.55f;
                    _rightAttackMax *= 0.55f;
                    _leftAttackMin *= 0.55f;
                    _leftAttackMax *= 0.55f;
                }
            }
        }
        private async void OnMoneyChange()
        {
            await Player.Session.SendAsync(new SItemGet { Result = 0xFE, Money = Money });
        }
        private ulong GetExperienceFromLevel(ushort level)
        {
            return (((level + 9ul) * level) * level) * 10ul + ((level > 255) ? ((((ulong)(level - 255) + 9ul) * (level - 255ul)) * (level - 255ul)) * 1000ul : 0ul);
        }
        private void OnDead()
        {

        }

        public async Task WarpTo(Maps map, Point position, byte dir)
        {
            Map.DelPlayer(this);
            MapID = map;
            Map.AddPlayer(this);
            Position = position;
            Direction = dir;
            await Player.Session.SendAsync(new STeleport(256, MapID, Position, Direction));
        }

        public async Task WarpTo(int gate)
        {
            var g = ResourceCache.Instance.GetGates()[gate];

            var rand = new Random();

            var randX = rand.Next(g.Door.Left, g.Door.Right);
            var randY = rand.Next(g.Door.Top, g.Door.Bottom);

            await WarpTo(g.Map, new Point(randX, randY), g.Dir);
        }

        public async void TeleportTo(Point position)
        {
            Position = position;
            await Player.Session.SendAsync(new STeleport(0, MapID, Position, Direction));
        }

        public async Task Save(GameContext db)
        {
            await Inventory.Save(db);
            await Spells.Save(db);

            if (_needSave == false)
                return;

            var charDto = db.Characters.First(x => x.CharacterId == _characterId);
            charDto.Class = (byte)_class;
            charDto.Level = _level;
            charDto.LevelUpPoints = _levelUpPoints;
            charDto.Map = (byte)_map;
            charDto.X = (byte)_position.X;
            charDto.Y = (byte)_position.Y;
            charDto.Experience = (long)_exp;
            charDto.Life = (ushort)_hp;
            charDto.MaxLife = (ushort)_hpMax;
            charDto.Mana = (ushort)_mp;
            charDto.MaxMana = (ushort)_mpMax;
            charDto.Str = _str;
            charDto.Agility = _agi;
            charDto.Vitality = _vit;
            charDto.Energy = _ene;
            charDto.Command = _cmd;
            charDto.Money = _zen;
            db.Characters.Update(charDto);

            _needSave = false;
        }

        public int Attack(Character target)
        {
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var rightHand = Inventory.Get(Equipament.RightHand);
            var wing = Inventory.Get(Equipament.Wings);
            var twing = target.Inventory.Get(Equipament.Wings);
            var criticalRate = Inventory.CriticalRate;

            var rand = new Random();

            var attack = 0.0f;

            if (leftHand?.Attack ?? false)
                attack += rand.Next(leftHand.AttackMin, leftHand.AttackMax);

            if (rightHand?.Attack ?? false)
                attack += rand.Next(rightHand.AttackMin, rightHand.AttackMax);

            attack -= target.Inventory.Defense;

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if(Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }                
            }

            if (twing != null) // Wings decrease Dmg 12%+(Level*2)%
                attack *= 0.88f - twing.Plus * 0.02f;

            return (int)attack;
        }

        public int Attack(Monster target, out DamageType type)
        {
            type = DamageType.Regular;
            var wing = Inventory.Get(Equipament.Wings);
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate;

            var attack = 0.0f;

            attack = BaseAttack();

            attack += Spells.BuffList.Sum(x => x.AttackAdd);
            attack -= target.Defense;

            if(excellentRate > _rand.Next(100))
            {
                type = DamageType.Excellent;
                attack *= 2.0f;
            }
            else if(criticalRate > _rand.Next(100))
            {
                type = DamageType.Critical;
                attack *= 2.0f;
            }

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }
            }

            if (attack < 0)
                attack = 0;

            return (int)attack;
        }

        public int SkillAttack(SpellInfo spell, Monster target, out DamageType type)
        {
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate;
            var wing = Inventory.Get(Equipament.Wings);
            var attack = BaseAttack();
            attack += _rand.Next(spell.Damage.X, spell.Damage.Y);
            attack -= target.Defense;

            type = DamageType.Regular;

            if (excellentRate > _rand.Next(100))
            {
                type = DamageType.Excellent;
                attack *= 2.0f;
            }
            else if (criticalRate > _rand.Next(100))
            {
                type = DamageType.Critical;
                attack *= 2.0f;
            }

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }
            }

            attack *= (200.0f + EnergyTotal / 10.0f) / 100.0f;

            return (int)attack;
        }
        
        public int MagicAttack(SpellInfo spell, Monster target, out DamageType type)
        {
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate;
            var wing = Inventory.Get(Equipament.Wings);
            //var leftHand = Inventory.Get(Equipament.LeftHand);
            var rightHand = Inventory.Get(Equipament.RightHand);

            var magicAdd = 0;

            if(rightHand != null)
                magicAdd = rightHand.BasicInfo.MagicPower / 2 + rightHand.Plus * 2;

            var attack = 0.0f;
            attack += _rand.Next(spell.Damage.X + Energy / 9, spell.Damage.Y + Energy / 4);
            attack *= 1.0f + magicAdd / 100.0f;
            attack -= target.Defense;

            type = DamageType.Regular;

            if (excellentRate > _rand.Next(100))
            {
                type = DamageType.Excellent;
                attack *= 2.0f;
            }
            else if (criticalRate > _rand.Next(100))
            {
                type = DamageType.Critical;
                attack *= 2.0f;
            }

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }
            }

            if (attack < 0)
                attack = 0.0f;

            return (int)attack;
        }

        public float BaseAttack()
        {
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var attack = 0.0f;
            var rand = new Random();

            attack = rand.Next((int)_rightAttackMin, (int)_rightAttackMax);

            if (leftHand?.Attack ?? false)
                attack += rand.Next((int)_leftAttackMin, (int)_leftAttackMax);

            return attack;
        }
    }
}
