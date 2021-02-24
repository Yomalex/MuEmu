using MU.DataBase;
using MuEmu.Network;
using MuEmu.Network.CashShop;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

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
            //DataTimeStart = int.Parse(vs[10]);
            //DataTimeExpir = int.Parse(vs[11]);
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
        private List<Item> _items = new List<Item>();

        private static Dictionary<int, IBSCategory> cat;
        private static Dictionary<int, IBSPackage> pack;

        public static void Initialize(ushort ver1, ushort ver2, ushort ver3)
        {
            version = new ushort[] { ver1, ver2, ver3 };
            var root = $"./Data/CashShop/{ver1}.{ver2}.{ver3}/";

            cat = new Dictionary<int, IBSCategory>();
            pack = new Dictionary<int, IBSPackage>();

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

                    var a = new IBSPackage(subs);
                    pack.Add(a.CategoryId*1000 + a.Position, a);
                }
            }
        }

        public CashShop(GSSession session, CharacterDto db)
        {
            session.SendAsync(new SCashInit()).Wait();
            session.SendAsync(new SCashVersion { Ver1 = version[0], Ver2 = version[1], Ver3 = version[2] }).Wait();
            session.SendAsync(new SCashBanner { Ver1 = 583, Ver2 = 2014, Ver3 = 001 }).Wait();

            switch(Program.Season)
            {
                case 9:
                    session.SendAsync(new SCashPointsS9()).Wait();
                    break;
                default:
                    session.SendAsync(new SCashPoints()).Wait();
                    break;
            }

            _player = session.Player;
            log = Logger.ForAccount(session);
        }

        public void SendPoints()
        {
            _player.Session.SendAsync(new SCashPointsS9
            {
                ViewType = 0,
                Cash_C = _wCoinC,
                Cash_P = _wCoinP,
                GoblinPoint = _goblinPoints,
                TotalCash = _wCoinC,
                TotalPoint = _wCoinP + _goblinPoints,
            });
        }

        public async void SendInventory(CCashInventoryItem message)
        {
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

            if(message.InventoryType == 0x53)
            {

            }
            else
            {

            }
        }

        public async void BuyItem(CCashItemBuy message)
        {
            var a = (from p in pack
                     where (p.Value.CategoryId == message.Category) &&
                     (p.Value.ItemGroupAndIndex == message.ItemID) &&
                     (p.Value.GameId == message.ItemIndex)
                     select p.Value).FirstOrDefault();

            log.Debug("Buy CashItem Cat:{0} ID:{1}, Index:{2}, SubIndex{3}, Opt:{4}, {5}", message.Category, message.ItemID, message.ItemIndex, message.Coin, message.ItemOpt, a?.Name??"ERR");

            await _player.Session.SendAsync(new SCashItemBuy { Result = 1 });
        }
    }
}
