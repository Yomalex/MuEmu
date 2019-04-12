using MuEmu.Network.Game;
using MuEmu.Resources;
using MuEmu.Resources.Map;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu.Monsters
{
    public class Monster
    {
        private static Random _rand;
        private float _life;
        private ObjectState _state;
        private DateTimeOffset _regen;

        public ushort Index { get; set; }

        public ObjectState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;

                if(value == ObjectState.Die)
                    _regen = DateTimeOffset.Now.AddMilliseconds(Info.RegenTime);

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

        public Spells Spells { get; set; }

        public Maps MapID { get; set; }
        public MapInfo Map { get; }
        public Point Spawn { get; private set; }
        public Point Position { get; private set; }
        public Point TPosition { get; private set; }
        public Player Target { get; private set; }
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
            if (_rand == null)
                _rand = new Random();
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
                plr.Session.SendAsync(new SAttackResult(Index, dmgSend, type, 0));
            }
        }

        public void TryRegen()
        {
            if (_regen > DateTimeOffset.Now)
                return;

            Life = MaxLife;
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

                    x = x > 0 ? x - 1 : (x < 0 ? x + 1 : 0);
                    y = y > 0 ? y - 1 : (y < 0 ? y + 1 : 0);

                    x += Position.X;
                    y += Position.Y;

                    TPosition = new Point(x, y);
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
                return;

            Position = new Point(Position.X+dx /** movSpeed*/, Position.Y+dy /** movSpeed*/);

            foreach(var obj in ViewPort)
                obj.Session.SendAsync(new SMove(Index, (byte)TPosition.X, (byte)TPosition.Y, dir[dy+1, dx+1]));
        }

        private void OnDie(object obj, EventArgs args)
        {
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

                pair.Key.Character.Experience += (ulong)EXP;

                if (EXP > ushort.MaxValue)
                    EXP = ushort.MaxValue;

                SubSystem.Instance.AddDelayedMessage(pair.Key, TimeSpan.FromMilliseconds(1000), new SKillPlayer(Index, (ushort)EXP, pair.Key == Killer ? DeadlyDmg : (ushort)0));
                Map.AddItem(Position.X, Position.Y, Item.Zen((uint)EXP));
            }

            DamageSum.Clear();
        }
    }
}
