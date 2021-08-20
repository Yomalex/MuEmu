using MU.DataBase;
using MU.Network.Game;
using MU.Resources;
using MuEmu.Entity;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    internal class  GremoryCaseItem
    {
        public GremoryStorage RewardInventory { get; set; }
        public GremorySource RewardSource { get; set; }
        public uint ItemGUID { get; set; }
        public uint AuthCode { get; set; }
        public DateTime ExpireTime { get; set; }
        public Item Item { get; set; }
    }
    public class GremoryCase
    {
        public const int MaxItems = 50;

        private List<GremoryCaseItem> _items = new List<GremoryCaseItem>();
        public Character Character { get; private set; }

        public GremoryCase(Character @char, CharacterDto dto)
        {
            Character = @char;
            dto.GremoryCases.Select(x => new GremoryCaseItem
            {
                AuthCode = x.Auth,
                ExpireTime = x.ExpireTime,
                ItemGUID = (uint)x.GiftId, //x.ItemGUID,
                RewardInventory = (GremoryStorage)x.Inventory,
                RewardSource = (GremorySource)x.Source,
                Item = new Item(x.ItemNumber, Options: new { x.Luck, Option28 = x.Option, x.Skill, x.Plus, x.OptionExe }),
            });
            SendList();
        }

        public void CheckStorageExpiredItems()
        {
            var items = _items
                .Where(x => x.ExpireTime < DateTime.Now)
                .Select(x => x.ItemGUID)
                .ToList();

            items.ForEach(x => RemoveItem(x));
        }

        public async void CheckIsInStorageItemAboutToExpire()
        {
            var nextWeek = DateTime.Now.AddDays(7);
            var items = _items.Count(x => x.ExpireTime < nextWeek && x.ExpireTime > DateTime.Now);
            if(items != 0)
            {
                await Character.Player.Session.SendAsync(new SGremoryCaseNotice
                {
                    Status = GremoryNotice.ItemAboutToExpire,
                });
            }
        }

        public async void CheckInventoryCount()
        {
            var c1 = _items.Count(x => x.RewardInventory == GremoryStorage.Character);
            var c2 = _items.Count(x => x.RewardInventory == GremoryStorage.Server);

            if ((c1 >= MaxItems-5 && c1 < MaxItems) || (c2 >= MaxItems - 5 && c2 < MaxItems))
            {
                await Character.Player.Session.SendAsync(new SGremoryCaseNotice
                {
                    Status = GremoryNotice.InventoryToBeFilled,
                });
            }else if(c1 >= MaxItems || c2 >= MaxItems)
            {
                await Character.Player.Session.SendAsync(new SGremoryCaseNotice
                {
                    Status = GremoryNotice.InventoryFull,
                });
            }
        }

        public async void SendList()
        {
            await Character.Player.Session.SendAsync(new SGremoryCaseList 
            { 
                List = _items.Select(x => new GCItemDto 
                { 
                    AuthCode = x.AuthCode, 
                    ExpireTime = (uint)x.ExpireTime.ToTimeT(), 
                    ItemGUID = x.ItemGUID, 
                    ItemInfo = x.Item.GetBytes(), 
                    RewardInventory = x.RewardInventory, 
                    RewardSource = x.RewardSource 
                }).ToArray() 
            });

            CheckIsInStorageItemAboutToExpire();
            CheckStorageExpiredItems();
            CheckInventoryCount();
        }

        public async void AddItem(Item it, DateTime ExpireTime, GremoryStorage storage, GremorySource source)
        {
            using(var db = new GameContext())
            {
                var tmp = new GremoryCaseDto
                {
                    AccountId = Character.Account.ID,
                    Auth = 0,//???
                    CharacterId = null,
                    Durability = it.Durability,
                    ExpireTime = ExpireTime,
                    //HarmonyOption = it.Harmony
                    Inventory = (byte)storage,
                    ItemNumber = it.Number,
                    Luck = it.Luck,
                    Option = it.Option28,
                    OptionExe = it.OptionExe,
                    Plus = it.Plus,
                    Skill = it.Skill,
                    Source = (byte)source,
                };

                if(storage == GremoryStorage.Character)
                {
                    tmp.CharacterId = Character.Id;
                }

                db.GremoryCase.Add(tmp);
                db.SaveChanges();

                await Character.Player.Session.SendAsync(new SGremoryCaseReceiveItem
                {
                    Item = new GCItemDto
                    {
                        AuthCode =tmp.Auth,
                        ExpireTime = (uint)tmp.ExpireTime.ToTimeT(),
                        ItemGUID = (uint)tmp.GiftId,
                        RewardInventory = storage,
                        RewardSource = source,
                        ItemInfo = it.GetBytes()
                    }
                });
            }
        }

        public async void RemoveItem(uint GUID)
        {
            var it = _items.First(x => x.ItemGUID == GUID);
            _items.Remove(it);

            await Character.Player.Session.SendAsync(new SGremoryCaseDelete
            {
                AuthCode = it.AuthCode,
                ItemGUID = it.ItemGUID,
                ItemNumber = it.Item.Number,
                StorageType = it.RewardInventory,
            });

            using (var db = new GameContext())
            {
                var itDB = db.GremoryCase.First(x => x.GiftId == GUID);
                db.GremoryCase.Remove(itDB);
                db.SaveChanges();
            }
        }
    }
}
