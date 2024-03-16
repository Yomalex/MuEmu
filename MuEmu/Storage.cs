using MU.Resources;
using MuEmu.Entity;
using MuEmu.Network.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Storage
    {
        public const int WarehouseSize = 8 * 15;
        public const int ShopSize = 8 * 15;
        public const int TradeSize = 8 * 5;
        public const int ChaosBoxSize = 8 * 5;
        public const int InventorySize = 8 * 8;

        private Dictionary<byte, Item> _items;
        private RectangleF _bounds;
        private List<RectangleF> _map;

        public bool NeedSave { get; set; }
        public StorageID IndexTranslate { get; private set; }
        public StorageID StorageID { get; set; }
        public int EndIndex { get; private set; }
        public int Size { get; private set; }
        public uint Money { get; set; }
        public Dictionary<byte, Item> Items => _items;

        public const int Expansion = 4 * 8;

        public Storage(int size, StorageID startIndex = StorageID.Equipament)
        {
            Size = size;
            _items = new Dictionary<byte, Item>();
            _bounds = new RectangleF(new Point(0, 0), new SizeF(8, Size / 8));
            _map = new List<RectangleF>();
            IndexTranslate = startIndex;
            StorageID = startIndex;
            EndIndex = (int)startIndex + size;
        }

        public bool NoMapped { get; internal set; }

        public byte Add(Item it, byte offset = 0)
        {
            if (it.BasicInfo.MaxStack != 0)
            {
                var firts = (from r in _items.Values.Where(x => x.Number == it.Number)
                             where r.Plus == it.Plus && r.Durability < it.BasicInfo.MaxStack
                             select r).FirstOrDefault();
                if (firts != null)
                {
                    if(firts.Overlap(it) == null)
                        return 0xfd;
                }
            }
            for (var i = offset; i < Size; i++)
            {
                var itemRect = new RectangleF(new Point(i % 8, i / 8), it.BasicInfo.Size);
                itemRect.Width -= 0.1f;
                itemRect.Height -= 0.1f;
                if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                    continue;

                if (NoIntersects(i, it.BasicInfo.Size).Width == 0)
                {
                    _add(i, it);
                    NeedSave = true;
                    it.SlotId = (int)IndexTranslate + i;
                    it.Storage = StorageID;
                    return (byte)it.SlotId;
                }
            }

            return 0xff;
        }

        private RectangleF NoIntersects(byte offset, Size freeSpace)
        {
            if (NoMapped)
                return _items.ContainsKey(offset)? _bounds : default(RectangleF);

            var itemRect = new RectangleF(new Point(offset % 8, offset / 8), freeSpace);
            itemRect.Width -= 0.1f;
            itemRect.Height -= 0.1f;

            if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                return _bounds;

            return _map.FirstOrDefault(x => x.IntersectsWith(itemRect));
        }

        public bool TryAdd(Size freeSpace, byte offset = 0)
        {
            for (var i = offset; i < Size; i++)
            {
                var itemRect = new RectangleF(new Point(i % 8, i / 8), freeSpace);
                itemRect.Width -= 0.1f;
                itemRect.Height -= 0.1f;
                if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                    continue;

                if (_map.Where(x => x.IntersectsWith(itemRect)).Count() == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryAdd(Size[] sizes, byte offset = 0)
        {
            var mapCopy = _map.ToList();

            var ret = true;
            foreach (var freeSpace in sizes)
            {
                var result = false;
                for (var i = offset; i < Size; i++)
                {
                    var itemRect = new RectangleF(new Point(i % 8, i / 8), freeSpace);
                    itemRect.Width -= 0.1f;
                    itemRect.Height -= 0.1f;
                    if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                        continue;

                    if (mapCopy.Where(x => x.IntersectsWith(itemRect)).Count() == 0)
                    {
                        mapCopy.Add(new RectangleF(new Point(i % 8, i / 8), freeSpace));
                        result = true;
                    }
                }
                ret &= result;
            }
            return ret;
        }

        private void _add(byte pos, Item it)
        {
            _items.Add(pos, it);
            if(!NoMapped)
                _map.Add(new RectangleF(new Point(pos % 8, pos / 8), it.BasicInfo.Size));
        }

        public void Add(byte pos, Item it)
        {
            var org = pos;
            pos -= (byte)IndexTranslate;

            if (_items.ContainsKey(pos))
            {
                if(_items[pos].Overlap(it) == null)
                    return;
            }

            if (pos >= Size)
                throw new Exception($"({org})[{IndexTranslate}] Out of range: {pos}/{Size}");

            it.SlotId = pos + (byte)IndexTranslate;

            var intersects = NoIntersects(pos, it.BasicInfo.Size);

            if (intersects.Width != 0)
                throw new Exception($"({org})[{IndexTranslate}] Space isn't free: ({Get((byte)(intersects.Y*8 + intersects.X + (float)IndexTranslate))})");

            it.Storage = StorageID;
            _items.Add(pos, it);

            if(!NoMapped)
                _map.Add(new RectangleF(new Point(pos % 8, pos / 8), it.BasicInfo.Size));
            NeedSave = true;
        }

        public Item Get(byte pos)
        {
            pos -= (byte)IndexTranslate;
            if(_items.ContainsKey(pos))
                return _items[pos];

            return null;
        }

        public void Remove(byte pos)
        {
            pos -= (byte)IndexTranslate;
            _items.Remove(pos);

            if (!NoMapped)
            {
                var pos2 = new Point(pos % 8, pos / 8);
                var rect = _map.First(x => x.Location == pos2);
                _map.Remove(rect);
            }
        }

        public void Remove(Item it)
        {
            if (!_items.Any(x => x.Value == it))
                return;

            var info = _items.First(x => x.Value == it);
            Remove((byte)(IndexTranslate+info.Key));
        }

        public void Clear()
        {
            _items.Clear();
            _map.Clear();
        }

        public InventoryDto[] GetInventory()
        {
            return _items
                .Select(x => new InventoryDto { Index = (byte)(x.Key+IndexTranslate), Item = x.Value.GetBytes() })
                .ToArray();
        }

        public bool CanContain(byte address)
        {
            return address >= (byte)IndexTranslate && address < EndIndex;
        }
    }
}
