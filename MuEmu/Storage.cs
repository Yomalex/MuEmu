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

        public int IndexTranslate { get; set; }
        public int Size { get; set; }
        public int Money { get; set; }
        public Dictionary<byte, Item> Items => _items;

        public Storage(int size)
        {
            Size = size;
            _items = new Dictionary<byte, Item>();
            _bounds = new RectangleF(new Point(0,0), new SizeF(8, Size / 8));
            _map = new List<RectangleF>();
        }

        public byte Add(Item it)
        {
            for (var i = 0; i < Size; i++)
            {
                var itemRect = new RectangleF(new Point(i % 8, i / 8), it.BasicInfo.Size);
                itemRect.Width -= 0.1f;
                itemRect.Height -= 0.1f;
                if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                    continue;

                if (_map.Where(x => x.IntersectsWith(itemRect)).Count() == 0)
                {
                    _add((byte)i, it);
                    return (byte)(IndexTranslate + i);
                }
            }

            return 0xff;
        }

        private void _add(byte pos, Item it)
        {
            _items.Add(pos, it);
            _map.Add(new RectangleF(new Point(pos % 8, pos / 8), it.BasicInfo.Size));
        }

        public bool Add(byte pos, Item it)
        {
            if (_items.ContainsKey(pos))
                return false;

            pos -= (byte)IndexTranslate;
            if (pos >= Size)
                return false;

            _items.Add(pos, it);
            _map.Add(new RectangleF(new Point(pos % 8, pos / 8), it.BasicInfo.Size));
            return true;
        }

        public Item Get(byte pos)
        {
            pos -= (byte)IndexTranslate;
            return _items[pos];
        }

        public void Remove(byte pos)
        {
            pos -= (byte)IndexTranslate;
            _items.Remove(pos);
            var pos2 = new Point(pos % 8, pos / 8);
            var rect = _map.First(x => x.Location == pos2);
            _map.Remove(rect);
        }

        public InventoryDto[] GetInventory()
        {
            return _items
                .Select(x => new InventoryDto { Index = (byte)(x.Key+IndexTranslate), Item = x.Value.GetBytes() })
                .ToArray();
        }
    }
}
