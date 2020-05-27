using MuEmu.Monsters;
using MuEmu.Network.Game;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Util;

namespace MuEmu.Resources.Map
{
    public enum MapAttributes : byte
    {
        Safe = 1,
        Stand = 2,
        NoWalk = 4,
        Hide = 8,
        Unknow = 16,
    }
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
    };
    public class MapInfo
    {
        private List<byte[]> _shadowLayer = new List<byte[]>();
        private byte[] Layer { get; }
        private List<Point> SafePoints { get; set; }

        public int Width { get; }
        public int Height { get; }
        public List<Monster> Monsters { get; }
        public List<Character> Players { get; }
        public List<ItemInMap> Items { get; }
        public int Map { get; }
        public byte Weather { get; set; }
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
                        if ((cell & 4) != 4 && (cell & 8) != 8)
                        {
                            SafePoints.Add(new Point(x, y));
                        }
                    }
                }
            }
            if (SafePoints.Any())
            {
                var rand = new Random().Next(SafePoints.Count());
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
        public event EventHandler MonsterAdd;
        
        public MapInfo(int map, string attFile)
        {
            using (var fr = File.OpenRead(attFile))
            {
                var type = fr.ReadByte();
                Width = fr.ReadByte();
                Height = fr.ReadByte();

                Layer = new byte[Width * Height];
                fr.Read(Layer, 0, Width * Height);

                Map = map;
            }

            Weather = 0x30;

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

        public bool ContainsAny(int X, int Y, MapAttributes[] attrs)
        {
            var info = Layer[Y * 256 + X];
            byte @byte = 0;
            foreach(var att in attrs)
            {
                @byte |= (byte)att;
            }

            return (info & @byte) != 0;
        }

        public MapAttributes[] GetAttributes(int X, int Y)
        {
            var info = Layer[Y*256 + X];
            var output = new List<MapAttributes>();
            foreach(var att in (MapAttributes[])Enum.GetValues(typeof(MapAttributes)))
            {
                if ((info & ((byte)att)) == ((byte)att))
                    output.Add(att);
            }

            return output.ToArray();
        }

        public MapAttributes[] GetAttributes(Point pt)
        {
            return GetAttributes(pt.X, pt.Y);
        }

        public void SetAttribute(int X, int Y, MapAttributes att)
        {
            Layer[Y * 256 + X] |= (byte)att;
        }

        public void ClearAttribute(int X, int Y, MapAttributes att)
        {
            Layer[Y * 256 + X] &= (byte)(~((byte)att));
        }

        public async void AddPlayer(Character @char)
        {
            await @char.Player.Session.SendAsync(new SWeather(Weather));

            var pos = @char.Position;
            SetAttribute(pos.X, pos.Y, MapAttributes.Stand);
            Players.Add(@char);
            PlayerJoins?.Invoke(@char.Player, new EventArgs());
        }

        public void AddMonster(Monster mons)
        {
            var pos = mons.Position;
            SetAttribute(pos.X, pos.Y, MapAttributes.Stand);
            Monsters.Add(mons);

            MonsterAdd?.Invoke(mons, new EventArgs());
        }

        public DateTimeOffset AddItem(int X, int Y, Item item)
        {
            var valid = DateTimeOffset.Now.AddSeconds(120);

            ItemInMap it = new ItemInMap {
                Index = (ushort)(Items.Count),
                Item = item,
                State = ItemState.Creating,
                Position = new Point(X, Y),
                validTime = valid,
            };

            if (Items.Count > 0)
            {
                try
                {
                    var fit = Items.First(x => x.State == ItemState.Deleted);
                    it.Index = fit.Index;
                    Items.Remove(fit);
                }
                catch(Exception) { }
            }

            Items.Add(it);

            //var pitem = new SViewPortItemCreate(new Network.Data.VPICreateDto[] { new Network.Data.VPICreateDto {
            //    ItemInfo = item.GetBytes(),
            //    wzNumber = ((ushort)(it.index | 0x8000)).ShufleEnding(),
            //    X = (byte)X,
            //    Y = (byte)Y,
            //} });
            //await SendAll(pitem);

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
            Monsters.Remove(mons);
            
        }

        public void PositionChanged(Point prev, Point current)
        {
            ClearAttribute(prev.X, prev.Y, MapAttributes.Stand);
            SetAttribute(current.X, current.Y, MapAttributes.Stand);
        }

        public async Task AddAttribute(MapAttributes att, Rectangle area)
        {
            for (int y = area.Top; y < area.Bottom; y++)
                for (int x = area.Left; x < area.Right; x++)
                    Layer[y * 256 + x] |= (byte)att;

            await SendAsync(new SSetMapAttribute(0, att, 1,
                new MapRectDto[] {
                    new MapRectDto {
                        StartX = (byte)area.Left, StartY = (byte)area.Top,
                        EndX = (byte)area.Right, EndY = (byte)area.Bottom
                    }
                }));
        }

        public async Task AddAttribute(MapAttributes att, Rectangle[] areas)
        {
            var result = new List<MapRectDto>();
            foreach (var area in areas)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                    for (int x = area.Left; x < area.Right; x++)
                        Layer[y * 256 + x] |= (byte)att;

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
            for(int y = area.Top; y < area.Bottom; y++)
                for(int x = area.Left; x < area.Right; x++)
                {
                    var info = Layer[y * 256 + x];
                    var invAtt = ~(int)att;
                    info &= (byte)invAtt;
                    Layer[y * 256 + x] = info;
                }

            await SendAsync(new SSetMapAttribute(0, att, 1, 
                new MapRectDto[] {
                    new MapRectDto {
                        StartX = (byte)area.Left, StartY = (byte)area.Top,
                        EndX = (byte)area.Right, EndY = (byte)area.Bottom
                    }
                }));
        }

        public async Task RemoveAttribute(MapAttributes att, Rectangle[] areas)
        {
            var result = new List<MapRectDto>();
            foreach (var area in areas)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                    for (int x = area.Left; x < area.Right; x++)
                    {
                        var info = Layer[y * 256 + x];
                        var invAtt = ~(int)att;
                        info &= (byte)invAtt;
                        Layer[y * 256 + x] = info;
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
    }
}
