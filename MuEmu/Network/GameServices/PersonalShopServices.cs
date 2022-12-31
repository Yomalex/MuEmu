using MU.Network.Game;
using MU.Network;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using MuEmu.Util;

namespace MuEmu.Network.GameServices
{
    public partial class GameServices : MessageHandler
    {
        [MessageHandler(typeof(CPShopSetItemPrice))]
        public async Task CPShopSetItemPrice(GSSession session, CPShopSetItemPrice message)
        {
            var @char = session.Player.Character;
            if (@char.Level < 6)
            {
                await session.SendAsync(new SPShopSetItemPrice(PShopResult.LevelTooLow, message.Position));
                return;
            }

            if (message.Price == 0)
            {
                await session.SendAsync(new SPShopSetItemPrice(PShopResult.InvalidPrice, message.Position));
                return;
            }

            var item = @char.Inventory.Get(message.Position);

            if (item == null)
            {
                await session.SendAsync(new SPShopSetItemPrice(PShopResult.InvalidItem, message.Position));
                return;
            }

            item.PShopValueZ = message.Price;
            item.PShopValueB = message.JewelOfBlessPrice;
            item.PShopValueS = message.JewelOfSoulPrice;
            item.PShopValueC = message.JewelOfChaosPrice;

            Logger.ForAccount(session).Information("Update price for {0}, {1}Zen, {2}Bless, {3}Soul, {4}Chaos", item, item.PShopValueZ, item.PShopValueB, item.PShopValueS, item.PShopValueC);
            await session.SendAsync(new SPShopSetItemPrice(PShopResult.Success, message.Position));
        }

        [MessageHandler(typeof(CPShopRequestOpen))]
        public async Task CPShopRequestOpen(GSSession session, CPShopRequestOpen message)
        {
            var @char = session.Player.Character;
            var log = Logger.ForAccount(session);

            if (@char.Map.IsEvent)
            {
                log.Error("Try to open PShop on Event map");
                await session.SendAsync(new SPShopRequestOpen(PShopResult.Disabled));
                return;
            }

            if (@char.Level < 6)
            {
                log.Error("Character Level Too Low ");
                await session.SendAsync(new SPShopRequestOpen(PShopResult.LevelTooLow));
                return;
            }

            if (!@char.Shop.Open)
            {
                log.Information("PShop:{0} Open", message.Name);
                @char.Shop.Name = message.Name;
                @char.Shop.Open = true;
                await session.SendAsync(new SPShopRequestOpen(PShopResult.Success));
                return;
            }

            await session.SendAsync(new SPShopRequestOpen(PShopResult.Disabled));
        }

        [MessageHandler(typeof(CPShopRequestClose))]
        public async Task CPShopRequestClose(GSSession session)
        {
            var @char = session.Player.Character;
            var log = Logger.ForAccount(session);

            if (@char == null)
                return;

            if (!@char.Shop.Open)
            {
                log.Error("PShop isn't open");
            }
            else
            {
                @char.Shop.Open = false;
                log.Error("PShop {0} Closed", @char.Shop.Name);
            }
            var msg = new SPShopRequestClose(PShopResult.Success, (ushort)session.ID);
            await session.SendAsync(msg);
            @char.SendV2Message(msg);
        }

        [MessageHandler(typeof(CPShopRequestList))]
        public async Task CPShopRequestList(GSSession session, CPShopRequestList message)
        {
            var seller = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);

            if (seller == null)
            {
                await session.SendAsync(new SPShopRequestList(PShopResult.InvalidPosition));
                return;
            }

            if (seller == session)
            {
                await session.SendAsync(new SPShopRequestList(PShopResult.Disabled));
                return;
            }

            await session.SendAsync(new SPShopAlterVault { type = 0 });
            var msg = VersionSelector.CreateMessage<SPShopRequestList>(PShopResult.Success, message.Number, seller.Player.Character.Name, seller.Player.Character.Shop.Name, seller.Player.Character.Shop.Items);
            await session.SendAsync(msg);
            session.Player.Window = seller;
            return;
        }

        [MessageHandler(typeof(CPShopCloseDeal))]
        public async Task CPShopCloseDeal(GSSession session, CPShopCloseDeal message)
        {
            var log = Logger.ForAccount(session);
            var seller = session.Player.Window as GSSession;
            session.Player.Window = null;
            if (seller != null)
            {
                log.Information("Close deal with {0}", seller.Player.Character);
            }
            else
            {
                log.Information("Close deal with {0}", message.Name);
            }
        }

        [MessageHandler(typeof(CPShopRequestBuy))]
        public async Task CPShopRequestBuy(GSSession session, CPShopRequestBuy message)
        {
            var seller = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);

            if (seller == null)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.InvalidPosition));
                return;
            }

            if (seller == session)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.Disabled));
                return;
            }

            var @char = seller.Player.Character;
            if (!@char.Shop.Open)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.Disabled));
                return;
            }

            var item = @char.Inventory.PersonalShop.Get(message.Position);

            if (item == null)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.InvalidItem));
                return;
            }

            if (item.PShopValueZ > session.Player.Character.Money)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.LackOfZen));
                return;
            }

            if (@char.Money + item.PShopValueZ > uint.MaxValue)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.ExceedingZen));
                return;
            }

            var Bless = session.Player.Character.Inventory.FindAllItems(7181);
            var BlessC = session.Player.Character.Inventory.FindAllItems(6174);
            var Bless10Pack = BlessC.Where(x => x.Plus == 0);
            var Bless20Pack = BlessC.Where(x => x.Plus == 1);
            var Bless30Pack = BlessC.Where(x => x.Plus == 2);
            var Soul = session.Player.Character.Inventory.FindAllItems(7182);
            var SoulC = session.Player.Character.Inventory.FindAllItems(6175);
            var Soul10Pack = SoulC.Where(x => x.Plus == 0);
            var Soul20Pack = SoulC.Where(x => x.Plus == 1);
            var Soul30Pack = SoulC.Where(x => x.Plus == 2);
            var Chaos = session.Player.Character.Inventory.FindAllItems(6159);
            var ChaosC = session.Player.Character.Inventory.FindAllItems(6285);
            var Chaos10Pack = ChaosC.Where(x => x.Plus == 0);
            var Chaos20Pack = ChaosC.Where(x => x.Plus == 1);
            var Chaos30Pack = ChaosC.Where(x => x.Plus == 2);

            if (item.PShopValueB > Bless.Count() + Bless30Pack.Count() * 30 + Bless20Pack.Count() * 20 + Bless10Pack.Count() * 10)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.LackOfBless));
                return;
            }

            if (item.PShopValueS > Soul.Count() + Soul30Pack.Count() * 30 + Soul20Pack.Count() * 20 + Soul10Pack.Count() * 10)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.LackOfSoul));
                return;
            }

            if (item.PShopValueC > Chaos.Count() + Chaos30Pack.Count() * 30 + Chaos20Pack.Count() * 20 + Chaos10Pack.Count() * 10)
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.LackOfChaos));
                return;
            }

            int jewel = item.PShopValueB;
            var pickB30 = Math.Min(jewel / 30, Bless30Pack.Count());
            jewel -= pickB30 * 30;
            var pickB20 = Math.Min(jewel / 20, Bless20Pack.Count());
            jewel -= pickB20 * 20;
            var pickB10 = Math.Min(jewel / 10, Bless10Pack.Count());
            jewel -= pickB10 * 10;
            var pickB1 = jewel;

            jewel = item.PShopValueS;
            var pickS30 = Math.Min(jewel / 30, Soul30Pack.Count());
            jewel -= pickS30 * 30;
            var pickS20 = Math.Min(jewel / 20, Soul20Pack.Count());
            jewel -= pickS20 * 20;
            var pickS10 = Math.Min(jewel / 10, Soul10Pack.Count());
            jewel -= pickS10 * 10;
            var pickS1 = jewel;

            jewel = item.PShopValueC;
            var pickC30 = Math.Min(jewel / 30, Chaos30Pack.Count());
            jewel -= pickC30 * 30;
            var pickC20 = Math.Min(jewel / 20, Chaos20Pack.Count());
            jewel -= pickC20 * 20;
            var pickC10 = Math.Min(jewel / 10, Chaos10Pack.Count());
            jewel -= pickC10 * 10;
            var pickC1 = jewel;

            var transfer = Bless30Pack.Take(pickB30).ToList();
            transfer.AddRange(Bless20Pack.Take(pickB20));
            transfer.AddRange(Bless10Pack.Take(pickB10));
            transfer.AddRange(Bless.Take(pickB1));

            transfer.AddRange(Soul30Pack.Take(pickS30));
            transfer.AddRange(Soul20Pack.Take(pickS20));
            transfer.AddRange(Soul10Pack.Take(pickS10));
            transfer.AddRange(Soul.Take(pickS1));

            transfer.AddRange(Chaos30Pack.Take(pickC30));
            transfer.AddRange(Chaos20Pack.Take(pickC20));
            transfer.AddRange(Chaos10Pack.Take(pickC10));
            transfer.AddRange(Chaos.Take(pickC1));

            if (!@char.Inventory.TryAdd(transfer))
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.SellerInventoryFull));
                return;
            }

            if (!session.Player.Character.Inventory.TryAdd(new[] { item }))
            {
                await session.SendAsync(new SPShopRequestBuy(PShopResult.InvalidPosition));
                return;
            }

            session.Player.Character.Inventory.Remove(transfer);
            @char.Inventory.Add(transfer);

            await @char.Inventory.Remove(message.Position, true);
            var res = session.Player.Character.Inventory.Add(item);

            session.Player.Character.Money -= item.PShopValueZ;
            @char.Money += item.PShopValueZ;

            await session.SendAsync(new SPShopRequestBuy(PShopResult.Success, message.Number, item.GetBytes(), res));
            await @char.Player.Session.SendAsync(new SPShopRequestSold(PShopResult.Success, message.Position, session.Player.Character.Name));

            if (@char.Inventory.PersonalShop.Items.Count == 0)
            {
                await CPShopRequestClose(@char.Player.Session);
            }
        }

        [MessageHandler(typeof(CPShopSearchItem))]
        public async Task CPShopSearchItem(GSSession session, CPShopSearchItem message)
        {
            IEnumerable<PShop> shopList;

            shopList = from cl in Program.server.Clients
                       where
                       cl.Player != null &&
                       cl.Player.Status == LoginStatus.Playing &&
                       cl.Player.Character.Shop.Open == true &&
                       (cl.Player.Character.MapID == Maps.Lorencia || cl.Player.Character.MapID == Maps.Davias || cl.Player.Character.MapID == Maps.Noria || cl.Player.Character.MapID == Maps.Elbeland || cl.Player.Character.MapID == Maps.LorenMarket)
                       select cl.Player.Character.Shop;

            if (message.sSearchItem != -1)
            {
                shopList = from cl in shopList
                           where
                           cl.Chararacter.Inventory.PersonalShop.Items.Values.Count(x => x.Number.Number == (ushort)message.sSearchItem) != 0
                           select cl;
            }

            shopList = shopList.Skip(message.iLastCount).Take(50);
            var msg = new SPShopSearchItem
            {
                iPShopCnt = shopList.Count(),
                btContinueFlag = (byte)(shopList.Count() == 50 ? 1 : 0),
                List = shopList.Select(x => new SPShopSearchItemDto
                {
                    Number = x.Chararacter.Player.ID,
                    szName = x.Chararacter.Name,
                    szPShopText = x.Name,
                }).ToArray()
            };
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CPShopSearch))]
        public async Task CPShopRequestListS16Kor(GSSession session, CPShopSearch message)
        {
            var shopList = from cl in Program.server.Clients
                       where
                       cl.Player != null &&
                       cl.Player.Status == LoginStatus.Playing &&
                       cl.Player.Character.Shop.Open == true &&
                       (cl.Player.Character.MapID == Maps.Lorencia || cl.Player.Character.MapID == Maps.Davias || cl.Player.Character.MapID == Maps.Noria || cl.Player.Character.MapID == Maps.Elbeland || cl.Player.Character.MapID == Maps.LorenMarket)
                       select cl.Player.Character.Shop;

            /*if (message.Number != -1)
            {
                shopList = from cl in shopList
                           where
                           cl.Chararacter.Inventory.PersonalShop.Items.Values.Count(x => x.Number.Number == (ushort)message.sSearchItem) != 0
                           select cl;
            }*/
            await session.SendAsync(new SPShopSearch
            {
                Count = (uint)shopList.Count(),
                Number = message.Number,
                List = shopList.Select(x => new SPShopSearchDto
                {
                    Seller = x.Chararacter.Name,
                    Description = x.Name,
                }).ToArray()
            });
        }

        [MessageHandler(typeof(CPShopItemSearch2))]
        public async Task CPShopItemSearch(GSSession session, CPShopItemSearch2 message)
        {
            var shopList = from cl in Program.server.Clients
                           where
                           cl.Player != null &&
                           cl.Player.Status == LoginStatus.Playing &&
                           cl.Player.Character.Shop.Open == true &&
                           (cl.Player.Character.MapID == Maps.Lorencia || cl.Player.Character.MapID == Maps.Davias || cl.Player.Character.MapID == Maps.Noria || cl.Player.Character.MapID == Maps.Elbeland || cl.Player.Character.MapID == Maps.LorenMarket)
                           select cl.Player.Character.Shop;

            var list = shopList.SelectMany(x =>
            {
                var seller = x.Chararacter.Name;
                var description = x.Name;
                return x.Items.Select(y => new SPShopItemSearchDto
                {
                    Seller = seller,
                    ItemInfo = y.Item,
                    JOBless = y.BlessValue,
                    JOSoul = y.SoulValue,
                    Zen = y.Price,
                    Slot = y.Pos,
                });
            });

            if(message.Number != 0xffff)
            {
                list = list.Where(x => (new Item(x.ItemInfo)).Number == message.Number);
            }else if(message.Name.Length > 0)
            {
                list = list.Where(x => (new Item(x.ItemInfo)).BasicInfo.Name.ToLower().Contains(message.Name.ToLower()));
            }
            await session.SendAsync(new SPShopItemSearch
            {
                Count = (uint)list.Count(),
                Number = message.Number,
                List = list.ToArray()
            });
        }

        [MessageHandler(typeof(CPShopItemSearch))]
        public async Task CPShopItemSearch(GSSession session, CPShopItemSearch message)
        {
            await CPShopItemSearch(session, new CPShopItemSearch2 { Item = 0xffff, Name = "", Number = message.Number });
        }

        [MessageHandler(typeof(CPShopRequestList2S16Kor))]
        public async Task CPShopRequestList2S16Kor(GSSession session, CPShopRequestList2S16Kor message)
        {
            var shop = session.Player.Character.Shop;
            await session.SendAsync(new SPShopSellList
            {
                Result = 1,
                Description = shop.Name,
                state = (byte)(shop.Open ? 1 : 0),
                Number = message.Number,
                List = shop.Items.Select(x => new SPShopItemSellListDto
                {
                    JOBless = x.BlessValue,
                    JOSoul = x.SoulValue,
                    ItemInfo = x.Item,
                    Zen = x.Price,
                    Slot = x.Pos,
                    Bundle = 0,
                }).ToArray()
            });
        }

        [MessageHandler(typeof(CPShopSetItemPriceS16Kor))]
        public async Task CPShopSetItemPriceS16Kor(GSSession session, CPShopSetItemPriceS16Kor message)
        {
            var shop = session.Player.Character.Shop;

            var items = message.Items.Select(x => session.Player.Character.Inventory.Get((byte)x)).ToList();
            var i = 0;
            foreach (var item in items)
            {
                session.Player.Character.Inventory.Remove(item, true);
                session.Player.Character.Inventory.PersonalShop.Add((byte)(StorageID.PersonalShop+message.Slot * 5 + i), item);
            }

            await session.SendAsync(new SPShopSetItemPriceS16Kor
            {
                ItemInfo = items.First().GetBytes(),
                Number = 1,
                Bundle = (byte)(items.Count > 1?2:1),
                JOBless = message.JewelOfBlessPrice,
                JOSoul = message.JewelOfSoulPrice,
                Result = message.Changed,
                Slot = message.Slot,
                Zen = message.Price,
            });
        }

        [MessageHandler(typeof(CPShopChangeStateS16Kor))]
        public async Task CPShopChangeStateS16Kor(GSSession session, CPShopChangeStateS16Kor message)
        {
            var shop = session.Player.Character.Shop;
            shop.Open = message.State == 1;
            shop.Name = message.Description;
            await session.SendAsync(new SPShopChangeStateS16Kor
            {
                Number = message.Number,
                Result = 0,
                State = (byte)(shop.Open ? 1 : 0)
            });
        }
    }
}
