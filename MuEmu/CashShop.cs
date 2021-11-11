using MU.DataBase;
using MuEmu.Network;
using MU.Network.CashShop;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using MU.Resources;
using MU.Network;

namespace MuEmu
{
    class IBSCategory
    {
        public int CategoryId;            //10
        public string Name;                    //W Coin (C)
        public int Const1;                //200
        public int Const2;                //201
        public int GroupId;                //10
        public int Position;            //1
        public bool IsRoot;                //1

        public IBSCategory(string[] vs)
        {
            CategoryId = int.Parse(vs[0]);
            Name = vs[1];
            Const1 = int.Parse(vs[2]);
            Const2 = int.Parse(vs[3]);
            GroupId = int.Parse(vs[4]);
            Position = int.Parse(vs[5]);
            IsRoot = int.Parse(vs[6])==1;
        }
    }
    class IBSPackage
    {
        public int CategoryId;              //13
        public int Position;                //1
        public int GameId;                  //263
        public string Name;                 //Rage Fighter Character Card
        public int Const1;                  //170
        public int Price;                   //500
        public string Description;              //A unlimited number of Rage Fighter can be created in one server. Only 1 Rage Fighter card is needed per server.
        public int Const2;                  //182
        public int Unknow;                  //185
        public DateTime DataTimeStart;              //20110608211000
        public DateTime DataTimeExpir;              //20150609141000
        public int Const3;                  //177
        public int Const4;                 //1
        public string CoinDesc;             //W Coin(C)
        public string CoinDesc2;                //W Coin(C) //Front type
        public int Const5;              //181
        public int Const6;              //200
        public int Const7;                  //0
        public int[] ProductRootId;         //284|
        public int ItemGroupAndIndex;       //7337
        public int coinType;            //2
        public int ProductCount;            //1
        public int[] ProductNodeId;     //353|
        public int CoinType1;               //0
        public int CoinType2;               //508
        public int Const8;	                //669


        public IBSPackage(string[] vs)
        {
            CategoryId = int.Parse(vs[0]);
            Position = int.Parse(vs[1]);
            GameId = int.Parse(vs[2]);
            Name = vs[3];
            Const1 = int.Parse(vs[4]);
            Price = int.Parse(vs[5]);
            Description = vs[6];
            int.TryParse(vs[8], out Const2);
            Unknow = int.Parse(vs[9]);

            int year, month, day, h, m, s;
            if (vs[10].Length >= 14)
            {
                year = int.Parse(vs[10].Substring(0, 4));
                month = int.Parse(vs[10].Substring(4, 2));
                day = int.Parse(vs[10].Substring(6, 2));
                h = int.Parse(vs[10].Substring(8, 2));
                m = int.Parse(vs[10].Substring(10, 2));
                s = int.Parse(vs[10].Substring(12, 2));
                DataTimeStart = new DateTime(year, month, day, h, m, s);
            }else
            {
                DataTimeStart = DateTime.Now;
            }
            if (vs[11].Length >= 14)
            {
                year = int.Parse(vs[11].Substring(0, 4));
                month = int.Parse(vs[11].Substring(4, 2));
                day = int.Parse(vs[11].Substring(6, 2));
                h = int.Parse(vs[10].Substring(8, 2));
                m = int.Parse(vs[10].Substring(10, 2));
                s = int.Parse(vs[10].Substring(12, 2));
                DataTimeExpir = new DateTime(year, month, day, h, m, s);
            }
            else
            {
                DataTimeExpir = DateTime.Now.AddYears(100);
            }

            Const3 = int.Parse(vs[12]);
            Const4 = int.Parse(vs[13]);
            CoinDesc = vs[14];
            CoinDesc2 = vs[15];
            Const5 = int.Parse(vs[16]);
            Const6 = int.Parse(vs[17]);
            Const7 = int.Parse(vs[18]);
            ProductRootId = vs[19].Split("|").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => int.Parse(x)).ToArray();
            ItemGroupAndIndex = int.Parse(vs[20]);
            coinType = int.Parse(vs[21]);
            ProductCount = int.Parse(vs[22]);
            ProductNodeId = vs[23].Split("|").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => int.Parse(x)).ToArray();
            CoinType1 = int.Parse(vs[24]);
            CoinType2 = int.Parse(vs[25]);
            Const8 = int.Parse(vs[26]);
        }
    }
    enum SellType
    {
        Duration,
        Quantity,
    }
    class IBSProduct
    {
        //3@Seal of Wealth(3day)@Duration@259200@Sec.@0@3@142@145@1@144@673@518@6700@10@140@386
        //12@ Devil Square Ticket @Quantity@1@EA@900@21@142@145@1@144@673@518@6702@7@138@680
        //60@ Pet Panda@Duration@1440@[1 Day(s)]@150@99@142@145@1@144@673@518@6736@10@138@680
        public int RootId;
        public string Name;
        public SellType sellType;
        public int Amount;
        public string sellTypeDesc;
        public int Coins;
        public int NodeId;
        public int Const1;
        public int Const2;
        public int Const3;
        public int Const4;
        public int Const5;
        public int Const6;
        public int ItemId;
        public int Const7;
        public int Const8;
        public int Const9;

        public IBSProduct(string[] vs)
        {
            RootId = int.Parse(vs[0]);
            Name = vs[1];
            Enum.TryParse(vs[2], out sellType);
            Amount = int.Parse(vs[3]);
            sellTypeDesc = vs[4];
            Coins = int.Parse(vs[5]);
            NodeId = int.Parse(vs[6]);
            Const1 = int.Parse(vs[7]);
            Const2 = int.Parse(vs[8]);
            Const3 = int.Parse(vs[9]);
            Const4 = int.Parse(vs[10]);
            Const5 = int.Parse(vs[11]);
            Const6 = int.Parse(vs[12]);
            ItemId = int.Parse(vs[13]);
            Const7 = int.Parse(vs[14]);
            Const8 = int.Parse(vs[15]);
            Const9 = int.Parse(vs[16]);
        }
    }
    class CashShopItem
    {

    }

    public class CashShop
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CashShop));
        private ILogger log;
        private static ushort[] version;
        private static int[] banner;
        private Player _player;
        private int _wCoinP;
        private int _wCoinC;
        private int _goblinPoints;
        private List<Item> _gifItems = new List<Item>();
        private List<Item> _storage = new List<Item>();

        private static Dictionary<int, IBSCategory> cat;
        private static Dictionary<int, IBSPackage> pack;
        private static Dictionary<int, IBSProduct> prod;

        public static void Initialize(ushort[] ver)
        {
            version = new ushort[] { ver[0], ver[1], ver[2] };
            var root = $"./Data/CashShop/{ver[0]}.{ver[1]}.{ver[2]}/";

            cat = new Dictionary<int, IBSCategory>();
            pack = new Dictionary<int, IBSPackage>();
            prod = new Dictionary<int, IBSProduct>();

            using (var fs = File.OpenText(root+ "IBSCategory.txt"))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    var subs = line.Split("@");

                    var a = new IBSCategory(subs);
                    cat.Add(a.CategoryId, a);
                }
            }

            using (var fs = File.OpenText(root + "IBSPackage.txt"))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    var subs = line.Split("@");
                    try
                    {
                        var a = new IBSPackage(subs);
                    pack.Add(a.CategoryId*1000 + a.Position, a);
                    }
                    catch (Exception ex) {
                        Logger.Error("",ex);
                    }
                }
            }

            using (var fs = File.OpenText(root + "IBSProduct.txt"))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    var subs = line.Split("@");

                    try
                    {
                        var a = new IBSProduct(subs);
                        prod.Add(a.NodeId, a);
                    }
                    catch (Exception) { }
                }
            }
        }

        public bool IsOpen { get; set; }

        public CashShop(GSSession session, CharacterDto db)
        {
            session.SendAsync(new SCashInit()).Wait();
            session.SendAsync(new SCashVersion { Ver1 = version[0], Ver2 = version[1], Ver3 = version[2] }).Wait();
            session.SendAsync(new SCashBanner { Ver1 = 583, Ver2 = 2014, Ver3 = 001 }).Wait();
            session.SendAsync(VersionSelector.CreateMessage<SCashPoints>(_wCoinC, _wCoinP, _goblinPoints)).Wait();

            _player = session.Player;
            log = Logger.ForAccount(session);
        }

        public void SendPoints()
        {
            _player.Session.SendAsync(VersionSelector.CreateMessage<SCashPoints>(_wCoinC, _wCoinP, _goblinPoints)).Wait();
        }

        public async void SendInventory(CCashInventoryItem message)
        {
            var _items = message.InventoryType == CSInventory.Storage ? _storage : _gifItems;

            var tPage = (ushort)Math.Ceiling(_items.Count / 8.0);
            var id = (message.Page - 1) * 8;
            var cicount = (ushort)Math.Max(_items.Count - id, 8);

            await _player.Session.SendAsync(new SCashInventoryItem
            {
                PageIndex = (ushort)message.Page,
                TotalPage = tPage,
                CurrentItemCount = cicount,
                TotalItemCount = (ushort)_items.Count,
            });

            var items = _items.Select(x => new SCashItemDto
            {
                InventoryType = message.InventoryType,
                AuthCode = 1,
                GiftName = "",
                Message = "",
                UniqueCode = 2,
                UniqueID1 = 3,
                UniqueID2 = 4,
                UniqueID3 = 5,
            }).ToArray();

            var msg = new SCashItemList
            {
                aIndex = _player.ID,
                AccountID = _player.Account.Nickname,
                InvType = message.InventoryType,
                InvNum = (byte)id,
                Result = 1,
                Items = items
            };
            Logger.Debug("Sending list from {0}", message.InventoryType);
            await _player.Session.SendAsync(msg);
        }

        public async void BuyItem(CCashItemBuy message)
        {
            var category = (from c in cat
                            where c.Value.CategoryId == message.Category
                            select c.Value).First();

            var package = (from p in pack
                           where p.Value.GameId == message.ItemIndex
                           select p.Value).First();

            var products = (from p in prod
                           where package.ProductNodeId.Contains(p.Value.NodeId) || package.ProductRootId.Contains(p.Value.RootId)
                           select p.Value);

            var product = (from p in products
                          where message.ItemOpt == 0 || p.NodeId == message.ItemOpt
                          select p)
                          .Skip(Program.RandomProvider(package.ProductRootId.Length))
                          .First();

            var neededCoins = product.Coins != 0 ? product.Coins : package.Price;

            log.Debug("Buy CashItem Cat:{0}->{1} ID:{2}, {3}{4}", category.Name, product.Name, message.ItemID, neededCoins, message.Coin);

            CSResult result = CSResult.Ok;

            switch(message.Coin)
            {
                case CoinType.GPoints:
                    if(_goblinPoints < neededCoins)
                    {
                        //result = CSResult.InsuficientWCoint;
                        break;
                    }
                    break;
                case CoinType.WCoin:
                    if (_wCoinC < neededCoins)
                    {
                        //result = CSResult.InsuficientWCoint;
                        break;
                    }
                    break;
            }

            if(package.DataTimeStart > DateTime.Now)
            {
                result = CSResult.ItemIsNotCurrentAvailable;
            }

            if(package.DataTimeExpir < DateTime.Now)
            {
                //result = CSResult.ItemIsNotLongerAvailable;
            }

            if(CSResult.Ok == result)
            {
                _storage.Add(new Item((ushort)product.ItemId));
            }

            await _player.Session.SendAsync(new SCashItemBuy { Result = result });
            SendInventory(new CCashInventoryItem { Page = 0, InventoryType = 0 });
        }
    }
}
