using MuEmu.Resources;
using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MuEmu.Monsters
{
    public class Monster
    {
        private float _life;

        public ushort Index { get; set; }

        public ObjectState State { get; set; }
        public ObjectType Type { get; set; }
        public MonsterBase Info { get; }
        public float Life { get => _life;
            set
            {
                if (_life == value)
                    return;

                _life = value;
                if (_life == 0)
                {
                    Die?.Invoke(this, new EventArgs());
                    State = ObjectState.Dying;
                }
            }
        }
        public float Mana { get; set; }

        public Maps MapID { get; set; }
        public MapInfo Map { get; }
        public Point Position { get; set; }
        public byte Direction { get; set; }
        public List<Item> ItemBag { get; set; }

        public bool Active { get; set; }

        public int Attack => Info.Attack + (new Random().Next(Info.DmgMin,Info.DmgMax));
        public int Defense => Info.Defense;

        public event EventHandler Die;

        public Monster(ushort Monster, ObjectType type, Maps mapID, Point position, byte direction)
        {
            Type = type;
            MapID = mapID;
            Position = position;
            Direction = direction;
            Info = MonstersMng.Instance.MonsterInfo[Monster];
            Life = Info.HP;
            Mana = Info.MP;
            Map = ResourceCache.Instance.GetMaps()[MapID];
            Map.AddMonster(this);
            State = ObjectState.Regen;
        }
    }
}
