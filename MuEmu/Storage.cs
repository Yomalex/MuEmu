﻿using MU.Resources;
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
        public int EndIndex { get; private set; }
        public int Size { get; private set; }
        public uint Money { get; set; }
        public Dictionary<byte, Item> Items => _items;

        public Storage(int size, StorageID startIndex = StorageID.Equipament)
        {
            Size = size;
            _items = new Dictionary<byte, Item>();
            _bounds = new RectangleF(new Point(0, 0), new SizeF(8, Size / 8));
            _map = new List<RectangleF>();
            IndexTranslate = startIndex;
            EndIndex = (int)startIndex + size;
        }
        
        public byte Add(Item it, byte offset = 0)
        {
            for (var i = offset; i < Size; i++)
            {
                var itemRect = new RectangleF(new Point(i % 8, i / 8), it.BasicInfo.Size);
                itemRect.Width -= 0.1f;
                itemRect.Height -= 0.1f;
                if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                    continue;

                if (_map.Where(x => x.IntersectsWith(itemRect)).Count() == 0)
                {
                    _add(i, it);
                    NeedSave = true;
                    it.SlotId = (int)IndexTranslate + i;
                    return (byte)it.SlotId;
                }
            }

            return 0xff;
        }

        private bool NoIntersects(byte offset, Size freeSpace)
        {
            var itemRect = new RectangleF(new Point(offset % 8, offset / 8), freeSpace);
            itemRect.Width -= 0.1f;
            itemRect.Height -= 0.1f;
            if (itemRect.Right >= _bounds.Right || itemRect.Bottom >= _bounds.Bottom)
                return false;

            if (_map.Where(x => x.IntersectsWith(itemRect)).Count() == 0)
            {
                return true;
            }

            return false;
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
            _map.Add(new RectangleF(new Point(pos % 8, pos / 8), it.BasicInfo.Size));
        }

        public void Add(byte pos, Item it)
        {
            var org = pos;
            pos -= (byte)IndexTranslate;

            if (_items.ContainsKey(pos))
            {
                /*var target = _items[pos];
                if(target.Number == it.Number && target.Plus == it.Plus) //try to stack
                {
                    target.Durability 
                }*/
                throw new Exception($"({org})[{IndexTranslate}] Position {pos} isn't free");
            }

            if (pos >= Size)
                throw new Exception($"({org})[{IndexTranslate}] Out of range: {pos}/{Size}");

            it.SlotId = pos + (byte)IndexTranslate;
            if (!NoIntersects(pos, it.BasicInfo.Size))
                return;

            _items.Add(pos, it);
            _map.Add(new RectangleF(new Point(pos % 8, pos / 8), it.BasicInfo.Size));
            NeedSave = true;
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
