using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using MuEmu.Data;

namespace MuEmu.Monsters
{
    public class Monster
    {
        private static ushort[] _maxItemIndex;
        private static Random _rand;
        private float _life;
        private ObjectState _state;
        private DateTimeOffset _regen;
        private Player _target;

        public ushort Index { get; set; }

        public ObjectState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;

                if(value == ObjectState.Die)
                    _regen = DateTimeOffset.Now.AddSeconds(Info.RegenTime);

                _state = value;
            }
        }
        public ObjectType Type { get; set; }
        public MonsterBase Info { get; }
        public ushort Level => Info.Level;
        public float Life { get => _life;
            set
            {
                if (_life == value)
                    return;

                _life = value;
                if (_life <= 0)
                {
                    _life = 0;
                    Die?.Invoke(this, new EventArgs());
                    State = ObjectState.Dying;
                }
            }
        }
        public float MaxLife => Info.HP;
        public float Mana { get; set; }
        public float MaxMana => Info.MP;

        public Spells Spells { get; set; }

        public Maps MapID { get; set; }
        public MapInfo Map { get; }
        public Point Spawn { get; private set; }
        public Point Position { get; private set; }
        public Point TPosition { get; private set; }
        public Player Target {
            get => _target;
            set
            {
                if (_target == value)
                    return;

                if (_target != null)
                {
                    _target.Character.PlayerDie -= EnemyDie;
                    _target.Character.MapChanged -= EnemyDie;
                }

                _target = value;

                if (value == null)
                    return;

                _target.Character.PlayerDie += EnemyDie;
                _target.Character.MapChanged += EnemyDie;
            }
        }
        public List<Player> ViewPort { get; set; } = new List<Player>();
        public Player Killer { get; set; }
        public ushort DeadlyDmg { get; set; }
        public byte Direction { get; set; }
        public List<Item> ItemBag { get; set; }

        public bool Active { get; set; }
        public Dictionary<Player, int> DamageSum { get; private set; } = new Dictionary<Player, int>();

        public int Attack => Info.Attack + (_rand.Next(Info.DmgMin,Info.DmgMax));
        public int Defense => Info.Defense;

        public event EventHandler Die;

        public Monster(ushort Monster, ObjectType type, Maps mapID, Point position, byte direction)
        {
            Type = type;
            MapID = mapID;
            Spawn = position;
            Position = position;
            TPosition = position;
            Direction = direction;
            Info = MonstersMng.Instance.MonsterInfo[Monster];
            Life = Info.HP;
            Mana = Info.MP;
            Map = ResourceCache.Instance.GetMaps()[MapID];
            Map.AddMonster(this);
            State = ObjectState.Regen;
            Die += OnDie;
            Spells = new Spells(this);
            ItemBag = new List<Item>();
            if (_rand == null)
            {
                _rand = new Random();
                _maxItemIndex = new ushort[(int)ItemType.End];

                foreach(var t in Enum.GetValues(typeof(ItemType)))
                {
                    if ((ItemType)t == ItemType.End)
                        break;

                    _maxItemIndex[(int)(ItemType)t] = (ushort)ResourceCache.Instance.GetItems().Where(x => (new ItemNumber(x.Key)).Type == (ItemType)(t)).Count();
                }
            }

            //gObjGiveItemSearch(Level);
        }

        public async Task GetAttacked(Player plr, int dmg, DamageType type)
        {
            if (State != ObjectState.Live)
                return;

            if (dmg < 0)
                dmg = 0;

            if (DamageSum.ContainsKey(plr))
                DamageSum[plr] += dmg;
            else
                DamageSum.Add(plr, dmg);

            var dmgSend = dmg < ushort.MaxValue ? (ushort)dmg : ushort.MaxValue;
            DeadlyDmg = dmgSend;
            Killer = plr;
            Life -= dmg;

            if(State != ObjectState.Dying)
            {
                await plr.Session.SendAsync(new SAttackResult(Index, dmgSend, type, 0));
            }
        }

        public void GetAttackedDelayed(Player plr, int dmg, DamageType type, TimeSpan delay)
        {
            if (State != ObjectState.Live)
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
                //plr.Session.SendAsync(new SAttackResult(Index, dmgSend, type, 0));
                SubSystem.Instance.AddDelayedMessage(plr, TimeSpan.FromMilliseconds(100), new SAttackResult(Index, dmgSend, type, 0));
            }
        }

        public void TryRegen()
        {
            if (_regen > DateTimeOffset.Now)
                return;

            Life = MaxLife;
            Mana = MaxMana;
            Position = Spawn;
            TPosition = Spawn;
            ViewPort.Clear();
            Target = null;
            Killer = null;
            DeadlyDmg = 0;
            State = ObjectState.Regen;
        }

        public void Update()
        {
            if (Type == ObjectType.NPC)
                return;

            var movRange = Info.MoveRange;
            var movSpeed = Info.MoveSpeed;

            var pt = Position;
            pt.Offset(15, 15);

            //ViewPort = (from plr in Map.Players
            //            let rect = new Rectangle(plr.Position, new Size(30, 30))
            //            where rect.Contains(pt)
            //            select plr.Player).ToList();

            var validTargets = from plr in ViewPort
                               from dmg in DamageSum
                               where plr == dmg.Key
                               select dmg;

            if (validTargets.Any())
            {
                var max = validTargets.Select(x => x.Value).Max();
                Target = validTargets.Where(x => x.Value == max).Select(x => x.Key).FirstOrDefault();
            }

            if (TPosition == Position)
            {
                if (Target != null)
                {
                    var x = Target.Character.Position.X - Position.X;
                    var y = Target.Character.Position.Y - Position.Y;

                    var d = Math.Sqrt(x * x + y * y);
                    if (d > Info.AttackRange)
                    {
                        x = x > 0 ? x - 1 : (x < 0 ? x + 1 : 0);
                        y = y > 0 ? y - 1 : (y < 0 ? y + 1 : 0);

                        x += Position.X;
                        y += Position.Y;

                        TPosition = new Point(x, y);
                    }
                } else
                {
                    var rand = new Random();
                    var x = rand.Next(movRange);
                    var y = rand.Next(movRange);
                    TPosition = new Point(Spawn.X+x, Spawn.Y+y);
                }
            }

            var dx = TPosition.X - Position.X;
            var dy = TPosition.Y - Position.Y;

            // 0 1 2
            // 7 X 3
            // 6 5 4

            var dir = new byte[3, 3]
            {
                { 0, 1, 2 },
                { 7, 8, 3 },
                { 6, 5, 4 },
            };
            

            dx = dx != 0 ? dx / Math.Abs(dx) : 0;
            dy = dy != 0 ? dy / Math.Abs(dy) : 0;

            if (dx == 0 && dy == 0)
            {
                if(Target != null)
                {
                    var x = Target.Character.Position.X - Position.X;
                    var y = Target.Character.Position.Y - Position.Y;

                    var d = Math.Sqrt(x * x + y * y);

                    if(d <= Info.AttackRange)
                    {
                        DamageType type = DamageType.Miss;
                        Spell isMagic;
                        var attack = MonsterAttack(out type, out isMagic);
                        Target.Character.GetAttacked(this, attack, type, isMagic);
                    }
                }
                return;
            }

            Position = new Point(Position.X+dx /** movSpeed*/, Position.Y+dy /** movSpeed*/);

            foreach(var obj in ViewPort)
                obj.Session.SendAsync(new SMove(Index, (byte)TPosition.X, (byte)TPosition.Y, dir[dy+1, dx+1]));
        }

        private int MonsterAttack(out DamageType type, out Spell isMagic)
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
                var baseAttack = _rand.Next(Info.DmgMin, Info.DmgMax);
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

        private void OnDie(object obj, EventArgs args)
        {
            gObjGiveItemSearch(Level);

            var die = new SDiePlayer(Index, 1, (ushort)Killer.Session.ID);
            foreach (var plr in ViewPort)
                plr.Session.SendAsync(die);


            foreach (var pair in DamageSum)
            {
                float EXP = ((Level + 10) * Level) / 4;
                if (Level + 10 < pair.Key.Character.Level)
                    EXP = EXP * (Level + 10) / pair.Key.Character.Level;

                if (EXP / 2.0f > 1.0f)
                    EXP += _rand.Next((int)(EXP / 2.0f));

                EXP *= pair.Value / MaxLife;
                var Zen = EXP;
                EXP *= Program.Experience;
                Zen *= Program.Zen;

                pair.Key.Character.Experience += (ulong)EXP;

                if (EXP > ushort.MaxValue)
                    EXP = ushort.MaxValue;

                //SubSystem.Instance.AddDelayedMessage(pair.Key, TimeSpan.FromMilliseconds(1000), new SKillPlayer(Index, (ushort)EXP, pair.Key == Killer ? DeadlyDmg : (ushort)0));
                pair.Key.Session.SendAsync(new SKillPlayer(Index, (ushort)EXP, pair.Key == Killer ? DeadlyDmg : (ushort)0));

                Item reward;
                if (_rand.Next(100) < Program.DropRate)
                {
                    if (_rand.Next(2) == 0 && ItemBag.Count > 0)
                    {
                        reward = ItemBag[_rand.Next(ItemBag.Count)];
                    } else
                    {
                        reward = Item.Zen((uint)Zen);
                    }

                    Map.AddItem(Position.X, Position.Y, reward);
                }
            }

            DamageSum.Clear();
        }

        private void EnemyDie(object obj, EventArgs args)
        {
            Target = null;
        }

        private void gObjGiveItemSearch(int maxlevel)
        {
            if (ItemBag.Count != 0)
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

            while(ItemBag.Count < 1000)
            {
                if(_rand.Next(20) == 0)
                {
                    if(_rand.Next(2) == 0)
                    {
                        itNum.Type = ItemType.Scroll;
                        itNum.Index = (ushort)_rand.Next(_maxItemIndex[(int)itNum.Type]+1);
                    }else
                    {
                        itNum.Type = ItemType.Wing_Orb_Seed;
                        itNum.Index = (ushort)BallTable[_rand.Next((int)ItemType.End)];
                    }
                }else
                {
                    itNum.Type = (ItemType)_rand.Next((int)ItemType.End);
                    itNum.Index = (ushort)_rand.Next(_maxItemIndex[(int)itNum.Type] + 1);

                    if (itNum.Type == ItemType.Scroll || (itNum.Type == ItemType.Wing_Orb_Seed && itNum.Index == 15))
                        continue;
                }

                if (itNum.Type == ItemType.Missellaneo && itNum.Index == 3) //Horn of Dinorant
                    continue;

                if ((itNum.Type == ItemType.Missellaneo && itNum.Index == 32) // Fenrrir Items
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 33)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 34)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 35)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 36)
                      || (itNum.Type == ItemType.Missellaneo && itNum.Index == 37))
                {
                    continue;
                }

                if ((itNum.Type == ItemType.Potion && itNum.Index == 35) // Potion SD
                  || (itNum.Type == ItemType.Potion && itNum.Index == 36)
                  || (itNum.Type == ItemType.Potion && itNum.Index == 37)
                  || (itNum.Type == ItemType.Potion && itNum.Index == 38) // Potion Complex
                  || (itNum.Type == ItemType.Potion && itNum.Index == 39)
                  || (itNum.Type == ItemType.Potion && itNum.Index == 40))
                {
                    continue;
                }

                if ((itNum.Type == ItemType.Missellaneo && itNum.Index < 8) || // Pets
                ((itNum.Type == ItemType.Potion) && (itNum.Index == 9 || itNum.Index == 10 || itNum.Index == 13 || itNum.Index == 14 || itNum.Index == 16 || itNum.Index == 17 || itNum.Index == 18 || itNum.Index == 22)) || // Misc
                (itNum.Type == ItemType.Wing_Orb_Seed && itNum.Index == 15) || // Jewel of Chaos
                (itNum.Type == ItemType.Missellaneo && itNum.Index == 14) || // Loch's Feather
                (itNum.Type == ItemType.Potion && itNum.Index == 31)) // Jewel of Guardian
                {
                    var perc = 0;
                    if(itNum.Type == ItemType.Wing_Orb_Seed && itNum.Index == 15) // Jewel of Chaos
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

                    if((itNum.Type == ItemType.Potion && itNum.Index == 17) || // Devil Eye
                       (itNum.Type == ItemType.Potion && itNum.Index == 18))   // Devil Key
                    {
                        perc = 0;
                    }

                    if(perc == 0)
                    {
                        if(itNum.Type == ItemType.Potion && (itNum.Index == 17 || itNum.Index == 18))
                        {
                            byte Plus = 0;

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

                            ItemBag.Add(new Item(itNum, 0, new { Plus }));
                        }
                        else
                        {
                            if (!items.ContainsKey(itNum))
                                continue;

                            var it = items[itNum];
                            if (it.Level < Level)
                                ItemBag.Add(new Item(itNum));
                        }
                    }
                }
                else
                {
                    if (!items.ContainsKey(itNum))
                        continue;

                    var it = new Item(itNum);
                    var result = it.GetLevel(Level);

                    if(result >= 0)
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
    }
}
