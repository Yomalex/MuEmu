using MU.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using MuEmu.Data;
using System.Threading;
using MuEmu.Util;
using MU.Resources;
using MU.Network;

namespace MuEmu.Monsters
{
    public enum MonsterState
    {
        Idle,
        Walking,
        Battle,
    }

    public class Monster
    {
        private static byte[,] _walkDirs;
        private static MapAttributes[] _cantGo;
        private static ushort[] _maxItemIndex;
        private static Random _rand;
        private float _life;
        private ObjectState _state;
        private DateTimeOffset _regen;
        private Player _target;
        private DateTimeOffset _nextAction;
        private MonsterState _monsterState;
        private List<Point> _path = null;
        private Point _TPosition;

        public bool UseTeleport { get; set; }

        private bool _active;

        public ushort Index { get; set; }
        public ObjectState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;

                switch(value)
                {
                    case ObjectState.Die:
                        _regen = DateTimeOffset.Now.AddSeconds(Info.RegenTime);
                        Die?.Invoke(this, new EventArgs());
                        break;
                    case ObjectState.Dying:
                        Dying?.Invoke(this, new EventArgs());
                        break;
                }

                _state = value;
            }
        }
        public ObjectType Type { get; set; }
        public MonsterState MonsterState { get => _monsterState; set => _monsterState = value; }
        public MonsterBase Info { get; }
        public ushort Level => Info.Level;
        public float Life
        {
            get => _life;
            set
            {
                if (_life == value)
                    return;

                _life = value;

                if (_life <= 0)
                {
                    _life = 0;
                    State = ObjectState.Dying;
                }

                var life = _life / MaxLife * 255.0f;

                ViewPort.Select(x => x.Session).SendAsync(new SLifeInfo { Life = (uint)life, MaxLife = 255u, Number = Index }).Wait();
            }
        }
        public float MaxLife => Info.HP;
        public float Mana { get; set; }
        public float MaxMana => Info.MP;
        public Spells Spells { get; set; }
        public Maps MapID { get; set; }
        public MapInfo Map { get; private set; }
        public Point Spawn { get; private set; }
        public Point Position { get; private set; }
        public Point TPosition
        {
            get => _TPosition;
            private set
            {
                _TPosition = value;
                if (Position != _TPosition)
                    MakePath();
            }
        }
        public Player Target
        {
            get => _target;
            set
            {
                if (_target == value)
                    return;

                if (_target != null && _target.Character != null)
                {
                    _target.Character.CharacterDie -= EnemyDie;
                    _target.Character.MapChanged -= EnemyDie;
                }

                _target = value;

                if (value == null)
                    return;

                _target.Character.CharacterDie += EnemyDie;
                _target.Character.MapChanged += EnemyDie;
            }
        }
        public List<Player> ViewPort { get; set; } = new List<Player>();
        public Player Killer { get; set; }
        public Player Caller { get; set; }
        public object Params { get; set; }
        public ushort DeadlyDmg { get; set; }
        public byte Direction { get; set; }
        public List<Item> ItemBag { get; set; }
        public bool Active { get => _active; set { _active = value; State = ObjectState.Regen; } }
        public Element Element { get; set; }

        public bool CanDrop { get; set; }
        public Dictionary<Player, int> DamageSum { get; private set; } = new Dictionary<Player, int>();
        public int Attack => Info.Attack + (_rand.Next(Info.DmgMin, Info.DmgMax));
        public int Defense => Info.Defense;

        /// <summary>
        /// On die monster trigger this event with sender as Monster object
        /// </summary>
        public event EventHandler Die;
        /// <summary>
        /// On Dying monster trigger this event with sender as Monster object
        /// </summary>
        public event EventHandler Dying;

        /// <summary>
        /// On regen monster trigger this event with sender as Monster object
        /// </summary>
        public event EventHandler Regen;

        // IA Unit
        public Monster Leader
        {
            get => _leader; set
            {
                _leader = value;
                _delta = _leader.Position.Substract(Position);
            }
        }
        public bool CanGoToBattleState { get; set; }
        private Point _delta;
        private Monster _leader;

        public Monster(ushort Monster, ObjectType type, Maps mapID, Point position, byte direction, Element element = Element.None)
        {
            CanGoToBattleState = true;
            Type = type;
            MapID = mapID;
            Spawn = position;
            Position = position;
            _TPosition = position;
            Direction = direction;
            Info = MonstersMng.Instance.MonsterInfo[Monster];
            Life = Info.HP;
            Mana = Info.MP;
            Active = true;
            CanDrop = true;
            Map = ResourceCache.Instance.GetMaps()[MapID];
            Map.AddMonster(this);
            State = ObjectState.Regen;
            Dying += OnDying;
            Spells = new Spells(this);
            ItemBag = new List<Item>();
            _nextAction = DateTimeOffset.Now;
            Element = element;
            if (_rand == null)
            {
                _rand = new Random();
                _maxItemIndex = new ushort[(int)ItemType.End];
                _cantGo = new MapAttributes[] { MapAttributes.Hide, MapAttributes.NoWalk, MapAttributes.Safe };
                _walkDirs = new byte[3, 3]{
                    { 0, 1, 2 },
                    { 7, 0, 3 },
                    { 6, 5, 4 },
                };

                foreach (var t in Enum.GetValues(typeof(ItemType)))
                {
                    if ((ItemType)t == ItemType.End)
                        break;

                    _maxItemIndex[(int)(ItemType)t] = (ushort)ResourceCache.Instance.GetItems().Where(x => (new ItemNumber(x.Key)).Type == (ItemType)(t)).Count();
                }
            }
        }

        public async Task GetAttacked(Player plr, int dmg, DamageType type, int eDmg)
        {
            if (State != ObjectState.Live || Type == ObjectType.NPC)
                return;

            if (dmg < 0)
                dmg = 0;

            if (DamageSum.ContainsKey(plr))
                DamageSum[plr] += dmg;
            else
                DamageSum.Add(plr, dmg);
            Killer = plr;
            if (Life < dmg + eDmg)
            {
                var tot = dmg + eDmg;
                dmg = (int)(dmg * Life / tot);
                eDmg = (int)(eDmg * Life / tot);
                Life = 0;
            }
            else
            {
                Life -= dmg + eDmg;
            }

            var dmgSend = dmg < ushort.MaxValue ? (ushort)dmg : ushort.MaxValue;
            DeadlyDmg = dmgSend;
            plr.Character.HuntingRecord.AttackPVM(dmg);
            plr.Character.HuntingRecord.ElementalAttackPVM(eDmg);

            if (State != ObjectState.Dying)
            {
                var attack = VersionSelector.CreateMessage<SAttackResult>(Index, dmgSend, type, (ushort)0);
                await plr.Session.SendAsync(attack);
            }
        }

        public void GetAttackedDelayed(Player plr, int dmg, DamageType type, TimeSpan delay)
        {
            if (State != ObjectState.Live || Type == ObjectType.NPC)
                return;

            if (DamageSum.ContainsKey(plr))
                DamageSum[plr] += dmg;
            else
                DamageSum.Add(plr, dmg);

            var dmgSend = dmg < ushort.MaxValue ? (ushort)dmg : ushort.MaxValue;
            DeadlyDmg = dmgSend;
            Killer = plr;
            Life -= dmg;

            if (State != ObjectState.Dying)
            {
                object message = VersionSelector.CreateMessage<SAttackResult>(Index, dmgSend, type, (ushort)0);
                SubSystem.Instance.AddDelayedMessage(plr, TimeSpan.FromMilliseconds(100), message);
            }
        }

        public void TryRegen()
        {
            if (_regen > DateTimeOffset.Now)
                return;

            DamageSum.Clear();
            Life = MaxLife;
            Mana = MaxMana;
            Position = Spawn;
            TPosition = Spawn;
            ViewPort.Clear();
            Target = null;
            Killer = null;
            DeadlyDmg = 0;
            _monsterState = MonsterState.Idle;
            State = ObjectState.Regen;

            Regen?.Invoke(this, new EventArgs());
        }

        private int Distance(Point A, Point B)
        {
            return (int)Math.Sqrt((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y));
        }

        public void Update()
        {
            if (Type != ObjectType.Monster)
                return;

            if (Life == 0 && State == ObjectState.Live)
                Life = MaxLife;

            if (_nextAction > DateTimeOffset.Now || Spells.BufActive(SkillStates.SkillSleep))
                return;

            lock (ViewPort)
            {
                ViewPort = Map.Players
                    .Where(x => x.Player.Status == LoginStatus.Playing && Distance(x.Position, Position) <= 18)
                    .Select(x => x.Player)
                    .ToList();
            }

            if (Target == null && Leader != null)
            {
                _monsterState = Leader._monsterState;
                TPosition = Leader.TPosition.Add(_delta);
            }

            if (_monsterState == MonsterState.Walking)
            {
                _nextAction = DateTimeOffset.Now.AddMilliseconds(Info.MoveSpeed);

                if (_path.Count == 0)
                {
                    _monsterState = Target != null ? MonsterState.Battle : MonsterState.Idle;
                }
                else
                {
                    Position = _path[0];
                    _path.RemoveAt(0);

                    if (Target != null && Target.Character != null)
                    {
                        try
                        {
                            var dis = Distance(Target.Character.Position, Position);
                            if (dis <= Info.AttackRange)
                            {
                                _monsterState = MonsterState.Battle;
                                _path.Clear();
                                return;
                            }
                            else if (dis > Info.ViewRange)
                            {
                                _monsterState = MonsterState.Idle;
                                _path.Clear();
                                return;
                            }
                        }catch(Exception)
                        {
                            Target = null;
                        }
                    }
                }
            }
            if (_monsterState == MonsterState.Battle)
            {
                if (Target == null || Target.Status != LoginStatus.Playing)
                {
                    _monsterState = MonsterState.Idle;
                    return;
                }
                var dis = Distance(Target.Character.Position, Position);
                if (dis <= Info.AttackRange && CanGoToBattleState)
                {
                    _nextAction = DateTimeOffset.Now.AddMilliseconds(Info.AttackSpeed);
                    DamageType type = DamageType.Miss;

                    var attack = MonsterAttack(out type, out Spell isMagic);
                    Target.Character
                        .GetAttacked(Index, Direction, 120, attack, type, isMagic, 0)
                        .Wait();
                    TPosition = Position;
                    return;
                }
                else if (dis > Info.ViewRange)
                {
                    _monsterState = MonsterState.Idle;
                    Target = null;
                    _nextAction = DateTimeOffset.Now.AddMilliseconds(Info.AttackSpeed);
                    return;
                }
                else
                {
                    TPosition = Target.Character.Position;
                }
            }
            if (_monsterState == MonsterState.Idle)
            {
                var possibleTarget1 = from plr in ViewPort
                                     let dist = Distance(plr.Character?.Position ?? new Point(), Position)
                                     where dist <= Info.ViewRange
                                     orderby dist ascending
                                     select plr;
                var possibleTarget2 = DamageSum
                    .Where(x => possibleTarget1.Contains(x.Key))
                    .OrderByDescending(x => x.Value);

                if(possibleTarget2.Any())
                {
                    Target = possibleTarget2.First().Key;
                    TPosition = Target.Character.Position;
                }
                else if(possibleTarget1.Any())
                {
                    Target = possibleTarget1.First();
                    TPosition = Target.Character.Position;
                }
                else
                {
                    var minX = Math.Max(Spawn.X - Info.MoveRange, 0);
                    var maxX = Math.Min(Spawn.X + Info.MoveRange, 255);
                    var minY = Math.Max(Spawn.Y - Info.MoveRange, 0);
                    var maxY = Math.Min(Spawn.Y + Info.MoveRange, 255);
                    var i = 0;
                    Point newPoint;
                    do {
                        newPoint = new Point(Program.RandomProvider(maxX, minX), Program.RandomProvider(maxY, minY));
                        if(!Map.ContainsAny(newPoint.X, newPoint.Y, _cantGo))
                        {
                            TPosition = newPoint;
                            return;
                        }
                    } while (i++ < 10);
                }                
            }
        }

        private void MakePath()
        {
            var pf = new PathFinding(Position, TPosition, Map, _cantGo);
            var fpt = TPosition;

            if (pf.FindPath())
            {
                _path = pf.GetPath();
                _path.RemoveAt(0);

                int count = 0;
                if (Target != null && Target.Character != null)
                {
                    foreach (var pt in _path)
                    {
                        count++;
                        var dis = Distance(Target.Character.Position, pt);
                        if (dis <= Info.AttackRange)
                        {
                            _TPosition = pt;
                            break;
                        }
                    }
                }
                else
                {
                    count = _path.Count;
                }

                var dx = fpt.X - TPosition.X;
                var dy = fpt.Y - TPosition.Y;
                dx = dx != 0 ? dx / Math.Abs(dx) : 0;
                dy = dy != 0 ? dy / Math.Abs(dy) : 0;

                Direction = _walkDirs[dy + 1, dx + 1];

                foreach (var obj in ViewPort.ToList())
                    obj.Session
                        .SendAsync(new SMove(Index, (byte)TPosition.X, (byte)TPosition.Y, Direction))
                        .Wait();

                _nextAction = DateTimeOffset.Now.AddMilliseconds(Info.MoveSpeed * count);
                _monsterState = MonsterState.Walking;
                return;
            }

            _TPosition = Position;
            _monsterState = MonsterState.Idle;
        }

        public int MonsterAttack(out DamageType type, out Spell isMagic)
        {
            var @char = Target.Character;
            var attack = 0;
            type = DamageType.Regular;
            isMagic = Info.Spell;

            if (!MissCheck())
            {
                type = DamageType.Miss;
                return 0;
            }

            if (Info.Spell != Spell.None)
            {
                SpellInfo si = ResourceCache.Instance.GetSkills()[Info.Spell];
                var baseAttack = _rand.Next(si.Damage.X + Info.DmgMin, si.Damage.Y + Info.DmgMax);
                type = DamageType.Regular;
                attack = baseAttack - @char.Defense;
            }
            else
            {
                var M = Math.Max(Info.DmgMin, Info.DmgMax);
                var m = Math.Min(Info.DmgMin, Info.DmgMax);
                var baseAttack = _rand.Next(m, M);
                attack = baseAttack - @char.Defense;
            }

            if (attack < 0)
                attack = 0;

            return attack;
        }

        private bool MissCheck()
        {
            var @char = Target.Character;

            if (Info.Success < @char.DefenseRatePvM)
            {
                if (_rand.Next(100) >= 5)
                {
                    return false;
                }
            }
            else
            {
                if (_rand.Next(Info.Success) < @char.DefenseRatePvM)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDying(object obj, EventArgs args)
        {
            gObjGiveItemSearch(Level);

            var die = new SDiePlayer(Index, 1, (ushort)(Killer?.Session?.ID ?? 0xffff));

            if (Killer?.Character == null)
            {
                return;
            }
            Killer.Character.Quests.OnMonsterDie(this);
            Killer.Character.PKTimeEnds = Killer.Character.PKTimeEnds.AddSeconds(Level*-1);
            Killer.Character.HuntingRecord.KilledMonster(this);

            var result = DamageSum.Where(x => x.Key.Status != LoginStatus.Playing).Select(x => x.Key);
            foreach (var r in result)
                DamageSum.Remove(r);

            foreach (var plr in ViewPort)
                plr.Session.SendAsync(die).Wait();

            var baseEXP = ((Level + 10) * Level) / 4;

            var partys = DamageSum
                .Where(x => x.Key.Character.Party != null)
                .GroupBy(x => x.Key.Character.Party);

            float Zen = 0;

            // Party EXP division based on WebZen
            foreach (var p in partys)
            {
                var dmg = p.Sum(x => x.Value);
                float EXP = baseEXP;
                if (Level + 10 < p.Key.MaxLevel)
                    EXP = EXP * (Level + 10) / p.Key.MaxLevel;

                if (EXP / 2.0f > 1.0f)
                    EXP += _rand.Next((int)(EXP / 2.0f));

                EXP *= dmg / MaxLife;
                Zen = EXP;
                EXP *= Program.Experience.FullExperate + 1.0f;
                Zen *= Program.Zen;

                p.Key.ExpDivision(Index, EXP, Killer, DeadlyDmg);
            }

            // Monster EXP division based on DMG excluding Partys
            foreach (var pair in DamageSum.Where(x => x.Key.Character.Party == null))
            {
                float EXP = baseEXP;
                if (Level + 10 < pair.Key.Character.Level)
                    EXP = EXP * (Level + 10) / pair.Key.Character.Level;

                if (EXP / 2.0f > 1.0f)
                    EXP += _rand.Next((int)(EXP / 2.0f));

                EXP *= Math.Min(pair.Value / MaxLife, 1.0f);

                if (pair.Key == Killer)
                    Zen = EXP * (1.0f + Killer.Character.Inventory.DropZen + Killer.Character.Spells.IncreaseZen);

                EXP *= Program.Experience.FullExperate + 1.0f + Killer.Character.Spells.IncreaseExperience;
                Zen *= Program.Zen;

                pair.Key.Character.Experience += (long)EXP;
                switch (Program.Season)
                {
                    case ServerSeason.Season9Eng:
                        pair.Key.Session
                            .SendAsync(new SKillPlayerEXT(Index, (int)EXP, pair.Key == Killer ? DeadlyDmg : (ushort)0))
                            .Wait();
                        break;
                    default:
                        pair.Key.Session
                            .SendAsync(new SKillPlayer(Index, (ushort)EXP, pair.Key == Killer ? DeadlyDmg : (ushort)0))
                            .Wait();
                        break;
                }

                var usedMana = pair.Key.Character.MaxMana - pair.Key.Character.Mana;
                var usedHealth = pair.Key.Character.MaxHealth - pair.Key.Character.Health;

                pair.Key.Character.Mana += usedMana * Killer.Character.Inventory.IncreaseManaRate;
                pair.Key.Character.Health += usedHealth * Killer.Character.Inventory.IncreaseLifeRate;
            }

            Item reward = null;
            if (Info.Bag != null)
            {
                var bag = Info.Bag as Bag;
                reward = bag.GetReward().FirstOrDefault();
            }

            if (reward == null)
            {
                if (_rand.Next(100) < Program.DropRate && CanDrop)
                {
                    if (_rand.Next(2) == 0)
                    {
                        reward = Program.GlobalEventsManager.GetItem(Level, MapID);

                        if (reward == null)
                            reward = Pentagrama.Drop(this);

                        if (reward == null && ItemBag.Count > 0)
                        {
                            reward = ItemBag[_rand.Next(ItemBag.Count)].Clone() as Item;
                            reward.NewOptionRand();
                        }
                    }

                    if (reward == null)
                        reward = Item.Zen((uint)Zen);
                }
            }

            Map.AddItem(Position.X, Position.Y, reward);
        }

        private void EnemyDie(object obj, EventArgs args)
        {
            Target = null;
        }

        private void gObjGiveItemSearch(int maxlevel)
        {
            if (ItemBag.Count == 100 || !CanDrop)
                return;

            var items = ResourceCache.Instance.GetItems();

            int[] BallTable = new int[17];
            ItemNumber itNum = new ItemNumber();

            BallTable[0] = 7;
            BallTable[1] = 8;
            BallTable[2] = 9;
            BallTable[3] = 10;
            BallTable[4] = 11;
            BallTable[5] = 12;
            BallTable[6] = 13;
            BallTable[7] = 14;
            BallTable[8] = 16;
            BallTable[9] = 17;
            BallTable[10] = 18;
            BallTable[11] = 19;
            BallTable[12] = 21;
            BallTable[13] = 22;
            BallTable[14] = 23;
            BallTable[15] = 24;
            BallTable[16] = 35;

            if (ItemBag.Count < 100)
            {
            start:
                if (_rand.Next(20) == 0)
                {
                    if (_rand.Next(2) != 0)
                    {
                        itNum.Type = ItemType.Scroll;
                        itNum.Index = (ushort)_rand.Next(_maxItemIndex[(int)itNum.Type] + 1);
                    }
                    else
                    {
                        itNum.Type = ItemType.Wing_Orb_Seed;
                        itNum.Index = (ushort)BallTable[_rand.Next(BallTable.Length)];
                    }
                }
                else
                {
                    itNum.Type = (ItemType)_rand.Next((int)ItemType.End);
                    itNum.Index = (ushort)_rand.Next(_maxItemIndex[(int)itNum.Type] + 1);

                    if (itNum.Type == ItemType.Scroll || (itNum.Type == ItemType.Wing_Orb_Seed && itNum.Index != 15))
                        goto start;
                }

                if (itNum.Type == ItemType.Missellaneo && itNum.Index == 3) //Horn of Dinorant
                    goto start;

                if ((itNum.Type == ItemType.Missellaneo && itNum.Index == 32) // Fenrrir Items
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 33)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 34)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 35)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 36)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 37))
                {
                    goto start;
                }

                if ((itNum.Type == ItemType.Potion && itNum.Index == 35) // Potion SD
                  || (itNum.Type == ItemType.Potion && itNum.Index == 36)
                  || (itNum.Type == ItemType.Potion && itNum.Index == 37)
                  || (itNum.Type == ItemType.Potion && itNum.Index == 38) // Potion Complex
                  || (itNum.Type == ItemType.Potion && itNum.Index == 39)
                  || (itNum.Type == ItemType.Potion && itNum.Index == 40))
                {
                    goto start;
                }

                if ((itNum.Type == ItemType.Missellaneo && itNum.Index < 8) || // Pets
                (itNum.Type == ItemType.Potion && (itNum.Index == 9 || itNum.Index == 10 || itNum.Index == 13 || itNum.Index == 14 || itNum.Index == 16 || itNum.Index == 17 || itNum.Index == 18 || itNum.Index == 22)) || // Misc
                (itNum.Type == ItemType.Wing_Orb_Seed && itNum.Index == 15) || // Jewel of Chaos
                (itNum.Type == ItemType.Missellaneo && itNum.Index == 14) || // Loch's Feather
                (itNum.Type == ItemType.Potion && itNum.Index == 31)) // Jewel of Guardian
                {
                    var perc = 0;
                    if (itNum.Type == ItemType.Wing_Orb_Seed && itNum.Index == 15) // Jewel of Chaos
                    {
                        if (Level >= 13 && Level <= 66) // 42%
                        {
                            perc = _rand.Next(7);

                            if (perc < 3)
                            {
                                perc = 0;
                            }
                        }
                        else
                        {
                            perc = 1;
                        }
                    }

                    if ((itNum.Type == ItemType.Potion && itNum.Index == 17) || // Devil Eye
                       (itNum.Type == ItemType.Potion && itNum.Index == 18))   // Devil Key
                    {
                        perc = 0;
                    }

                    if (perc == 0)
                    {
                        if (itNum.Type == ItemType.Potion && (itNum.Index == 17 || itNum.Index == 18))
                        {
                            byte Plus;

                            if (Level < 3)
                                Plus = 0;
                            else if (Level < 36)
                                Plus = 1;
                            else if (Level < 47)
                                Plus = 2;
                            else if (Level < 60)
                                Plus = 3;
                            else if (Level < 70)
                                Plus = 4;
                            else if (Level < 80)
                                Plus = 5;
                            else
                                Plus = 6;

                            ItemBag.Add(new Item(itNum, new { Plus }));
                        }
                        else
                        {
                            if (!items.ContainsKey(itNum))
                                goto start;

                            var it = items[itNum];
                            if (it.Level < Level)
                                ItemBag.Add(new Item(itNum));
                        }
                    }
                }
                else
                {
                    if (!items.ContainsKey(itNum))
                        goto start;

                    var it = new Item(itNum);
                    var result = it.GetLevel(Level);

                    if (result >= 0)
                    {
                        if ((it.Number.Type == ItemType.Missellaneo && it.Number.Index == 10) || (it.Number.Type == ItemType.Wing_Orb_Seed && it.Number.Index == 11))
                        {
                            it.Plus = result;
                            ItemBag.Add(it);
                        }
                        else if (result <= maxlevel)
                        {
                            if (it.Number.Type == ItemType.Wing_Orb_Seed)
                            {
                                if (it.Number.Index != 11)
                                    result = 0;
                            }

                            if (it.Number.Type == ItemType.Wing_Orb_Seed && it.Number.Index == 11)
                            {

                            }
                            else
                            {
                                if (result > maxlevel)
                                {
                                    result = (byte)maxlevel;
                                }
                            }

                            if ((it.Number.Type == ItemType.BowOrCrossbow && it.Number.Index == 7) || (it.Number.Type == ItemType.BowOrCrossbow && it.Number.Index == 15))
                                result = 0;

                            it.Plus = result;

                            ItemBag.Add(it);
                        }
                    }
                }
            }
        }

        public void Warp(Maps map, byte x, byte y)
        {
            var att = Map.GetAttributes(x, y);
            if(att.Contains(MapAttributes.NoWalk) || att.Contains(MapAttributes.Hide))
            {
                return;
            }

            Map.DelMonster(this);
            Map = ResourceCache.Instance.GetMaps()[map];
            Map.AddMonster(this);
            var msg = VersionSelector.CreateMessage<SMagicAttack>(Spell.Teleport, Index, Index);
            ViewPort.Where(x => x.Character.MonstersVP.Contains(Index)).SendAsync(msg).Wait();
            Position = new Point(x, y);
            _TPosition = new Point(x, y);
            UseTeleport = true;
        }
    }
}
