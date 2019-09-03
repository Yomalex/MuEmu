using MU.DataBase;
using MuEmu.Entity;
using MuEmu.Network.Game;
using Serilog;
using Serilog.Core;
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
        private static ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Inventory));
        //public Player Player { get; set; }
        public Character Character { get; }

        private Dictionary<Equipament, Item> _equipament;
        private Dictionary<StorageID, object> Storages;
        private Storage _inventory;
        private Storage _chaosBox;
        private Storage _personalShop;
        private Storage _tradeBox;
        private List<Item> _forDelete;
        private bool _needSave;
        private int _defense;
        private int _defenseRate;
        private int _excellentRate;
        private int _criticalRate;
        private int _reflect;

        public int ExcellentRate => _excellentRate;
        public int CriticalRate => _criticalRate;
        public int Defense => _defense;
        public int DefenseRate => _defenseRate;
        public int Reflect => _reflect;

        public Storage ChaosBox => _chaosBox;
        public Storage PersonalShop => _personalShop;
        public Storage TradeBox => _tradeBox;

        public bool Lock { get; set; }

        public byte[] FindAll(ItemNumber num)
        {
            var res = new List<byte>();
            foreach (var e in _equipament)
            {
                if (e.Value.Number == num)
                {
                    res.Add((byte)e.Key);
                }
            }
            foreach (var inv in _inventory.Items)
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
        
        public bool TryAdd()
        {
            return _inventory.TryAdd(new System.Drawing.Size(5,3));
        }

        public Inventory(Character @char, CharacterDto characterDto)
        {
            Character = @char;
            Storages = new Dictionary<StorageID, object>();
            _equipament = new Dictionary<Equipament, Item>();
            _inventory = new Storage(Storage.InventorySize, StorageID.Inventory);
            _personalShop = new Storage(Storage.TradeSize, StorageID.PersonalShop);
            if (@char != null)
            {
                _chaosBox = new Storage(Storage.ChaosBoxSize);
                _tradeBox = new Storage(Storage.TradeSize);
            }
            _forDelete = new List<Item>();
            Storages.Add(StorageID.Equipament, _equipament);
            Storages.Add(StorageID.Inventory, _inventory);
            Storages.Add(StorageID.PersonalShop, _personalShop);

            foreach (var item in characterDto.Items.Where(x => x.VaultId == 0))
            {
                var it = new Item(item);
                try
                {
                    Add((byte)item.SlotId, it, false);
                }catch(Exception)
                {
                    if(Add(it) == 0xff)
                    {
                        _forDelete.Add(it);
                    }
                }
            }
        }

        private void Add(byte pos, Item item, bool save = true)
        {
            foreach(var st in Storages)
            {
                if (st.Value.GetType() == typeof(Storage))
                {
                    var sto = st.Value as Storage;
                    if (sto.CanContain(pos))
                    {
                        sto.Add(pos, item);
                        break;
                    }
                }
                else
                {
                    var sto = st.Value as Dictionary<Equipament, Item>;
                    if (pos < (byte)StorageID.Inventory)
                    {
                        sto.Add((Equipament)pos, item);
                        break;
                    }
                }
            }
            
            if (Character != null)
            {
                item.AccountId = Character.Account.ID;
                item.CharacterId = Character.Id;
                item.Character = Character;
            }

            if (save)
                _needSave = true;
        }

        public byte Add(Item it)
        {
            if (Character != null)
            {
                it.AccountId = Character.Account.ID;
                it.CharacterId = Character.Id;
            }
            _needSave = true;
            return _inventory.Add(it);
        }

        public void Equip(Equipament slot, Item item)
        {
            var bslot = (byte)slot;
            if (_equipament.ContainsKey(slot))
            //if(_inventory.Items.ContainsKey(bslot))
                throw new InvalidOperationException("Trying to equip already equiped slot:"+slot);

            if (Character != null)
            {
                if (item.ReqStrength > Character.StrengthTotal)
                    throw new InvalidOperationException("Need more Strength");

                if (item.ReqAgility > Character.AgilityTotal)
                    throw new InvalidOperationException("Need more Agility");

                if (item.ReqVitality > Character.VitalityTotal)
                    throw new InvalidOperationException("Need more Vitality");

                if (item.ReqEnergy > Character.EnergyTotal)
                    throw new InvalidOperationException("Need more Energy");

                if (item.ReqCommand > Character.CommandTotal)
                    throw new InvalidOperationException("Need more Command");
            }
            else
            {
                throw new InvalidOperationException("No character logged");
            }

            item.SlotId = (int)slot;
            item.VaultId = 0;
            _equipament.Add(slot, item);
            item.ApplyEffects(Character.Player);
            item.CharacterId = Character.Id;

            _defense += item.Defense;
            _defenseRate += item.BasicInfo.DefRate;
            _criticalRate += item.CriticalDamage;
        }

        public Item Unequip(Equipament slot)
        {
            if(!_equipament.ContainsKey(slot))
                throw new InvalidOperationException("Trying to unequip no equiped slot:"+slot);

            var it = _equipament[slot];
            _equipament.Remove(slot);
            it.RemoveEffects();
            it.SlotId = 0xff;

            _defense -= it.Defense;
            _defenseRate -= it.BasicInfo.DefRate;
            _criticalRate -= it.CriticalDamage;

            return it;
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
                    sFrom = Character.Account.Vault;
                    break;
                case MoveItemFlags.PersonalShop:
                    sFrom = _personalShop;
                    break;
                case MoveItemFlags.ChaosBox:
                case MoveItemFlags.DarkTrainer:
                    sFrom = _chaosBox;
                    break;
                case MoveItemFlags.Trade:
                    sFrom = _tradeBox;
                    break;
            }
            switch (to)
            {
                case MoveItemFlags.Inventory:
                    sTo = _inventory;
                    break;
                case MoveItemFlags.Warehouse:
                    sTo = Character.Account.Vault;
                    break;
                case MoveItemFlags.PersonalShop:
                    sTo = _personalShop;
                    break;
                case MoveItemFlags.ChaosBox:
                case MoveItemFlags.DarkTrainer:
                    sTo = _chaosBox;
                    break;
                case MoveItemFlags.Trade:
                    sTo = _tradeBox;
                    break;
            }

            if (sFrom == null || sTo == null)
                return false;

            Item it = null;

            if (from == MoveItemFlags.Inventory && fromIndex < (byte)Equipament.End)
            {
                it = Unequip((Equipament)fromIndex);
            }
            else
            {
                it = sFrom.Get(fromIndex);
                sFrom.Remove(fromIndex);
            }

            if (to == MoveItemFlags.Inventory && toIndex < (byte)Equipament.End)
            {
                Equip((Equipament)toIndex, it);
            }
            else
            {
                try
                {
                    sTo.Add(toIndex, it);

                    if (to == MoveItemFlags.Warehouse)
                        it.VaultId = Character.Account.ID * 10 + Character.Account.ActiveVault;
                }catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Can't move, rolling back");
                    if (from == MoveItemFlags.Inventory && fromIndex < (byte)Equipament.End)
                    {
                        Equip((Equipament)fromIndex, it);
                    }
                    else
                    {
                        sFrom.Add(fromIndex, it);
                    }
                    return false;
                }
            }
            _needSave = true;
            return true;
        }

        public Item Get(byte from)
        {
            if (_inventory.CanContain(from))
                return _inventory.Get(from);

            if (from < 12)
                return _equipament[(Equipament)from];

            return null;
        }

        public Item Get(Equipament from)
        {
            if (from < Equipament.End && _equipament.ContainsKey(from))
                return _equipament[from];

            return null;
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
            if(_inventory.CanContain(from))
            {
                _inventory.Remove(from);
            }
            else if (_personalShop.CanContain(from))
            {
                _personalShop.Remove(from);
            }else if(from < 12)
            {
                _equipament.Remove((Equipament)from);
            }

            _needSave = true;
        }

        public async Task Delete(byte target)
        {
            _needSave = true;
            var session = Character.Player.Session;

            if (_equipament.ContainsKey((Equipament)target))
            {
                _forDelete.Add(Unequip((Equipament)target));
            }
            else
            if (_inventory.CanContain(target))
            {
                _forDelete.Add(_inventory.Get(target));
            }else if(_personalShop.CanContain(target))
            {
                _forDelete.Add(_personalShop.Get(target));
            }

            Remove(target);
            await session.SendAsync(new SInventoryItemDelete(target, 1));
        }

        public async Task Delete(Item item)
        {
            if (_equipament.ContainsValue(item))
            {
                await Delete((byte)_equipament.First(x => x.Value == item).Key);
            }
            else
            if (_inventory.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_inventory.Items.First(x => x.Value == item).Key + _inventory.IndexTranslate));
            }
            else if (_chaosBox.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_chaosBox.Items.First(x => x.Value == item).Key + _chaosBox.IndexTranslate));
            }
            else if (_personalShop.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_personalShop.Items.First(x => x.Value == item).Key + _personalShop.IndexTranslate));
            }
            else if (_tradeBox.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_tradeBox.Items.First(x => x.Value == item).Key + _tradeBox.IndexTranslate));
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

            await Character.Player.Session.SendAsync(new SInventory(list.ToArray()));
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
                CharSet[1] = (byte)it.Number.Index;
                CharSet[12] |= (byte)((byte)it.Number.Type << 4);
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
                CharSet[2] = (byte)it.Number.Index;
                CharSet[13] |= (byte)((byte)it.Number.Type << 4);
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
            return GetCharset(Character.Class, this);
        }

        public async Task Save(GameContext db)
        {            
            if(_forDelete.Any())
            {
                var forDel = from it in db.Items
                             from del in _forDelete
                             where it.ItemId == del.Serial
                             select it;

                if (forDel.Any())
                {
                    db.Items.RemoveRange(forDel);
                    _logger.Information("Deleting {0} items", forDel.Count());
                }

                _forDelete.Clear();
            }

            if (!_needSave || Lock)
                return;

            _logger.Information("----- Main Inventory Save");

            foreach(var e in _equipament.Values)
                await e.Save(db);

            foreach (var e in _inventory.Items.Values)
                await e.Save(db);

            foreach (var e in _personalShop.Items.Values)
                await e.Save(db);

            _needSave = false;
        }

        public void DeleteAll(Storage storage)
        {
            _forDelete.AddRange(storage.Items.Values);
            storage.Clear();
        }

        public void DeleteChaosBox() => DeleteAll(ChaosBox);
    }
}
