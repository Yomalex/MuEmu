﻿using MU.DataBase;
using MU.Resources;
using MuEmu.Entity;
using MuEmu.Network;
using MU.Network.Game;
using MU.Resources;
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
    internal class Transaction
    {
        public MoveItemFlags From;
        public byte fromIndex;
        public MoveItemFlags To;
        public byte toIndex;
    }
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
        private float _excellentRate;
        private int _criticalRate;
        private float _reflect;
        private float _increaseHP;
        private float _increaseMP;
        private float _dropZen;
        private float _dmgDecrease;
        private float _increaseWizardryRate;
        private float _increaseLifeRate;
        private float _increaseManaRate;
        private float _increaseWizardry;

        public float ExcellentRate => _excellentRate;
        public int CriticalRate => _criticalRate;
        public int Defense => _defense;
        public int DefenseRate => _defenseRate;
        public float IncreaseWizardryRate => _increaseWizardryRate;
        public float IncreaseWizardry => _increaseWizardry;
        public float IncreaseLifeRate => _increaseLifeRate;
        public float IncreaseManaRate => _increaseManaRate;
        public float DropZen => _dropZen;
        public float Reflect => _reflect;
        public float IncreaseHP => _increaseHP;
        public float IncreaseMP => _increaseMP;
        public float DmgDecrease => _dmgDecrease;

        public Storage ChaosBox => _chaosBox;
        public Storage PersonalShop => _personalShop;
        public Storage TradeBox => _tradeBox;
        public bool TradeOk { get; set; }
        public bool TradeOpen { get => _tradeOpen; set
            {
                _tradeOpen = value;
                if (value == false)
                    _transactions.Clear();
            }
        }
        private bool _tradeOpen;
        private List<Transaction> _transactions = new List<Transaction>();

        public Item ItemMoved { get; private set; }

        public Item Arrows { get; private set; }

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

        public bool TryAdd(IEnumerable<Item> items)
        {
            return _inventory.TryAdd(items.Select(x => x.BasicInfo.Size).ToArray());
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
                var it = new Item(item, @char?.Account, @char);
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
                        if (item.Number.Type == ItemType.BowOrCrossbow && (item.Number.Index == 7 || item.Number.Index == 15))
                            Arrows = item;
                        break;
                    }
                }
            }
            
            if (Character != null)
            {
                //item.Account = Character.Account;
                item.Character = Character;
            }

            if (save)
                _needSave = true;
        }

        public void CalcStats()
        {
            _defense = 0;
            _defenseRate = 0;
            _criticalRate = 0;
            _excellentRate = 0;
            _increaseWizardryRate = 0;
            _increaseWizardry = 0;
            _increaseLifeRate = 0;
            _increaseManaRate = 0;
            _dropZen = 0;
            _reflect = 0;
            _dmgDecrease = 0;
            _increaseMP = 0;
            _increaseHP = 0;

            foreach (var equip in _equipament)
            {
                var item = equip.Value;

                if (Character != null)
                    item.ApplyEffects(Character);

                _defense += item.Defense + item.AditionalDefense;
                _defenseRate += item.BasicInfo.DefRate;

                _criticalRate += item.CriticalDamage;
                _excellentRate += item.ExcellentDmgRate;
                _increaseWizardryRate += item.IncreaseWizardryRate;
                _increaseWizardry += item.IncreaseWizardry;
                _increaseLifeRate += item.IncreaseLifeRate;
                _increaseManaRate += item.IncreaseManaRate;

                _dropZen += item.IncreaseZenRate;
                _reflect += item.ReflectDamage;
                _dmgDecrease += item.DamageDecrease;
                _increaseMP += item.IncreaseMana;
                _increaseHP += item.IncreaseHP;
            }
        }

        public byte Add(Item it)
        {
            if (Character != null)
            {
                it.Account = Character.Account;
                it.Character = Character;
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
                    throw new InvalidOperationException("Need more Strength: req "+ item.ReqStrength);

                if (item.ReqAgility > Character.AgilityTotal)
                    throw new InvalidOperationException("Need more Agility: req " + item.ReqAgility);

                if (item.ReqVitality > Character.VitalityTotal)
                    throw new InvalidOperationException("Need more Vitality: req " + item.ReqVitality);

                if (item.ReqEnergy > Character.EnergyTotal)
                    throw new InvalidOperationException("Need more Energy: req " + item.ReqEnergy);

                if (item.ReqCommand > Character.CommandTotal)
                    throw new InvalidOperationException("Need more Command: req " + item.ReqCommand);
            }
            else
            {
                throw new InvalidOperationException("No character logged");
            }

            item.SlotId = (int)slot;
            item.VaultId = 0;
            _equipament.Add(slot, item);
            item.ApplyEffects(Character);
            item.Character = Character;
            item.Account = Character.Account;

            if (item.Number.Type == ItemType.BowOrCrossbow && (item.Number.Index == 7 || item.Number.Index == 15))
                Arrows = item;

            CalcStats();
            Character.ObjCalc();
        }

        public Item Unequip(Equipament slot)
        {
            if(!_equipament.ContainsKey(slot))
                throw new InvalidOperationException("Trying to unequip no equiped slot:"+slot);

            var item = _equipament[slot];
            _equipament.Remove(slot);
            item.RemoveEffects();
            item.SlotId = 0xff;

            CalcStats();
            Character.ObjCalc();
            if (Arrows == item)
                Arrows = null;

            return item;
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
            it.Account = Character.Account;

            Log.Debug("Move item {0}:{1} {4} to {2}:{3}", from, fromIndex, to, toIndex, it.ToString());

            if (to == MoveItemFlags.Inventory && toIndex < (byte)Equipament.End)
            {
                try
                {
                    Equip((Equipament)toIndex, it);
                }catch(Exception ex)
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
            else
            {
                try
                {
                    sTo.Add(toIndex, it);

                    if (to == MoveItemFlags.Warehouse)
                    {
                        it.VaultId = Character.Account.ID * 10 + Character.Account.ActiveVault;
                        it.Character = null;
                    }
                    else
                    {
                        it.VaultId = 0;
                        it.Character = Character;
                    }

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
            ItemMoved = it;
            _needSave = true;
            if (TradeOpen)
            {
                var session2 = Character.Player.Window as GSSession;
                _transactions.Add(new Transaction { From = from, fromIndex = fromIndex, To = to, toIndex = toIndex });
                Character.Player.Session.SendAsync(new CTradeButtonOk { Flag = 0 }).Wait();
                session2.SendAsync(new CTradeButtonOk { Flag = 2 }).Wait();
                session2.SendAsync(new STradeOtherAdd { Position = toIndex, ItemInfo = it.GetBytes() }).Wait();
            }
            return true;
        }

        public void TradeRollBack()
        {
            _transactions.Reverse();
            var clone = _transactions.ToList();
            TradeOpen = false;

            Character.Money += TradeBox.Money;

            foreach (var t in clone)
            {
                if (Move(t.To, t.toIndex, t.From, t.fromIndex) == false)
                    throw new Exception("Can't rollback transactions");
            }
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

        /// <summary>
        /// Remove item from character inventory without delete it from database
        /// </summary>
        /// <param name="from">Position</param>
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
        }
        /// <summary>
        /// Delete item from Database
        /// </summary>
        /// <param name="target">Position</param>
        /// <param name="send">Update client inventory</param>
        /// <returns></returns>
        public async Task Delete(byte target, bool send = true)
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

            if(send)
                await session.SendAsync(new SInventoryItemDelete(target, 1));
        }

        public async Task Delete(Item item, bool send = true)
        {
            if (_equipament.ContainsValue(item))
            {
                var slot = _equipament.First(x => x.Value == item).Key;
                await Delete((byte)slot, send);
            }
            else
            if (_inventory.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_inventory.Items.First(x => x.Value == item).Key + _inventory.IndexTranslate), send);
            }
            else if (_chaosBox.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_chaosBox.Items.First(x => x.Value == item).Key + _chaosBox.IndexTranslate), send);
            }
            else if (_personalShop.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_personalShop.Items.First(x => x.Value == item).Key + _personalShop.IndexTranslate), send);
            }
            else if (_tradeBox.Items.Any(x => x.Value == item))
            {
                await Delete((byte)(_tradeBox.Items.First(x => x.Value == item).Key + _tradeBox.IndexTranslate), send);
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

        public static byte[] GetCharset(HeroClass @class, Inventory inv, byte ActionNumber)
        {
            var CharSet = new byte[18];
            var equip = inv._equipament;

            CharSet[0] = Character.GetClientClass(@class);
            switch(ActionNumber)
            {
                case 128:
                    CharSet[0] |= (byte)2u;
                    break;
                case 129:
                    CharSet[0] |= (byte)3u;
                    break;
            }
            var SmallLevel = 0u;

            if (equip.ContainsKey(Equipament.RightHand))
            {
                var it = equip[Equipament.RightHand];
                CharSet[1] = (byte)it.Number;
                CharSet[12] |= (byte)((it.Number & 0xF00) >> 4);
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
                CharSet[2] = (byte)it.Number;
                CharSet[13] |= (byte)((it.Number & 0xF00) >> 4);
                CharSet[10] |= (byte)(it.OptionExe != 0 ? 0x02 : 0x00);
                CharSet[11] |= (byte)(it.SetOption != 0 ? 0x02 : 0x00);
                SmallLevel |= (uint)(it.SmallPlus << 3);
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
                SmallLevel |= (uint)(it.SmallPlus << 6);
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
                SmallLevel |= (uint)(it.SmallPlus << 9);
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
                SmallLevel |= (uint)(it.SmallPlus << 12);
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
                SmallLevel |= (uint)(it.SmallPlus << 15);
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
                SmallLevel |= (uint)(it.SmallPlus << 18);
            }
            else
            {
                CharSet[15] |= 0x0F;
                CharSet[9] |= 0x08;
                CharSet[5] |= 0xF0;
            }

            if (equip.ContainsKey(Equipament.Pet))
            {
                var it = equip[Equipament.Pet];

                if(
                    it.Number.Number == 6723 || //Rudolph
                    it.Number.Number == 6779 //??
                    )
                {
                    CharSet[5] |= 2;
                }

                if ((it.Number.Number & 0x03) != 0)
                {
                    CharSet[10] |= 0x01;
                    CharSet[5] |= (byte)(it.Number.Number & 0x03);
                }

                if(it.Number.Number == ItemNumber.FromTypeIndex(13,4))
                {
                    CharSet[12] |= 0x01;
                }
            }

            if(
                (equip.ContainsKey(Equipament.LeftRing) && (equip[Equipament.LeftRing].Number.Number == ItemNumber.FromTypeIndex(13,169) || equip[Equipament.LeftRing].Number.Number == ItemNumber.FromTypeIndex(13, 170))) ||
               (equip.ContainsKey(Equipament.RightRing) && (equip[Equipament.RightRing].Number.Number == ItemNumber.FromTypeIndex(13, 169) || equip[Equipament.RightRing].Number.Number == ItemNumber.FromTypeIndex(13, 170))))
            {
                CharSet[12] |= 0x08;
            }

            if(equip.ContainsKey(Equipament.Wings))
            {
                Dictionary<int, byte[]> sub;
                var it = equip[Equipament.Wings];
                // Pre season X
                sub = new Dictionary<int, byte[]>
                {
                                  //[5], [9], [16]
                    { 0, new byte[]{ 4, 0x01, 0 } }, //Wings of Fairy
                    { 1, new byte[]{ 4, 0x02, 0 } }, //Wings of Angel
                    { 2, new byte[]{ 4, 0x03, 0 } }, //Wings of Satan
                    { 3, new byte[]{ 8, 0x01, 0 } }, //Wings of Spirit
                    { 4, new byte[]{ 12, 0x02, 0 } }, //Wings of Soul
                    { 5, new byte[]{ 12, 0x03, 0 } }, //Wings of Dragon
                    { 6, new byte[]{ 12, 0x04, 0 } }, //Wings of Darkness
                    { 30, new byte[]{ 12, 0x05, 0 } },//Cape of lord
                    { 36, new byte[]{ 12, 0x01, 0 } },//Wing of Storm
                    { 37, new byte[]{ 12, 0x02, 0 } },//Wing of Space Time
                    { 38, new byte[]{ 12, 0x03, 0 } },//Wing of Illusion
                    { 39, new byte[]{ 12, 0x04, 0 } },//Wings of Hurricane
                    { 40, new byte[]{ 12, 0x05, 0 } },//Mantle of Monarch
                    { 41, new byte[]{ 4, 0x04, 0 } },//Wing of Mistery
                    { 42, new byte[]{ 12, 0x07, 0 } },//Wing of Despair
                    { 43, new byte[]{ 12, 6, 0 } },//Wings of Violent Wind
                    { 49, new byte[]{ 8, 0x07, 0 } },
                    { 50, new byte[]{ 12, 7, 0 } },
                    { 51, new byte[]{ 12, 0, 8 } },
                    { 52, new byte[]{ 12, 0, 9 } },
                    { 53, new byte[]{ 12, 0, 10 } },
                    { 54, new byte[]{ 12, 0, 11 } },
                    { 55, new byte[]{ 12, 0, 12 } },
                    { 56, new byte[]{ 12, 0, 13 } },
                    { 57, new byte[]{ 12, 0, 14 } },
                    //{ 139, new byte[]{ 0x00, 0x02, 0x02 << 2 } },
                    //{ 140, new byte[]{ 0x00, 0x02, 0x03 << 2 } },
                    //{ 141, new byte[]{ 0x00, 0x02, 0x04 << 2 } },
                    //{ 142, new byte[]{ 0x00, 0x02, 0x05 << 2 } },
                    //{ 143, new byte[]{ 0x00, 0x02, 0x06 << 2 } },
                    //{ 144, new byte[]{ 0x00, 0x02, 0x07 << 2 } },
                    //{ 145, new byte[]{ 0x00, 0x02, 0x08 << 2 } },
                    //{ 262, new byte[]{ 0x00, 0x03, 0x00 << 2 } },
                    //{ 263, new byte[]{ 0x00, 0x03, 0x01 << 2 } },
                    //{ 264, new byte[]{ 0x00, 0x03, 0x02 << 2 } },
                    //{ 265, new byte[]{ 0x00, 0x03, 0x03 << 2 } },
                    //{ 266, new byte[]{ 0x00, 0x03, 0x10 << 2 } },
                    //{ 267, new byte[]{ 0x00, 0x03, 0x14 << 2 } },
                    //{ 268, new byte[]{ 0x00, 0x03, 0x10 << 2 } },
                    //{ 269, new byte[]{ 0x00, 0x03, 0x1C << 2 } },
                    //{ 30, new byte[]{ 0x00, 0x03, 0x18 << 2 } },
                    //{ 270, new byte[]{ 0x00, 0x04, 0x00 << 2 } },
                    //{ 278, new byte[]{ 0x00, 0x04, 0x04 << 2 } },
                };
                // Season X
                /*sub = new Dictionary<int, byte[]>
                {
                    { 0, new byte[]{ 0x00, 0x00, 0x01 << 2 } },
                    { 1, new byte[]{ 0x00, 0x00, 0x02 << 2 } },
                    { 2, new byte[]{ 0x00, 0x00, 0x03 << 2 } },
                    { 3, new byte[]{ 0x00, 0x00, 0x04 << 2 } },
                    { 4, new byte[]{ 0x00, 0x00, 0x05 << 2 } },
                    { 5, new byte[]{ 0x00, 0x00, 0x06 << 2 } },
                    { 6, new byte[]{ 0x00, 0x00, 0x07 << 2 } },
                    { 36, new byte[]{ 0x00, 0x01, 0x00 << 2 } },
                    { 37, new byte[]{ 0x00, 0x01, 0x01 << 2 } },
                    { 38, new byte[]{ 0x00, 0x01, 0x02 << 2 } },
                    { 39, new byte[]{ 0x00, 0x01, 0x03 << 2 } },
                    { 40, new byte[]{ 0x00, 0x01, 0x04 << 2 } },
                    { 41, new byte[]{ 0x00, 0x01, 0x05 << 2 } },
                    { 42, new byte[]{ 0x00, 0x01, 0x06 << 2 } },
                    { 43, new byte[]{ 0x00, 0x01, 0x07 << 2 } },
                    { 49, new byte[]{ 0x00, 0x02, 0x00 << 2 } },
                    { 50, new byte[]{ 0x00, 0x02, 0x01 << 2 } },
                    { 139, new byte[]{ 0x00, 0x02, 0x02 << 2 } },
                    { 140, new byte[]{ 0x00, 0x02, 0x03 << 2 } },
                    { 141, new byte[]{ 0x00, 0x02, 0x04 << 2 } },
                    { 142, new byte[]{ 0x00, 0x02, 0x05 << 2 } },
                    { 143, new byte[]{ 0x00, 0x02, 0x06 << 2 } },
                    { 144, new byte[]{ 0x00, 0x02, 0x07 << 2 } },
                    { 145, new byte[]{ 0x00, 0x02, 0x08 << 2 } },
                    { 262, new byte[]{ 0x00, 0x03, 0x00 << 2 } },
                    { 263, new byte[]{ 0x00, 0x03, 0x01 << 2 } },
                    { 264, new byte[]{ 0x00, 0x03, 0x02 << 2 } },
                    { 265, new byte[]{ 0x00, 0x03, 0x03 << 2 } },
                    { 266, new byte[]{ 0x00, 0x03, 0x10 << 2 } },
                    { 267, new byte[]{ 0x00, 0x03, 0x14 << 2 } },
                    { 268, new byte[]{ 0x00, 0x03, 0x10 << 2 } },
                    { 269, new byte[]{ 0x00, 0x03, 0x1C << 2 } },
                    { 30, new byte[]{ 0x00, 0x03, 0x18 << 2 } },
                    { 270, new byte[]{ 0x00, 0x04, 0x00 << 2 } },
                    { 278, new byte[]{ 0x00, 0x04, 0x04 << 2 } },
                };*/

                var info = sub[it.Number.Index];
                CharSet[5] |= info[0];
                CharSet[9] |= info[1];
                CharSet[16] |= info[2];
            }

            if(equip.ContainsKey(Equipament.Pet))
            {
                switch(equip[Equipament.Pet].Number.Number)
                {
                    case 6720:
                        CharSet[16] |= 0x20;
                        break;
                    case 6721:
                        CharSet[16] |= 0x40;
                        break;
                    case 6723:
                        CharSet[10] |= 0x01;
                        CharSet[16] |= 0x80;
                        break;
                    case 6736:
                        CharSet[16] |= 0xE0;
                        break;
                    case 6762:
                        CharSet[16] |= 0xA0;
                        break;
                    case 6779:
                        CharSet[16] |= 0x60;
                        break;
                }
            }

            CharSet[6] = (byte)((SmallLevel >> 16) & 0xff);
            CharSet[7] = (byte)((SmallLevel >>  8) & 0xff);
            CharSet[8] = (byte)(SmallLevel & 0xff);

            return CharSet;
        }

        public byte[] GetCharset()
        {
            return GetCharset(Character.Class, this, Character.Action);
        }

        public async Task Save(GameContext db)
        {
            try
            {
                if (Lock)
                    return;
                var _log = _logger.ForAccount(Character.Player.Session);

                if (_forDelete.Any())
                {
                    _forDelete.ForEach(x => x.Delete(db));
                    _log.Information("Deleting {0} items", _forDelete.Count());

                    _forDelete.Clear();
                }
                await db.SaveChangesAsync();

                foreach (var e in _equipament.Values)
                {
                    await e.Save(db);
                    await db.SaveChangesAsync();
                }

                foreach (var e in _inventory.Items.Values)
                {
                    await e.Save(db);
                    await db.SaveChangesAsync();
                }

                foreach (var e in _personalShop.Items.Values)
                {
                    await e.Save(db);
                    await db.SaveChangesAsync();
                }
            }catch(Exception ex)
            {
                _logger.Error(ex, "Inventory");
            }
        }

        public void DeleteAll(Storage storage)
        {
            _forDelete.AddRange(storage.Items.Values);
            storage.Clear();
        }

        public void DeleteChaosBox() => DeleteAll(ChaosBox);
    }
}
