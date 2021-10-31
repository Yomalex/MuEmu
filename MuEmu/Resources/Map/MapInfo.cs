using MuEmu.Monsters;
using MU.Network.Game;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using WebZen.Util;
using MU.Resources;
using MuEmu.Network.Data;

namespace MuEmu.Resources.Map
{
    public enum ItemState : byte
    {
        Creating,
        Created,
        Deleting,
        Deleted,
    }

    public class ItemInMap
    {
        public ushort Index { get; set; }
        public ItemState State { get; set; }
        public Point Position { get; set; }
        public Item Item { get; set; }
        public DateTimeOffset validTime { get; set; }

        public Character Character { get; set; }
        public DateTimeOffset OwnedTime { get; set; }
    };
    public class MapInfo
    {
        private List<MapAttributes[]> _shadowLayer = new List<MapAttributes[]>();
        private DateTime _nextWeater;

        internal Item ItemPickUp(Character @char, ushort number)
        {
            var item = (from obj in @char.Map.Items
                        where obj.Index == number && obj.State == ItemState.Created
                        select obj).FirstOrDefault();

            if(item == null)
            {
                throw new Exception("Invalid item");
            }

            if (
                item.Item.Number != ItemNumber.Zen && 
                item.Character != null && 
                item.Character != @char && 
                item.OwnedTime > DateTimeOffset.Now
                )
            {
                throw new Exception("This item does not belong to you");
            }

            var msg = new SViewPortItemDestroy { ViewPort = new VPDestroyDto[] { new VPDestroyDto(item.Index) } };
            var session = @char.Player.Session;
            _ = session.SendAsync(msg);
            @char.SendV2Message(msg);
            item.State = ItemState.Deleting;
            return item.Item;
        }

        private MapAttributes[] Layer { get; }
        private List<Point> SafePoints { get; set; }
        private IEnumerable<Monster> NPC => Monsters.Where(x => x.Type == ObjectType.NPC);

        public int Width { get; }
        public int Height { get; }
        public List<Monster> Monsters { get; }
        public List<Character> Players { get; }
        public List<ItemInMap> Items { get; }
        public int Map { get; }

        public bool IsEvent { get; }
        public byte Weather { get; set; }
        public bool DragonInvasion { get; set; }
        public Rectangle SafeArea { get; private set; }
        public Point GetRespawn()
        {
            if (SafePoints == null)
            {
                SafePoints = new List<Point>();
                for (var y = SafeArea.Y; y < SafeArea.Bottom; y++)
                {
                    for (var x = SafeArea.X; x < SafeArea.Right; x++)
                    {
                        var cell = Layer[x + y * 256];
                        if ((cell & (MapAttributes.NoWalk | MapAttributes.Hide)) == 0)
                        {
                            SafePoints.Add(new Point(x, y));
                        }
                    }
                }
            }
            if (SafePoints.Any())
            {
                var rand = Program.RandomProvider(SafePoints.Count());
                var id = SafePoints[rand];
                return id;
            }
            else
            {
                var p = ResourceCache.Instance.GetGates().Values
                    .First(x => x.Map == (Maps)Map)
                    .Door.Location;

                SafePoints.Add(p);
            }

            return new Point();
        }

        public event EventHandler PlayerJoins;
        public event EventHandler PlayerLeaves;

        /// <summary>
        /// On Monster added to map Invoke event with sender as monster
        /// </summary>
        public event EventHandler MonsterAdd;
        
        public MapInfo(int map, string attFile)
        {
            Maps[] disabled = new Maps[]
            {
                Maps.BloodCastle1,
                Maps.BloodCastle2,
                Maps.BloodCastle3,
                Maps.BloodCastle4,
                Maps.BloodCastle5,
                Maps.BloodCastle6,
                Maps.BloodCastle7,
                Maps.BloodCastle8,
                Maps.DevilSquare,
                Maps.DevilSquare2,
                Maps.ChaosCastle1,
                Maps.ChaosCastle2,
                Maps.ChaosCastle3,
                Maps.ChaosCastle4,
                Maps.ChaosCastle5,
                Maps.ChaosCastle6,
                Maps.ChaosCastle7,
            };

            IsEvent = disabled.Any(x => (int)x == map);

            using (var fr = File.OpenRead(attFile))
            {
                var length = fr.Length;
                var type = fr.ReadByte();
                Width = fr.ReadByte();
                Height = fr.ReadByte();
                length -= fr.Position;

                var tmp = new byte[length];
                fr.Read(tmp, 0, tmp.Length);
                Layer = tmp.Select(x => (MapAttributes)x).ToArray();

                Map = map;
            }

            Weather = 0x30;
            _nextWeater = DateTime.Now.AddMilliseconds(Program.RandomProvider(10000) + 10000);

            Monsters = new List<Monster>();
            Players = new List<Character>();
            Items = new List<ItemInMap>();
            switch((Maps)Map)
            {
                case Maps.Lorencia:
                    SafeArea = new Rectangle(130, 116, 21, 21);
                    break;
                case Maps.Dugeon:
                    SafeArea = new Rectangle(106, 236, 6, 7);
                    break;
                case Maps.Davias:
                    SafeArea = new Rectangle(197, 35, 21, 15);
                    break;
                case Maps.Noria:
                    SafeArea = new Rectangle(174, 101, 13, 24);
                    break;
                case Maps.LostTower:
                    SafeArea = new Rectangle(201, 70, 12, 11);
                    break;
                case Maps.Atlans:
                    SafeArea = new Rectangle(14, 11, 13, 12);
                    break;
                case Maps.Tarkan:
                    SafeArea = new Rectangle(187, 54, 16, 15);
                    break;
                case Maps.Aida:
                    SafeArea = new Rectangle(82, 8, 5, 6);
                    break;
                //case Maps.Barracks:
                //    SafeArea = new Rectangle(30, 75, 33, 78);
                //    break;
                case Maps.Elbeland:
                    SafeArea = new Rectangle(50, 220, 6, 6);
                    break;
                case Maps.SilentSwamp:
                    SafeArea = new Rectangle(135, 105, 10, 10);
                    break;
                case Maps.Raklion:
                    SafeArea = new Rectangle(220, 210, 13, 2);
                    break;
                case Maps.Vulcan:
                    SafeArea = new Rectangle(110, 120, 15, 15);
                    break;
                case Maps.Kantru1:
                    SafeArea = new Rectangle(124, 123, 3, 2);
                    break;
                case Maps.Kantru2:
                    SafeArea = new Rectangle(162, 16, 1, 1);
                    break;
                default:
                    SafeArea = new Rectangle(0, 0, 255, 255);
                    break;
            }
        }

        private MapAttributes GetByte(int X, int Y)
        {
            if (Y * 256 + X > Layer.Length)
                return MapAttributes.Unknow;

            return Layer[Y * 256 + X];
        }
        private void SetByte(int X, int Y, MapAttributes val)
        {
            Layer[Y * 256 + X] = val;
        }

        public bool ContainsAny(int X, int Y, MapAttributes[] attrs)
        {
            var info = GetByte(X, Y);
            MapAttributes @byte = 0;
            foreach(var att in attrs)
                @byte |= att;

            return (info & @byte) != 0;
        }

        public MapAttributes[] GetAttributes(int X, int Y)
        {
            var info = GetByte(X, Y);
            var in_ = (MapAttributes[])Enum.GetValues(typeof(MapAttributes));
            return in_.Where(x => (x & info) != 0).ToArray();
        }

        public MapAttributes[] GetAttributes(Point pt)
        {
            return GetAttributes(pt.X, pt.Y);
        }

        public void SetAttribute(int X, int Y, MapAttributes att)
        {
            SetByte(X, Y, (GetByte(X, Y) | att));
        }

        public void ClearAttribute(int X, int Y, MapAttributes att)
        {
            SetByte(X, Y, (GetByte(X, Y) & (~att)));
        }

        public void AddPlayer(Character @char)
        {
            SendMinimapInfo(@char);
            SendWeather(@char);
            var pos = @char.Position;
            SetAttribute(pos.X, pos.Y, MapAttributes.Stand);
            Players.Add(@char);
            PlayerJoins?.Invoke(@char.Player, new EventArgs());
        }

        public async void SendMinimapInfo(Character @char)
        {
            byte i = 0;
            var npcs = ResourceCache.Instance.GetNPCs();
            var gates = ResourceCache.Instance.GetGates();
            var mapGates = gates
                .Where(x => x.Value.Map == (Maps)Map && x.Value.GateType == GateType.Entrance)
                .Select(x => x.Value);

            foreach (var npc in NPC)
            {
                var icon = MiniMapTag.Shield;
                if(npcs.TryGetValue(npc.Info.Monster, out var npcInfo))
                {
                    icon = npcInfo.Icon;
                }
                await @char.Player.Session.SendAsync(new SMiniMapNPC(npc.Position, i++, icon, 0, npc.Info.Name));
            }
            foreach (var gate in mapGates)
            {
                var target = gates[gate.Target].Map;
                await @char.Player.Session.SendAsync(new SMiniMapNPC(gate.Door, i++, MiniMapTag.Shield, 0, target));
            }
        }

        public void SendWeather(Character @char)
        {
            SubSystem.Instance.AddDelayedMessage(@char.Player, TimeSpan.FromSeconds(1), new SWeather(Weather));
            SubSystem.Instance.AddDelayedMessage(@char.Player, TimeSpan.FromSeconds(1), new SEventState(MapEvents.GoldenInvasion, DragonInvasion));
        }

        public void AddMonster(Monster mons)
        {
            var pos = mons.Position;
            SetAttribute(pos.X, pos.Y, MapAttributes.Stand);
            lock(Monsters)
                Monsters.Add(mons);

            MonsterAdd?.Invoke(mons, new EventArgs());
        }

        public DateTimeOffset AddItem(int X, int Y, Item item, Character character = null)
        {
            if (item == null)
                return DateTimeOffset.Now;

            item.Character = null;
            item.Account = null;
            item.SlotId = 0;
            item.Storage = 0;
            item.NeedSave = false;

            var valid = DateTimeOffset.Now.AddSeconds(120);
            var own = DateTimeOffset.Now.AddSeconds(60);

            ItemInMap it = new ItemInMap {
                Index = (ushort)(Items.Count),
                Item = item,
                State = ItemState.Creating,
                Position = new Point(X, Y),
                validTime = valid,
                Character = character,
                OwnedTime = own,
            };

            lock(Items)
                Items.Add(it);

            return valid;
        }
        
        public void DelPlayer(Character @char)
        {
            var pos = @char.Position;
            ClearAttribute(pos.X, pos.Y, MapAttributes.Stand);
            Players.Remove(@char);
            PlayerLeaves?.Invoke(@char.Player, new EventArgs());
        }
        public void DelMonster(Monster mons)
        {
            var pos = mons.Position;
            ClearAttribute(pos.X, pos.Y, MapAttributes.Stand);
            lock(Monsters)
                Monsters.Remove(mons);
            
        }

        public void PositionChanged(Point prev, Point current)
        {
            ClearAttribute(prev.X, prev.Y, MapAttributes.Stand);
            SetAttribute(current.X, current.Y, MapAttributes.Stand);
        }

        public async Task AddAttribute(MapAttributes att, Rectangle area)
        {
            await AddAttribute(att, new Rectangle[] { area });
        }

        public async Task AddAttribute(MapAttributes att, Rectangle[] areas)
        {
            var result = new List<MapRectDto>();
            foreach (var area in areas)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                    for (int x = area.Left; x < area.Right; x++)
                        SetAttribute(x, y, att);

                result.Add(new MapRectDto
                {
                    StartX = (byte)area.Left,
                    StartY = (byte)area.Top,
                    EndX = (byte)area.Right,
                    EndY = (byte)area.Bottom
                });
            }

            await SendAsync(new SSetMapAttribute(0, att, 1, result.ToArray()));
        }

        public async Task RemoveAttribute(MapAttributes att, Rectangle area)
        {
            await RemoveAttribute(att, new Rectangle[] { area });
        }

        public async Task RemoveAttribute(MapAttributes att, Rectangle[] areas)
        {
            var result = new List<MapRectDto>();
            foreach (var area in areas)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                    for (int x = area.Left; x < area.Right; x++)
                    {
                        ClearAttribute(x, y, att);
                    }

                result.Add(new MapRectDto
                {
                    StartX = (byte)area.Left,
                    StartY = (byte)area.Top,
                    EndX = (byte)area.Right,
                    EndY = (byte)area.Bottom
                });
            }

            await SendAsync(new SSetMapAttribute(0, att, 1, result.ToArray()));
        }

        public async Task SendAsync(object message)
        {
            foreach(var @char in Players)
                await @char.Player.Session.SendAsync(message);
        }

        public void Push()
        {
            _shadowLayer.Add(Layer.ToArray());
        }

        public void Pop()
        {
            var last = _shadowLayer.PopBack();
            Array.Copy(last, Layer, Layer.Length);
        }

        public async Task WeatherUpdate()
        {
            if (DateTime.Now < _nextWeater)
                return;

            _nextWeater = DateTime.Now.AddMilliseconds(Program.RandomProvider(10000) + 10000);

            Weather = (byte)(Program.RandomProvider(3) << 4 | Program.RandomProvider(10));
            await SendAsync(new SWeather(Weather));
        }
    }
}
