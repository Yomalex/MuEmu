using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MuEmu.Resources.Map
{
    public enum MapAttributes
    {
        Unk1 = 1,
        Stand = 2,
        NoWalk = 4,
        Hide = 8,
    }
    public class MapInfo
    {
        public int Width { get; }
        public int Height { get; }
        public List<Character> Players { get; }
        public List<Item> Items { get; }
        private byte[] Layer { get; }
        public int Map { get; }
        public byte Weather { get; set; }
        
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
        }

        public void PositionChanged(Point prev, Point current)
        {
            ClearAttribute(prev.X, prev.Y, MapAttributes.Stand);
            SetAttribute(current.X, current.Y, MapAttributes.Stand);
        }
    }
}
