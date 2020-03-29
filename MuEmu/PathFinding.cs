using MuEmu.Resources.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class PathFinding
    {
        private Point _start;
        private Point _end;
        private List<Tile> _tiles = new List<Tile>();
        private MapInfo _map;
        private MapAttributes[] _cantWalkOn;
        private Tile _subtile;
        public PathFinding(Point start, Point end, MapInfo map, MapAttributes[] cantWalkOn = null)
        {
            _start = start;
            _end = end;
            _map = map;
            _cantWalkOn = cantWalkOn;
            if (_cantWalkOn == null)
                _cantWalkOn = new MapAttributes[] { MapAttributes.Hide, MapAttributes.NoWalk };
        }

        public bool FindPath()
        {
            var tile = new Tile(_start);
            _subtile = SubTile(tile);
            return _subtile != null;
        }

        private Tile SubTile(Tile tile)
        {
            tile.Closed = true;

            for(var y = Math.Max(tile.Position.Y - 1, 0); y <= Math.Min(tile.Position.Y + 1, 255); y++)
            {
                for (var x = Math.Max(tile.Position.X - 1, 0); x <= Math.Min(tile.Position.X + 1, 255); x++)
                {
                    if (_map.ContainsAny(x, y, _cantWalkOn))
                        continue;
                    if (x == tile.Position.X && y == tile.Position.Y)
                        continue;

                    var H = (Math.Abs(_end.X - x) + Math.Abs(_end.Y - y)) * 10;
                    var stile = new Tile(new Point(x,y), tile, H);

                    if (stile.F == Tile.MaxF)
                        return null;

                    if (H == 0)
                        return stile;

                    var exist = _tiles.Any(f => f.Position.X == x && f.Position.Y == y);
                    if(!exist)
                    {
                        _tiles.Add(stile);
                    }
                }
            }

            foreach(var otile in _tiles.Where(x => !x.Closed).ToList())
            {
                var rtile = SubTile(otile);
                if (rtile != null)
                    return rtile;
            }

            return null;
        }

        public List<Point> GetPath()
        {
            var rList = new List<Point>();
            var endTile = _subtile;

            do
            {
                rList.Add(endTile.Position);
                var next = endTile.Parent;
                endTile = next;
            } while (endTile != null);
            rList.Reverse();

            return rList;
        }
    }

    class Tile
    {
        public const int MaxF = 100; //25555
        public int H { get; private set; }
        public int G { get; private set; }
        public int F => Math.Min(G + H, MaxF);
        public Point Position { get; private set; }
        public Tile Parent { get; private set; }
        public bool Closed { get; set; }
        public Tile(Point pt, Tile parent = null, int h = 0)
        {
            Position = pt;
            H = h;
            Parent = parent;

            if(parent != null)
                G = parent.G + ((pt.Y == parent.Position.Y || pt.X == parent.Position.X) ? 10 : 14);
        }
    }
}
