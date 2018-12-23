using MU.DataBase;
using MuEmu.Entity;
using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public class Inventory
    {
        public Player Player { get; set; }
        private Dictionary<Equipament, Item> _equipament;
        private Storage _inventory;
        private Storage _chaosBox;
        private Storage _personalShop;
        private Storage _tradeBox;
        private List<Item> _forDelete;

        public int Defense => _equipament.Sum(x => x.Value.Defense);
        //public int DefenseRate => _equipament.Sum(x => x.Value.BasicInfo.DefRate);

        public byte[] FindAll(ItemNumber num)
        {
            var res = new List<byte>();
            foreach(var e in _equipament)
            {
                if(e.Value.Number == num)
                {
                    res.Add((byte)e.Key);
                }
            }
            foreach(var inv in _inventory.Items)
            {
                if (inv.Value.Number == num)
                {
                    res.Add((byte)(inv.Key+_inventory.IndexTranslate));
                }
            }
            return res.ToArray();
        }

        public List<Item> FindAllItems(ItemNumber num)
        {
            var res = new List<Item>();
            foreach (var e in _equipament)
            {
                if (e.Value.Number == num)
                {
                    res.Add(e.Value);
                }
            }
            foreach (var inv in _inventory.Items)
            {
                if (inv.Value.Number == num)
                {
                    res.Add(inv.Value);
                }
            }
            return res;
        }

        public int GetFreeSlotCount()
        {
            return 0;
        }

        public Inventory(Character @char, CharacterDto characterDto)
        {
            Player = @char?.Player??null;
            _inventory = new Storage(Storage.InventorySize);
            _chaosBox = new Storage(Storage.ChaosBoxSize);
            _tradeBox = new Storage(Storage.TradeSize);
            _personalShop = new Storage(Storage.TradeSize);
            _inventory.IndexTranslate = (int)Equipament.End;
            _personalShop.IndexTranslate = _inventory.IndexTranslate + Storage.InventorySize;
            _forDelete = new List<Item>();
            _equipament = new Dictionary<Equipament, Item>();

            foreach (var item in characterDto.Items)
            {
                if(item.VaultId == 0)
                    Add((byte)item.SlotId, new Item(item));
            }
        }

        private void Add(byte pos, Item item)
        {
            if(pos < _inventory.IndexTranslate)
            {
                _equipament.Add((Equipament)pos, item);
            }else if(pos < _personalShop.IndexTranslate)
            {
                _inventory.Add(pos, item);
            }else
            {
                _personalShop.Add(pos, item);
            }
        }

        public byte Add(Item it)
        {
            return _inventory.Add(it);
        }

        public void Equip(Equipament slot, Item item)
        {
            if (_equipament.ContainsKey(slot))
                throw new InvalidOperationException("Trying to equip already equiped slot:"+slot);

            if ((Player?.Character ?? null) != null)
            {
                if (item.ReqStrength > Player.Character.Str)
                    throw new InvalidOperationException("Need more Strength");

                if (item.ReqAgility > Player.Character.Agility)
                    throw new InvalidOperationException("Need more Agility");

                if (item.ReqVitality > Player.Character.Vitality)
                    throw new InvalidOperationException("Need more Vitality");

                if (item.ReqEnergy > Player.Character.Energy)
                    throw new InvalidOperationException("Need more Energy");

                if (item.ReqCommand > Player.Character.Command)
                    throw new InvalidOperationException("Need more Command");
            }
            else
            {
                throw new InvalidOperationException("No character logged");
            }

            _equipament.Add(slot, item);
            item.ApplyEffects(Player);
        }

        public void Unequip(Equipament slot)
        {
            if (!_equipament.ContainsKey(slot))
                throw new InvalidOperationException("Trying to unequip no equiped slot:"+slot);

            var it = _equipament[slot];
            _equipament.Remove(slot);
            it.RemoveEffects();
        }

        public bool Move(MoveItemFlags from, byte fromIndex, MoveItemFlags to, byte toIndex)
        {
            Storage sFrom = null, sTo = null;
            switch(from)
            {
                case MoveItemFlags.Inventory:
                    sFrom = _inventory;
                    break;
                case MoveItemFlags.Warehouse:
                    sFrom = Player.Account.Vault;
                    break;
                case MoveItemFlags.PersonalShop:
                    sFrom = _personalShop;
                    break;
                case MoveItemFlags.ChaosBox:
                case MoveItemFlags.DarkTrainer:
                    sFrom = _personalShop;
                    break;
                case MoveItemFlags.Trade:
                    sFrom = _personalShop;
                    break;
            }
            switch (to)
            {
                case MoveItemFlags.Inventory:
                    sTo = _inventory;
                    break;
                case MoveItemFlags.Warehouse:
                    sTo = Player.Account.Vault;
                    break;
                case MoveItemFlags.PersonalShop:
                    sTo = _personalShop;
                    break;
                case MoveItemFlags.ChaosBox:
                case MoveItemFlags.DarkTrainer:
                    sTo = _personalShop;
                    break;
                case MoveItemFlags.Trade:
                    sTo = _personalShop;
                    break;
            }

            if (sFrom == null || sTo == null)
                return false;

            Item it = null;

            if(from == MoveItemFlags.Inventory && fromIndex < (byte)Equipament.End)
            {
                it = _equipament[(Equipament)fromIndex];
                sTo.Add(toIndex, it);
                Unequip((Equipament)fromIndex);
                return true;
            }else if(to == MoveItemFlags.Inventory && toIndex < (byte)Equipament.End)
            {
                it = sFrom.Get(fromIndex);
                sFrom.Remove(fromIndex);
                it.Target = Player.Character;
                Equip((Equipament)toIndex, it);
                return true;
            }
            
            it = sFrom.Get(fromIndex);
            sTo.Add(toIndex, it);
            sFrom.Remove(fromIndex);
            return true;
        }

        public Item Get(byte from)
        {
            if (_inventory.IndexTranslate <= from)
                return _inventory.Items.First(x => x.Key == from - _inventory.IndexTranslate).Value;

            return _equipament.FirstOrDefault(x => x.Key == (Equipament)from).Value;
        }

        public List<Item> Get(IEnumerable<byte> positions)
        {
            var returns = new List<Item>();

            foreach(var p in positions)
                returns.Add(Get(p));

            return returns;
        }

        public void Remove(byte from)
        {
            if (_inventory.IndexTranslate <= from)
            {
                _inventory.Remove(from);
            }

            _equipament.Remove((Equipament)from);
        }

        public async Task Delete(byte target)
        {
            if (_equipament.ContainsKey((Equipament)target))
            {
                _forDelete.Add(_equipament[(Equipament)target]);
                Unequip((Equipament)target);
                await Player.Session.SendAsync(new SInventoryItemDelete(target, 1));
                return;
            }

            _forDelete.Add(_inventory.Get(target));
            Remove(target);
            await Player.Session.SendAsync(new SInventoryItemDelete(target, 1));
        }

        public async Task Delete(Item item)
        {
            if (_equipament.ContainsValue(item))
            {
                await Delete((byte)_equipament.First(x => x.Value == item).Key);
            }else if(_inventory.Items.Any(x => x.Value == item))
            {
                await Delete(_inventory.Items.First(x => x.Value == item).Key);
            }
            else if (_chaosBox.Items.Any(x => x.Value == item))
            {
                await Delete(_chaosBox.Items.First(x => x.Value == item).Key);
            }
            else if (_personalShop.Items.Any(x => x.Value == item))
            {
                await Delete(_personalShop.Items.First(x => x.Value == item).Key);
            }
            else if (_tradeBox.Items.Any(x => x.Value == item))
            {
                await Delete(_tradeBox.Items.First(x => x.Value == item).Key);
            }
        }

        public async void SendInventory()
        {
            var list = new List<Network.Data.InventoryDto>();

            foreach (var it in _equipament)
            {
                list.Add(new Network.Data.InventoryDto
                {
                    Index = (byte)it.Key,
                    Item = it.Value.GetBytes()
                });
            }

            list.AddRange(_inventory.GetInventory());
            list.AddRange(_personalShop.GetInventory());

            await Player.Session.SendAsync(new SInventory(list.ToArray()));
        }

        public static byte[] GetCharset(HeroClass @class, Inventory inv)
        {
            var CharSet = new byte[18];
            var equip = inv._equipament;

            CharSet[0] = Character.GetClientClass(@class);
            var SmallLevel = 0u;

            if (equip.ContainsKey(Equipament.RightHand))
            {
                var it = equip[Equipament.RightHand];
                CharSet[1] = it.Number.Type;
                CharSet[12] |= (byte)(it.Number.Index << 4);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x04 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x04 : 0x00);
                SmallLevel |= it.SmallPlus;
            } else
            {
                CharSet[1] = 0xff;
                CharSet[12] |= 0xF0;
            }

            if (equip.ContainsKey(Equipament.LeftHand))
            {
                var it = equip[Equipament.LeftHand];
                CharSet[2] = it.Number.Type;
                CharSet[13] |= (byte)(it.Number.Index << 4);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x02 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x02 : 0x00);
                SmallLevel |= (byte)(it.SmallPlus << 3);
            } else
            {
                CharSet[2] = 0xff;
                CharSet[13] |= 0xF0;
            }

            if(equip.ContainsKey(Equipament.Helm))
            {
                var it = equip[Equipament.Helm];
                CharSet[13] |= (byte)((it.Number.Number&0x1E0) >> 5); //1FF
                CharSet[9] |= (byte)((it.Number.Number & 0x10) << 3);
                CharSet[3] |= (byte)((it.Number.Number & 0x0F) << 4);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x80 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x80 : 0x00);
                SmallLevel |= (byte)(it.SmallPlus << 6);
            }
            else
            {
                CharSet[13] |= 0x0F;
                CharSet[9] |= 0x80;
                CharSet[3] |= 0xF0;
            }

            if (equip.ContainsKey(Equipament.Armor))
            {
                var it = equip[Equipament.Armor];
                CharSet[14] |= (byte)((it.Number.Number & 0x1E0) >> 1); //1FF
                CharSet[9] |= (byte)((it.Number.Number & 0x10) << 2);
                CharSet[3] |= (byte)((it.Number.Number & 0x0F)/* << 4*/);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x40 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x40 : 0x00);
                SmallLevel |= (byte)(it.SmallPlus << 9);
            }
            else
            {
                CharSet[14] |= 0xF0;
                CharSet[9] |= 0x40;
                CharSet[3] |= 0x0F;
            }

            if (equip.ContainsKey(Equipament.Pants))
            {
                var it = equip[Equipament.Pants];
                CharSet[14] |= (byte)((it.Number.Number & 0x1E0) >> 5); //1FF
                CharSet[9] |= (byte)((it.Number.Number & 0x10) << 1);
                CharSet[4] |= (byte)((it.Number.Number & 0x0F) << 4);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x20 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x20 : 0x00);
                SmallLevel |= (byte)(it.SmallPlus << 12);
            }
            else
            {
                CharSet[14] |= 0x0F;
                CharSet[9] |= 0x20;
                CharSet[4] |= 0xF0;
            }

            if (equip.ContainsKey(Equipament.Gloves))
            {
                var it = equip[Equipament.Gloves];
                CharSet[15] |= (byte)((it.Number.Number & 0x1E0) >> 1); //1FF
                CharSet[9] |= (byte)((it.Number.Number & 0x10)/* << 1*/);
                CharSet[4] |= (byte)((it.Number.Number & 0x0F)/* << 4*/);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x10 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x10 : 0x00);
                SmallLevel |= (byte)(it.SmallPlus << 15);
            }
            else
            {
                CharSet[15] |= 0xF0;
                CharSet[9] |= 0x10;
                CharSet[4] |= 0x0F;
            }

            if (equip.ContainsKey(Equipament.Boots))
            {
                var it = equip[Equipament.Boots];
                CharSet[15] |= (byte)((it.Number.Number & 0x1E0) >> 5); //1FF
                CharSet[9] |= (byte)((it.Number.Number & 0x10) << 1);
                CharSet[5] |= (byte)((it.Number.Number & 0x0F) << 4);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x08 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x08 : 0x00);
                SmallLevel |= (byte)(it.SmallPlus << 18);
            }
            else
            {
                CharSet[15] |= 0x0F;
                CharSet[9] |= 0x08;
                CharSet[5] |= 0xF0;
            }

            if(equip.ContainsKey(Equipament.Wings))
            {
                var it = equip[Equipament.Wings];
                CharSet[5] |= (byte)((it.Number.Number & 0x03) << 2);
            }
            else
            {
                CharSet[5] |= 0x0C;
            }

            if (equip.ContainsKey(Equipament.Pet))
            {
                var it = equip[Equipament.Pet];
                CharSet[5] |= (byte)((it.Number.Number & 0x03)/* << 2*/);
            }
            else
            {
                CharSet[5] |= 0x03;
            }

            CharSet[6] = (byte)((SmallLevel >> 16) & 0xff);
            CharSet[7] = (byte)((SmallLevel >>  8) & 0xff);
            CharSet[8] = (byte)(SmallLevel & 0xff);

            return CharSet;
        }

        public byte[] GetCharset()
        {
            return GetCharset(Player.Character.Class, Player.Character.Inventory);
        }
    }
}
