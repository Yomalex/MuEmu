using MuEmu.Monsters;
using MuEmu.Network.Game;
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
        Unk1 = 1,
        Stand = 2,
        NoWalk = 4,
        Hide = 8,
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
        private byte[] Layer { get; }

        public int Width { get; }
        public int Height { get; }
        public List<Monster> Monsters { get; }
        public List<Character> Players { get; }
        public List<ItemInMap> Items { get; }
        public int Map { get; }
        public byte Weather { get; set; }

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
            Players.Remove(@char);
            PlayerLeaves?.Invoke(@char.Player, new EventArgs());
        }

        public void PositionChanged(Point prev, Point current)
        {
            ClearAttribute(prev.X, prev.Y, MapAttributes.Stand);
            SetAttribute(current.X, current.Y, MapAttributes.Stand);
        }

        public async Task SendAll(object message)
        {
            foreach(var plr in Players)
            {
                await plr.Player.Session.SendAsync(message);
            }
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
    }
}
