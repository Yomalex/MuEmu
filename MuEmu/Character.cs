using MU.DataBase;
using MuEmu.Data;
using MuEmu.Entity;
using MuEmu.Monsters;
using MuEmu.Network;
using MU.Network.Auth;
using MU.Network.Game;
using MU.Network.PCPShop;
using MuEmu.Resources;
using MuEmu.Resources.Game;
using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Util;
using MU.Resources;

namespace MuEmu
{
    public class PShop
    {
        public Character Chararacter { get; set; }
        public bool Open { get; set; }
        public string Name { get; set; }

        public PShopItem[] Items => Chararacter.Inventory.PersonalShop.Items
            .Select(x => new PShopItem() { Pos = x.Key, Item = x.Value.GetBytes(), wzPrice = x.Value.PShopValue.ShufleEnding() })
            .ToArray();
        
        public PShop(Character @char)
        {
            Chararacter = @char;
        }
    }
    public class Character : IDisposable
    {
        #region Private
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
        private Maps _map = Maps.InvalidMap;
        private bool _needSave;
        private HeroClass _class;
        private ushort _level;
        private byte _pkLevel;
        private ushort _levelUpPoints;
        private readonly Random _rand = new Random();
        private float _attackRatePvM = 0.0f;
        private float _attackRatePvP = 0.0f;
        private float _leftAttackMin = 0.0f;
        private float _leftAttackMax = 0.0f;
        private float _rightAttackMin = 0.0f;
        private float _rightAttackMax = 0.0f;
        private float _magicAttackMin = 0.0f;
        private float _magicAttackMax = 0.0f;
        private float _defense = 0.0f;
        private float _defenseRatePvM = 0.0f;
        private float _defenseRatePvP = 0.0f;
        private float _attackSpeed = 0.0f;
        private ushort _killerId;
        private int _deadlyDmg;
        private DateTime _autoRecuperationTime;
        #endregion

        /// <summary>
        /// Character ID in Database
        /// </summary>
        public int Id { get; }
        public Player Player { get; private set; }
        public Account Account => Player.Account;
        public ushort Index => (ushort)Player.Session.ID;

        public ControlCode CtlCode;
        private ushort _pcPoints;

        public Quests Quests { get; private set; }
        public Guild Guild { get; set; }
        public Inventory Inventory { get; private set; }
        public Spells Spells { get; private set; }
        public bool Change { get; set; }
        public Party Party { get; set; }
        public MasterLevel MasterLevel { get; private set; }
        public Friends Friends { get; private set; }
        public Gens Gens { get; private set; }
        public MuBot MuHelper { get; private set; }

        public PShop Shop { get; private set; }
        public CashShop CashShop { get; private set; }
        public Duel Duel { get; set; }
        public Monster KalimaGate { get; set; }
        public List<ushort> MonstersVP { get; set; }
        public List<Player> PlayersVP { get; set; }
        public List<ushort> ItemsVP { get; set; }

        #region Basic Info
        // Basic Info
        public HeroClass Class { get => _class; set { _class = value; _needSave = true; BaseInfo = ResourceCache.Instance.GetDefChar()[BaseClass]; } }
        public HeroClass BaseClass => (HeroClass)(((int)Class) & 0xF0);
        public bool Changeup {
            get => (((byte)Class) & 0x0F) == 1;
            set
            {
                Class &= (HeroClass)(0xF0);
                Class |= (HeroClass)(value ? 1 : 0);
                _needSave = true;
            }
        }
        public bool MasterClass
        {
            get => (((byte)Class) & 0x0F) > 1;
            set
            {
                Class &= (HeroClass)(0xF0);
                Class |= (HeroClass)(value ? 2 : 0);
                _needSave = true;
            }
        }
        public CharacterInfo BaseInfo { get; private set; }
        public ushort Resets { get; set; }
        public GremoryCase GremoryCase { get; private set; }
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
                var arg = _hp > value ? RefillInfo.Update : RefillInfo.Drink;
                if (value <= 0)
                {
                    _hp = 0;
                    CharacterDie?.Invoke(this, new EventArgs());
                }else
                {
                    _hp = value;
                }


                HPorSDChanged(arg);
            }
        }
        public float MaxHealth
        {
            get => _hpMax + _hpAdd;
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
                if (value > _sdMax)
                {
                    value = _sdMax;
                }

                if (_sd == value)
                    return;

                var arg = _sd > value ? RefillInfo.Update : RefillInfo.Drink;
                if(value < 0)
                {
                    _sd = 0;
                }
                else
                {
                    _sd = value;
                }

                HPorSDChanged(arg);
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

                var arg = _mp > value ? RefillInfo.Update : RefillInfo.Drink;

                if(value < 0)
                {
                    _mp = 0;
                }
                else
                {
                    _mp = value;
                }

                MPorBPChanged(arg);
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
                if (value > _bpMax)
                {
                    value = _bpMax;
                }

                if (_bp == value)
                    return;
                var arg = _bp > value ? RefillInfo.Update : RefillInfo.Drink;
                if (value < 0)
                {
                    _bp = 0;
                }
                else
                {
                    _bp = value;
                }

                MPorBPChanged(arg);
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

                if (value < 0)
                {
                    _zen = 0;
                }
                else
                {
                    _zen = value;
                }
                _needSave = true;
                OnMoneyChange();
            }
        }
        public byte PKLevel { get => _pkLevel; set { _pkLevel = value; _needSave = true; } }
        #endregion

        #region MapInfo
        // Map
        public Maps MapID {
            get => _map;
            set {
                if (_map == value)
                    return;
                _map = value;
                _needSave = true;
            }
        }
        public Point Position {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                Map.PositionChanged(_position, value);
                _position = value;
                _needSave = true;
            }
        }
        public Point TPosition { get; set; }
        public byte Direction { get; set; }
        public byte Action { get; set; }

        public MapInfo Map { get; private set; }
        #endregion

        /// <summary>
        /// On Map Change trigger this event with sender as Character
        /// </summary>
        public event EventHandler MapChanged;

        /// <summary>
        /// On player die trigger this event with sender as Character
        /// </summary>
        public event EventHandler CharacterDie;

        /// <summary>
        /// On player regen trigger this event with sender as Character
        /// </summary>
        public event EventHandler CharacterRegen;

        // Experience
        public ulong Experience { get => _exp;
            set
            {
                if (value == _exp)
                    return;

                MasterLevel.GetExperience(value - _exp);

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

        #region Stats
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

        public ushort PCPoints
        {
            get => _pcPoints;
            set
            {
                _pcPoints = value;
                Player.Session.SendAsync(new SPCPShopPoints(value, 25000)).Wait();
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
                _needSave = true;
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
                _needSave = true;
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
                _needSave = true;
                CalcStats();
            }
        }

        internal static void Reset(object session, CommandEventArgs e)
        {
            var Session = session as GSSession;
            var @char = Session.Player.Character;

            if(@char.Level < 400)
            {
                Session.SendAsync(new SNotice(NoticeType.Blue, "Necesitas nivel 400")).Wait();
                return;
            }

            if (@char.Money < 1000000)
            {
                Session.SendAsync(new SNotice(NoticeType.Blue, "Necesitas 1000000zen")).Wait();
                return;
            }

            
            var cinfo = @char.BaseInfo.Stats;

            @char.Level = 1;
            @char.Experience = 0;
            @char.Money -= 1000000u;
            //@char.Strength = (ushort)cinfo.Str;
            //@char.Agility = (ushort)cinfo.Agi;
            //@char.Vitality = (ushort)cinfo.Vit;
            //@char.Energy = (ushort)cinfo.Ene;
            //@char.Command = (ushort)cinfo.Cmd;
            @char.MapID = @char.BaseInfo.Map;
            @char.Resets++;
            //@char.LevelUpPoints = (ushort)Math.Min(@char.Resets*250, 65535);
            GameServices.CClinetClose(Session, new CClientClose { Type = ClientCloseType.ServerList }).Wait();
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
                _needSave = true;
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
                _needSave = true;
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
        #endregion

        public ushort AttackRatePvM => (ushort)_attackRatePvM;
        public ushort AttackRatePvP => (ushort)_attackRatePvP;

        public ushort Defense => (ushort)(_defense + Spells.BuffList.Sum(x => x.DefenseAdd));
        public ushort DefenseRatePvM => (ushort)(_defenseRatePvM + Spells.BuffList.Sum(x => x.DefenseAddRate)*100.0f);
        public ushort DefenseRatePvP => (ushort)(_defenseRatePvP + Spells.BuffList.Sum(x => x.DefenseAddRate)*100.0f);

        public ObjectState State { get; set; }
        public DateTimeOffset RegenTime { get; private set; }
        public byte ClientClass => GetClientClass(Class);

        public static byte GetClientClass(HeroClass dbClass)
        {
            var @class = (int)dbClass;
            var changeUp = @class & 0x03;

            var result = @class & 0xF0;
            result |= (changeUp == 1) ? 0x08 : 0x00;
            result |= (changeUp == 2) ? 0x0C : 0x00;
            result <<= Program.Season == 12 ? 0 : 1;
            result &= 0xFF;
            return (byte)result;

            /*if (Program.Season12)
            {
                return (byte)((@class & 0xF0) | ((@class << 3) & 0x08) | (((@class << 1) & 0x04) != 0?0x0C:0x00));
            }
            else
            {
                // CCC1 2000
                return (byte)(((@class & 0x70) << 1) | ((@class) == 1 ? 0x10 : (((@class & 0x03) == 2) ? 0x18 : 0x00)));
            }*/
        }

        public Character(Player plr, CharacterDto characterDto)
        {
            Player = plr;
            Player.Character = this;
            _autoRecuperationTime = DateTime.Now;
            CharacterDie += OnDead;
            Id = characterDto.CharacterId;
            Name = characterDto.Name;
            Class = (HeroClass)characterDto.Class;
            Level = characterDto.Level;
            Quests = new Quests(this, characterDto);
            Spells = new Spells(this, characterDto);
            Inventory = new Inventory(this, characterDto);
            MasterLevel = new MasterLevel(this, characterDto);
            Friends = new Friends(this, characterDto);
            Shop = new PShop(this);
            MonstersVP = new List<ushort>();
            ItemsVP = new List<ushort>();
            PlayersVP = new List<Player>();
            MuHelper = new MuBot(plr);
            Gens = new Gens(this, characterDto);
            State = ObjectState.Regen;
            CtlCode = (ControlCode)characterDto.CtlCode;
            Resets = characterDto.Resets;
            GremoryCase = new GremoryCase(this, characterDto);

            _position = new Point(characterDto.X, characterDto.Y);
            TPosition = _position;
            _map = (Maps)characterDto.Map;
            Map = ResourceCache.Instance.GetMaps()[_map];
            Map.AddPlayer(this);
            Map.SetAttribute(_position.X, _position.Y, MapAttributes.Stand);

            _exp = (ulong)characterDto.Experience;
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

            PCPoints = 0;

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
                Position = _position,
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
                ExpandedInv = characterDto.ExpandedInventory,
                Ruud = 1,
                ControlCode = (byte)CtlCode,
                MapX = (byte)Position.X,
                MapY = (byte)Position.Y,
            };

            CashShop = new CashShop(plr.Session, characterDto);
            plr.Session.SendAsync(StatsInfo).Wait();

            Inventory.SendInventory();

            Quests.SendList();

            if (Class >= HeroClass.MuseElf && BaseClass == HeroClass.FaryElf)
                Spells.TryAdd(Spell.InfinityArrow).Wait();

            Spells.SendList();
            MasterLevel.SendInfo();
            Gens.SendMemberInfo();
            try
            {
                plr.Session.SendAsync(new SResets { Resets = Resets }).Wait();
            }
            catch (Exception) { }
        }

        public void SendV2Message(object message, Player exclude = null)
        {
            lock (PlayersVP)
            {
                foreach (var plr in PlayersVP.Where(x => x != exclude))
                    plr.Session.SendAsync(message).Wait();
            }
        }

        #region EventHandlers
        public async void HPorSDChanged(RefillInfo info)
        {
            if (Party != null)
                Party.LifeUpdate();

            await Player.Session.SendAsync(new SHeatlUpdate(info, (ushort)_hp, (ushort)_sd, false));
        }
        private async void HPorSDMaxChanged()
        {
            if (Party != null)
                Party.LifeUpdate();

            await Player.Session.SendAsync(new SHeatlUpdate(RefillInfo.MaxChanged, (ushort)MaxHealth, (ushort)MaxShield, false));
        }
        private async void MPorBPChanged(RefillInfo info)
        {
            await Player.Session.SendAsync(new SManaUpdate(info, (ushort)_mp, (ushort)_bp));
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

            CalcStats();

            var levelPoint = BaseClass == HeroClass.MagicGladiator || BaseClass == HeroClass.DarkLord ? 7 : 5;
            levelPoint += MasterClass ? 1 : 0;
            LevelUpPoints += (ushort)(levelPoint * (Level - curLevel));
            _hp = MaxHealth;
            _mp = MaxMana;
            _bp = MaxStamina;
            _sd = MaxShield;

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
        private async void OnMoneyChange()
        {
            await Player.Session.SendAsync(new SItemGet { Result = 0xFE, Money = Money });
        }
        internal void DisposeKalimaGate()
        {
            if (KalimaGate != null)
            {
                KalimaGate.Killer = Player;
                KalimaGate.Life = 0;
                KalimaGate.Caller = null;
                KalimaGate.ViewPort.ForEach(x => x.Character.MonstersVP.Remove(KalimaGate.Index));
                KalimaGate.ViewPort.Clear();
                KalimaGate.Map.DelMonster(KalimaGate);
                MonstersMng.Instance.Monsters.Remove(KalimaGate);
                KalimaGate = null;
            }
        }
        private void OnDead(object obj, EventArgs args)
        {
            State = ObjectState.Dying;
            RegenTime = DateTimeOffset.Now.AddSeconds(4);
            var die = new SDiePlayer((ushort)Player.Session.ID, 1, _killerId);
            DisposeKalimaGate();
            Player.Session.SendAsync(die);
            SendV2Message(die);

            var EXPPenalty = 0.00f;

            if(Level >= 221)
            {
                switch(PKLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        EXPPenalty = 0.01f;
                        break;
                    case 4:
                        EXPPenalty = 0.05f;
                        break;
                    case 5:
                        EXPPenalty = 0.10f;
                        break;
                    case 6:
                        EXPPenalty = 0.20f;
                        break;
                }
            }else if(Level >= 151)
            {
                switch (PKLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        EXPPenalty = 0.02f;
                        break;
                    case 4:
                        EXPPenalty = 0.05f;
                        break;
                    case 5:
                        EXPPenalty = 0.10f;
                        break;
                    case 6:
                        EXPPenalty = 0.20f;
                        break;
                }
            }
            else if (Level >= 11)
            {
                switch (PKLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        EXPPenalty = 0.02f;
                        break;
                    case 4:
                        EXPPenalty = 0.05f;
                        break;
                    case 5:
                        EXPPenalty = 0.10f;
                        break;
                    case 6:
                        EXPPenalty = 0.20f;
                        break;
                }
            }

            var expReduced = Experience * EXPPenalty;
            Experience -= (ulong)expReduced;
        }
        #endregion
        public void CalcStats()
        {
            if (Inventory == null)
                return;

            var att = BaseInfo.Attributes;
            _hpMax = (att.Life + att.LevelLife * (Level - 1 + (MasterLevel.Level - 1)) + att.VitalityToLife * Vitality)* (1.0f + Inventory.IncreaseHP + Spells.IncreaseMaxHP);
            _mpMax = (att.Mana + att.LevelMana * (Level - 1 + (MasterLevel.Level - 1)) + att.EnergyToMana * Energy)* (1.0f + Inventory.IncreaseMP);
            _bpMax = ((att.StrToBP * StrengthTotal) + (att.AgiToBP * AgilityTotal) + (att.VitToBP * VitalityTotal) + (att.EneToBP * EnergyTotal)) * (1.0f + Spells.IncreaseMaxAG);
            _sdMax = TotalPoints * 3 + (Level * Level) / 30/* + Defense*/;

            Inventory.CalcStats();
            ObjCalc();
        }
        public void ObjCalc()
        {
            var right = Inventory?.Get(Equipament.RightHand)??null;
            var left = Inventory?.Get(Equipament.LeftHand)??null;

            switch(BaseClass)
            {
                case HeroClass.DarkWizard:
                    _leftAttackMin = (StrengthTotal / 8);
                    _leftAttackMax = (StrengthTotal / 4);
                    _rightAttackMin = (StrengthTotal / 8);
                    _rightAttackMax = (StrengthTotal / 4);
                    break;
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
                case HeroClass.Summoner:
                    _leftAttackMin = (StrengthTotal / 8);
                    _leftAttackMax = (StrengthTotal / 4);
                    _rightAttackMin = (StrengthTotal / 8);
                    _rightAttackMax = (StrengthTotal / 4);
                    break;
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

            if (BaseClass == HeroClass.DarkKnight || BaseClass == HeroClass.MagicGladiator)
            {
                if ((right?.Number ?? ItemNumber.Invalid) == (left?.Number ?? ItemNumber.Invalid) && (right?.Number ?? ItemNumber.Invalid) != ItemNumber.Invalid)
                {
                    _rightAttackMin *= 0.55f;
                    _rightAttackMax *= 0.55f;
                    _leftAttackMin *= 0.55f;
                    _leftAttackMax *= 0.55f;
                }
            }

            switch(BaseClass)
            {
                case HeroClass.DarkKnight:
                    _defense = AgilityTotal / 3.0f;
                    _defenseRatePvM = AgilityTotal / 3.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.5f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 1.5f + StrengthTotal / 4;
                    _attackRatePvP = Level * 5 + AgilityTotal * 4.5f;
                    _attackSpeed = AgilityTotal / 15.0f;
                    break;
                case HeroClass.DarkWizard:
                    _defense = AgilityTotal / 5.0f;
                    _defenseRatePvM = AgilityTotal / 3.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.25f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 1.5f + StrengthTotal / 4;
                    _attackRatePvP = Level * 3 + AgilityTotal * 4.0f;
                    _attackSpeed = AgilityTotal / 10.0f;
                    _magicAttackMin = EnergyTotal / 9.0f;
                    _magicAttackMax = EnergyTotal / 4.0f;
                    break;
                case HeroClass.FaryElf:
                    _defense = AgilityTotal / 10.0f;
                    _defenseRatePvM = AgilityTotal / 4.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.1f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 1.5f + StrengthTotal / 4;
                    _attackRatePvP = Level * 5 + AgilityTotal * 0.6f;
                    _attackSpeed = AgilityTotal / 50.0f;
                    _magicAttackMax = _leftAttackMax;
                    _magicAttackMin = _leftAttackMin;
                    break;
                case HeroClass.MagicGladiator:
                    _defense = AgilityTotal / 5.0f;
                    _defenseRatePvM = AgilityTotal / 3.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.25f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 1.5f + StrengthTotal / 4;
                    _attackRatePvP = Level * 5 + AgilityTotal * 3.5f;
                    _attackSpeed = AgilityTotal / 15.0f;
                    break;
                case HeroClass.DarkLord:
                    _defense = AgilityTotal / 7.0f;
                    _defenseRatePvM = AgilityTotal / 7.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.5f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 2.5f + StrengthTotal / 4 + CommandTotal / 10;
                    _attackRatePvP = Level * 5 + AgilityTotal * 4.0f;
                    _attackSpeed = AgilityTotal / 10.0f;
                    break;
                case HeroClass.Summoner:
                    _defense = AgilityTotal / 3.0f;
                    _defenseRatePvM = AgilityTotal / 4.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.5f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 1.5f + StrengthTotal / 4;
                    _attackRatePvP = Level * 5 + AgilityTotal * 3.5f;
                    _attackSpeed = AgilityTotal / 20.0f;
                    _magicAttackMin = EnergyTotal / 9.0f;
                    _magicAttackMax = EnergyTotal / 4.0f;
                    break;
                case HeroClass.RageFighter:
                    _defense = AgilityTotal / 3.0f;
                    _defenseRatePvM = AgilityTotal / 3.0f;
                    _defenseRatePvP = Level * 2 + AgilityTotal / 0.5f;
                    _attackRatePvM = Level * 5 + AgilityTotal * 1.5f + StrengthTotal * 4;
                    _attackRatePvP = Level * 5 + AgilityTotal * 4.5f;
                    _attackSpeed = AgilityTotal / 15.0f;
                    if (_attackSpeed > 288) _attackSpeed = 288.0f;
                    break;
            }

            if (_attackSpeed > 288) _attackSpeed = 288.0f;

            _defense += Inventory.Defense;
            _defenseRatePvP += Inventory.DefenseRate;
            _defenseRatePvM += Inventory.DefenseRate;

            Player.Session.SendAsync(new SAttackSpeed {
                AttackSpeed = (uint)_attackSpeed,
                MagicSpeed = (uint)_attackSpeed / 2,
            }).Wait();
        }
        private ulong GetExperienceFromLevel(ushort level)
        {
            return (((level + 9ul) * level) * level) * 10ul + ((level > 255) ? ((((ulong)(level - 255) + 9ul) * (level - 255ul)) * (level - 255ul)) * 1000ul : 0ul);
        }

        public void TryRegen()
        {
            if(RegenTime <= DateTimeOffset.Now)
            {
                Health = MaxHealth;
                Mana = MaxMana;
                Shield = MaxShield;
                Stamina = MaxStamina;
                State = ObjectState.Regen;
                _position = Map.GetRespawn();
                CharacterRegen?.Invoke(this, new EventArgs());
                var regen = new SCharRegen(MapID, (byte)_position.X, (byte)_position.Y, 1, (ushort)Health, (ushort)Mana, (ushort)Shield, (ushort)Stamina, (uint)Experience, Money);
                Player.Session.SendAsync(regen).Wait();
            }
        }

        public async Task WarpTo(Maps map, Point position, byte dir)
        {
            if(MapID != map)
            {
                _map = map;
                Map?.ClearAttribute(_position.X, _position.Y, MapAttributes.Stand);
                Map?.DelPlayer(this);
                MapChanged?.Invoke(this, new EventArgs());
                Map = ResourceCache.Instance.GetMaps()[_map];
                Map.AddPlayer(this);
                _position = position;
                Map.SetAttribute(_position.X, _position.Y, MapAttributes.Stand);
            }else
            {
                Map.PositionChanged(Position, position);
                Map.SendWeather(this);
                _position = position;
            }
            Direction = dir;
            TPosition = _position;

            if (!MapServerManager.CheckMapServerMove(Player.Session, map))
                return;

            if (State == ObjectState.Live)
                await Player.Session.SendAsync(new STeleport(256, MapID, _position, Direction));
        }

        public async Task WarpTo(int gate)
        {
            var g = ResourceCache.Instance.GetGates()[gate];

            if (g.Door.Left<g.Door.Right && g.Door.Top<g.Door.Bottom)
            {
                var rand = new Random();
                var randX = rand.Next(g.Door.Left, g.Door.Right);
                var randY = rand.Next(g.Door.Top, g.Door.Bottom);

                await WarpTo(g.Map, new Point(randX, randY), g.Dir);
            }else
            {
                await WarpTo(g.Map, new Point(g.Door.Left, g.Door.Top), g.Dir);
            }
        }

        public void TeleportTo(Point position)
        {
            _position = position;
            SubSystem.Instance.AddDelayedMessage(Player, TimeSpan.FromMilliseconds(500), new STeleport(0, MapID, _position, Direction));
        }

        public async Task Save(GameContext db)
        {
            if (_needSave)
            {
                _needSave = false;
                var charDto = db.Characters.First(x => x.CharacterId == Id);
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
                charDto.CtlCode = (int)CtlCode;
                charDto.Resets = Resets;
                if(charDto.Gens == null)
                {
                    charDto.Gens = new GensDto
                    {
                        CharacterId = charDto.CharacterId,
                        Character = charDto,
                        Class = 14,
                        Contribution = 0,
                        Influence = 0,
                        Ranking = 9999,
                    };
                }
                charDto.Gens.Influence = (int)Gens.Influence;
                charDto.Gens.Ranking = Gens.Ranking;
                charDto.Gens.Class = Gens.Class;
                charDto.Gens.Contribution = Gens.Contribution;
                db.Characters.Update(charDto);
                await db.SaveChangesAsync();
            }
            await Inventory.Save(db);
            await Spells.Save(db);
            await Quests.Save(db);
            MasterLevel.Save(db);
            await db.SaveChangesAsync();
        }

        #region Battle
        public int Attack(Character target, out DamageType type)
        {
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var rightHand = Inventory.Get(Equipament.RightHand);
            var pet = Inventory.Get(Equipament.Pet);
            var wing = Inventory.Get(Equipament.Wings);
            var twing = target.Inventory.Get(Equipament.Wings);
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate*100;
            var tReflect = target.Inventory.Reflect;

            type = DamageType.Regular;
            var attack = 0.0f;

            if (MissCheck(target))
            {
                type = DamageType.Miss;
                return (int)attack;
            }

            if (excellentRate > _rand.Next(100))
                type = DamageType.Excellent;
            else if (criticalRate > _rand.Next(100))
                type = DamageType.Critical;

            attack = BaseAttack(type != DamageType.Regular);
            attack *= (type == DamageType.Excellent) ? 2.2f : 1.0f;
            attack += Spells.BuffList.Sum(x => x.AttackAdd);
            attack += rightHand?.AditionalDamage ?? 0;
            attack += leftHand?.AditionalDamage ?? 0;

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if(Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }
            }

            if ((pet?.Number ?? ItemNumber.Zen) == ItemNumber.FromTypeIndex(13, 1)) // Satan 30% Attack Fisic & Magic
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.3f;
                }
            }

            if (twing != null) // Wings decrease Dmg 12%+(Level*2)%
                attack *= 0.88f - twing.Plus * 0.02f;

            attack -= target.Inventory.Defense;
            WeaponDurDown(target.Inventory.Defense);
            return (int)attack;
        }

        public int Attack(Monster target, out DamageType type)
        {
            type = DamageType.Regular;
            var wing = Inventory.Get(Equipament.Wings);
            var pet = Inventory.Get(Equipament.Pet);
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate*100;
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var rightHand = Inventory.Get(Equipament.RightHand);

            var attack = 0.0f;

            if(!MissCheck(target))
            {
                type = DamageType.Miss;
                return 0;
            }

            if (excellentRate > _rand.Next(100))
                type = DamageType.Excellent;
            else if (criticalRate > _rand.Next(100))
                type = DamageType.Critical;

            attack = BaseAttack(type != DamageType.Regular);
            attack *= (type == DamageType.Excellent) ? 2.2f : 1.0f;
            attack += rightHand?.AditionalDamage ?? 0;
            attack += leftHand?.AditionalDamage ?? 0;
            attack += Spells.BuffList.Sum(x => x.AttackAdd);

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }
            }

            if((pet?.Number??ItemNumber.Zen) == ItemNumber.FromTypeIndex(13,1)) // Satan 30% Attack Fisic & Magic
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.3f;
                }
            }

            if (attack < 0)
                attack = 0;

            attack -= target.Defense;
            WeaponDurDown(target.Defense);
            return (int)attack;
        }

        private bool MissCheck(Monster target)
        {
            if (AttackRatePvM < target.Info.Success)
            {
                if (_rand.Next(100) >= 5)
                {
                    return false;
                }
            }
            else
            {
                if (_rand.Next(AttackRatePvM) < target.Info.Success)
                {
                    return false;
                }
            }
            return true;
        }

        private bool MissCheck(Character target)
        {
            if (AttackRatePvP < target.DefenseRatePvP)
            {
                if (_rand.Next(100) >= 5)
                {
                    return false;
                }
            }
            else
            {
                if (_rand.Next(AttackRatePvP) < target.DefenseRatePvP)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task GetAttacked(ushort source, byte dirdis, byte aa, int dmg, DamageType type, Spell isMagic, int eDmg)
        {
            if (State != ObjectState.Live)
                return;

            if (dmg < 0)
                dmg = 0;

            GSSession sourceSession = source >= MonstersMng.MonsterStartIndex ? null : Program.server.Clients.FirstOrDefault(x => x.ID == source);

            _killerId = source;
            switch (_rand.Next(6))
            {
                case 0:
                    Inventory.Get(Equipament.Helm)?.ArmorDurabilityDown(dmg);
                    break;
                case 1:
                    Inventory.Get(Equipament.Armor)?.ArmorDurabilityDown(dmg);
                    break;
                case 2:
                    Inventory.Get(Equipament.Pants)?.ArmorDurabilityDown(dmg);
                    break;
                case 3:
                    Inventory.Get(Equipament.Gloves)?.ArmorDurabilityDown(dmg);
                    break;
                case 4:
                    Inventory.Get(Equipament.Boots)?.ArmorDurabilityDown(dmg);
                    break;
                case 5:
                    Inventory.Get(Equipament.Wings)?.ArmorDurabilityDown(dmg);
                    break;
            }
            object message;
            var sdDamage = 0.0f;
            var healthDamage = 0.0f;

            if (sourceSession != null)
            {
                sdDamage = ((dmg+eDmg) * 0.8f);
                healthDamage = ((dmg+eDmg) * 0.2f);
                if(sdDamage > Shield)
                {
                    healthDamage += (sdDamage - Shield);
                }

                if(healthDamage > Health)
                {
                    healthDamage = Health;
                }
            }
            else
            {
                healthDamage = dmg+eDmg;
            }

            Shield -= sdDamage;
            Health -= healthDamage;
            _deadlyDmg = (int)healthDamage;

            switch (Program.Season)
            {
                case 9:
                    message = new SAttackResultS9((ushort)Player.Session.ID, (ushort)healthDamage, type, (ushort)sdDamage);
                    break;
                default:
                    message = new SAttackResult((ushort)Player.Session.ID, (ushort)healthDamage, type, (ushort)sdDamage);
                    break;
            }

            var reflex = Inventory.Reflect + Spells.BuffList.Sum(x => x.DamageDeflection);
            var dmgReflect = reflex * dmg;
            if (dmgReflect>0)
            {
                if (sourceSession != null)
                {
                    await sourceSession.Player.Character.GetAttacked(Player.ID, 0, 0, (int)dmgReflect, DamageType.Reflect, Spell.None, 0);
                } else
                {
                    Monster mob = MonstersMng.Instance.GetMonster(source);
                    await mob.GetAttacked(Player, (int)dmgReflect, DamageType.Reflect, 0);
                }
            }

            if (State != ObjectState.Dying)
            {
                if (isMagic == Spell.None)
                {
                    var msg = new SAction(source, dirdis, aa, (ushort)Player.Session.ID);
                    await Player.Session.SendAsync(message);
                    if(sourceSession != null)
                        await sourceSession.SendAsync(message);
                    await Player.Session.SendAsync(msg);
                    Player.SendV2Message(msg, sourceSession?.Player);
                }
                else
                {
                    SubSystem.Instance.AddDelayedMessage(Player, TimeSpan.FromMilliseconds(100), message);
                    SubSystem.Instance.AddDelayedMessage(sourceSession?.Player, TimeSpan.FromMilliseconds(100), message);


                    switch (Program.Season)
                    {
                        case 9:
                            message = new SMagicAttackS9(isMagic, source, (ushort)Player.Session.ID);
                            break;
                        default:
                            message = new SMagicAttack(isMagic, source, (ushort)Player.Session.ID);
                            break;
                    }

                    await Player.Session.SendAsync(message);
                    Player.SendV2Message(message);
                }
            }
        }

        public int SkillAttack(SpellInfo spell, int targetDefense, out DamageType type)
        {
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate;
            var wing = Inventory.Get(Equipament.Wings);
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var rightHand = Inventory.Get(Equipament.RightHand);

            var attack = 0.0f;
            type = DamageType.Regular;

            if (excellentRate > _rand.Next(100))
                type = DamageType.Excellent;
            else if (criticalRate > _rand.Next(100))
                type = DamageType.Critical;

            WeaponDurDown(targetDefense);

            attack = BaseAttack(type != DamageType.Regular);
            attack *= (type == DamageType.Excellent) ? 2.2f : 1.0f;
            attack += _rand.Next(spell.Damage.X, spell.Damage.Y);
            attack += rightHand?.AditionalDamage ?? 0;
            attack += leftHand?.AditionalDamage ?? 0;

            if (wing != null) // Wings increase Dmg 12%+(Level*2)%
            {
                Health -= 3;
                if (Health > 0)
                {
                    attack *= 1.12f + wing.Plus * 0.02f;
                }
            }

            attack *= (200.0f + EnergyTotal / 10.0f) / 100.0f;

            attack -= targetDefense;
            return (int)attack;
        }
        
        public int MagicAttack(SpellInfo spell, int targetDefense, out DamageType type)
        {
            var criticalRate = Inventory.CriticalRate;
            var excellentRate = Inventory.ExcellentRate;
            var wing = Inventory.Get(Equipament.Wings);
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var rightHand = Inventory.Get(Equipament.RightHand);

            WeaponDurDown(targetDefense);
            var magicAdd = 0;

            if (rightHand != null)
                magicAdd = rightHand.BasicInfo.MagicPower / 2 + rightHand.Plus * 2;

            var attack = 0.0f;
            type = DamageType.Regular;
            if (excellentRate > _rand.Next(100))
                type = DamageType.Excellent;
            else if (criticalRate > _rand.Next(100))
                type = DamageType.Critical;

            attack += (type != DamageType.Regular) ? spell.Damage.Y + _magicAttackMax : _rand.Next((int)(spell.Damage.X + _magicAttackMin), (int)(spell.Damage.Y + _magicAttackMax));
            attack += Spells.BuffList.Sum(x => x.AttackAdd);
            attack *= 1.0f + magicAdd / 100.0f;
            attack += rightHand?.AditionalMagic??0;
            attack += Inventory.IncreaseWizardry;
            attack *= 1.0f + Inventory.IncreaseWizardryRate;
            attack *= (type == DamageType.Excellent) ? 2.2f : 1.0f;

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

            attack -= targetDefense;
            return (int)attack;
        }

        public float BaseAttack(bool critical)
        {
            _autoRecuperationTime = DateTime.Now;
            var leftHand = Inventory.Get(Equipament.LeftHand);
            var attack = 0.0f;

            attack = critical ? _rightAttackMax : _rand.Next((int)_rightAttackMin, (int)_rightAttackMax);

            if (leftHand?.Attack ?? false)
                attack += critical ? _leftAttackMax : _rand.Next((int)_leftAttackMin, (int)_leftAttackMax);

            return attack;
        }

        public async Task<int> PentagramAttack(Character target)
        {
            var pItem = Inventory.Get(Equipament.Pentagrama);
            if (pItem == null)
                return 0;

            var tpItem = target.Inventory.Get(Equipament.Pentagrama);

            var def = tpItem?.Defense ?? 0;
            var tElement = tpItem?.PentagramaMainAttribute ?? Element.None;

            var max = Math.Max(pItem.AttackMax, pItem.AttackMin);
            var min = Math.Min(pItem.AttackMax, pItem.AttackMin);

            var dmg = Program.RandomProvider(max, min) * Pentagrama.GetElementalFactor(pItem.PentagramaMainAttribute, tElement);
            dmg -= def;

            var eMessage = new SElementalDamage
            {
                Damage = (uint)dmg,
                Element = pItem.PentagramaMainAttribute,
                Number = Player.ID,
                Target = target.Player.ID,
            };

            await Player.Session.SendAsync(eMessage);
            await target.Player.Session.SendAsync(eMessage);

            return (int)dmg;
        }

        public async Task<int> PentagramAttack(Monster target)
        {
            var pItem = Inventory.Get(Equipament.Pentagrama);
            if (pItem == null)
                return 0;

            var tpItem = target.Info.MainAttribute;

            var def = target.Info.PentagramDefense;
            var tElement = (Element)target.Info.MainAttribute;

            var max = Math.Max(pItem.AttackMax, pItem.AttackMin);
            var min = Math.Min(pItem.AttackMax, pItem.AttackMin);

            var dmg = Program.RandomProvider(max, min) * Pentagrama.GetElementalFactor(pItem.PentagramaMainAttribute, tElement);
            dmg -= def;

            var eMessage = new SElementalDamage
            {
                Damage = (uint)dmg,
                Element = pItem.PentagramaMainAttribute,
                Number = target.Index,
                Target = Player.ID,
            };

            await Player.Session.SendAsync(eMessage);

            return (int)dmg;
        }
        #endregion

        public void Autorecovery()
        {
            var elapsed = DateTime.Now - _autoRecuperationTime;

            var add = (elapsed.TotalSeconds > 25)? 10 : (elapsed.TotalSeconds > 15)? 5 : (elapsed.TotalSeconds > 10)? 1 : 0;

            var update1 = add > 0 && (_hp < MaxHealth || _sd < MaxShield);
            var update2 = _mp < MaxMana || _bp < MaxStamina;

            if (_hp < MaxHealth) _hp += Math.Min(add, MaxHealth - _hp);
            if (_sd < MaxShield) _sd += Math.Min(add, MaxShield - _sd);

            float addMp = 0;
            float addBp = 0;

            switch (BaseClass)
            {
                case HeroClass.DarkKnight:
                    addMp = MaxMana / 27.5f;
                    addBp = 2 + (float)MaxStamina / 20;
                    break;
                case HeroClass.DarkWizard:
                case HeroClass.FaryElf:
                case HeroClass.Summoner:
                    addMp = (float)MaxMana / 27.5f;
                    addBp = 2 + (float)MaxStamina / 33.333f;
                    break;
                case HeroClass.MagicGladiator:
                case HeroClass.DarkLord:
                    addMp = (float)MaxMana / 27.5f;
                    addBp = 1.9f + (float)MaxStamina / 33;
                    break;

            }

            if (_mp < MaxMana) _mp += Math.Min(addMp, MaxMana - _mp);
            if (_bp < MaxStamina) _bp += Math.Min(addBp, MaxStamina - _bp);

            if (update1)
                HPorSDChanged(RefillInfo.Drink);

            if (update2)
                MPorBPChanged(RefillInfo.Drink);
        }

        public void WeaponDurDown(int Defense)
        {
            var left = Inventory.Get(Equipament.LeftHand);
            var right = Inventory.Get(Equipament.RightHand);

            if ((left != null && left.Number.Type == ItemType.BowOrCrossbow && left.Number.Index != 7)) // Isn't Bolts
            {
                if (Inventory.Arrows.Durability <= 0)
                {
                    return;
                }

                if(!Spells.BufActive(SkillStates.InfinityArrow))
                    right.Durability--;

                if(right.Durability == 0)
                {
                    Inventory.Delete(right).Wait();
                }
                left.BowWeaponDurabilityDown(Defense);
            }else
            if (right != null && right.Number.Type == ItemType.BowOrCrossbow && right.Number.Index != 15) // Isn't Arrows
            {
                if (left.Durability <= 0)
                {
                    return;
                }

                if (!Spells.BufActive(SkillStates.InfinityArrow))
                    right.Durability--;
                if (left.Durability == 0)
                {
                    Inventory.Delete(left).Wait();
                }
                right.BowWeaponDurabilityDown(Defense);
            }

            if (right != null && left != null  && right.Number.Type >= ItemType.Sword && right.Number.Type < ItemType.BowOrCrossbow &&
                left.Number.Type >= ItemType.Sword && left.Number.Type < ItemType.BowOrCrossbow)
            {
                var item = Program.RandomProvider(2) == 0 ? right : left;
                item.NormalWeaponDurabilityDown(Defense);
            }
            else if (right != null && right.Number.Type >= ItemType.Sword && right.Number.Type < ItemType.BowOrCrossbow)
            {
                right.NormalWeaponDurabilityDown(Defense);
            }
        }

        public override string ToString()
        {
            return Name+" Level:"+Level+" Class:"+Class;
        }

        #region Commands
        public static async void AddStr(object session, CommandEventArgs eventArgs)
        {
            var Session = session as GSSession;
            var @char = Session.Player.Character;

            if(int.TryParse(eventArgs.Argument, out int res))
            {
                if(res <= @char.LevelUpPoints)
                {
                    if (res + @char.StrengthTotal >= ushort.MaxValue)
                        res = ushort.MaxValue - @char.StrengthTotal;

                    @char.LevelUpPoints -= (ushort)res;
                    @char.Strength += (ushort)res;


                    await Session.SendAsync(new SNotice(NoticeType.Blue, "You need relogin"));
                    return;
                }
                await Session.SendAsync(new SNotice(NoticeType.Blue, "Insufficient Points"));
                return;
            }

            await Session.SendAsync(new SNotice(NoticeType.Blue, "Syntax error. command is: /add*** <Number>"));
        }

        public static async void AddAgi(object session, CommandEventArgs eventArgs)
        {
            var Session = session as GSSession;
            var @char = Session.Player.Character;

            if (int.TryParse(eventArgs.Argument, out int res))
            {
                if (res <= @char.LevelUpPoints)
                {
                    if (res + @char.AgilityTotal >= ushort.MaxValue)
                        res = ushort.MaxValue - @char.AgilityTotal;

                    @char.LevelUpPoints -= (ushort)res;
                    @char.Agility += (ushort)res;
                    
                    await Session.SendAsync(new SNotice(NoticeType.Blue, "You need relogin"));
                    return;
                }
                await Session.SendAsync(new SNotice(NoticeType.Blue, "Insufficient Points"));
                return;
            }

            await Session.SendAsync(new SNotice(NoticeType.Blue, "Syntax error. command is: /add*** <Number>"));
        }

        public static async void AddVit(object session, CommandEventArgs eventArgs)
        {
            var Session = session as GSSession;
            var @char = Session.Player.Character;

            if (int.TryParse(eventArgs.Argument, out int res))
            {
                if (res <= @char.LevelUpPoints)
                {
                    if (res + @char.VitalityTotal >= ushort.MaxValue)
                        res = ushort.MaxValue - @char.VitalityTotal;

                    @char.LevelUpPoints -= (ushort)res;
                    @char.Vitality += (ushort)res;


                    await Session.SendAsync(new SNotice(NoticeType.Blue, "You need relogin"));
                    return;
                }
                await Session.SendAsync(new SNotice(NoticeType.Blue, "Insufficient Points"));
                return;
            }

            await Session.SendAsync(new SNotice(NoticeType.Blue, "Syntax error. command is: /add*** <Number>"));
        }

        public static async void AddEne(object session, CommandEventArgs eventArgs)
        {
            var Session = session as GSSession;
            var @char = Session.Player.Character;

            if (int.TryParse(eventArgs.Argument, out int res))
            {
                if (res <= @char.LevelUpPoints)
                {
                    if (res + @char.EnergyTotal >= ushort.MaxValue)
                        res = ushort.MaxValue - @char.EnergyTotal;

                    @char.LevelUpPoints -= (ushort)res;
                    @char.Energy += (ushort)res;


                    await Session.SendAsync(new SNotice(NoticeType.Blue, "You need relogin"));
                    return;
                }
                await Session.SendAsync(new SNotice(NoticeType.Blue, "Insufficient Points"));
                return;
            }

            await Session.SendAsync(new SNotice(NoticeType.Blue, "Syntax error. command is: /add*** <Number>"));
        }

        public static async void AddCmd(object session, CommandEventArgs eventArgs)
        {
            var Session = session as GSSession;
            var @char = Session.Player.Character;

            if (int.TryParse(eventArgs.Argument, out int res))
            {
                if (res <= @char.LevelUpPoints)
                {
                    if (res + @char.CommandTotal >= ushort.MaxValue)
                        res = ushort.MaxValue - @char.CommandTotal;

                    @char.LevelUpPoints -= (ushort)res;
                    @char.Command += (ushort)res;

                    await Session.SendAsync(new SNotice(NoticeType.Blue, "You need relogin"));
                    return;
                }
                await Session.SendAsync(new SNotice(NoticeType.Blue, "Insufficient Points"));
                return;
            }

            await Session.SendAsync(new SNotice(NoticeType.Blue, "Syntax error. command is: /add*** <Number>"));
        }

        public void Dispose()
        {
            Map?.DelPlayer(this);
            Map = null;

            PartyManager.Remove(Player);
            Duel?.Leave(Player);
            Duel = null;
            DisposeKalimaGate();

            var mobVp = MonstersVP.Select(x => MonstersMng.Instance.GetMonster(x)).ToList();
            foreach (var m in mobVp.Where(x => x.Target == Player))
            {
                m.Target = null;
            }
        }
        #endregion
    }
}
