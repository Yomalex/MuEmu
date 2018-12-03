using MuEmu.Monsters;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Network;
using WebZen.Util;

namespace MuEmu.Network.Game
{
    public class GameServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GameServices));

        [MessageHandler(typeof(CCheckSum))]
        public void CCheckSum(GSSession session, CCheckSum message)
        {
            //session.Player.CheckSum.IsValid(message.Key);
            Logger
                .ForAccount(session)
                .Debug("Key {0:X4}", message.Key);
        }

        [MessageHandler(typeof(CClientMessage))]
        public void CClientMessage(GSSession session, CClientMessage message)
        {
            Logger
                .ForAccount(session)
                .Information("Client Hack Check {0}", message.Flag);
        }

        [MessageHandler(typeof(CAction))]
        public void CAction(GSSession session, CAction message)
        {
            session.Player.Character.Direction = message.Dir;
            var ans = new SAction((ushort)session.Player.Account.ID, message.Dir, message.ActionNumber, message.Target);
            session.SendAsync(ans);

            foreach (var plr in session.Player.Character.PlayersVP)
                plr.Session.SendAsync(ans);
        }

        [MessageHandler(typeof(CMove))]
        public void CMove(GSSession session, CMove message)
        {
            var dirs = new List<Point>
            {
                new Point(-1,-1),
                new Point(0, -1),
                new Point(1, -1),

                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1),

                new Point(-1, 1),
                new Point(-1, 0)
            };

            var @char = session.Player.Character;
            var count = message.Path[0] & 0x0F;
            var solvedPath = new List<Point>();
            var Cpos = new Point(message.X, message.Y);
            solvedPath.Add(Cpos);

            var valid = true;

            for (int i = 1; i <= count; i++)
            {
                var a = (message.Path[(i + 1) / 2] >> (((i % 2) == 1) ? 4 : 0)) & 0x0F;
                Cpos.Offset(dirs[a]);
                solvedPath.Add(Cpos);
                //Logger.Debug("Path solved [{0}] X:{1} Y:{2}", i, Cpos.X, Cpos.Y);
                var att = @char.Map.GetAttributes(Cpos);
                if (att.Where(y => y == Resources.Map.MapAttributes.NoWalk || y == Resources.Map.MapAttributes.Hide).Count() > 0)
                {
                    valid = false;
                }
            }

            if (!valid)
            {
                session.SendAsync(new SPositionSet { Number = (ushort)session.Player.Account.ID.ShufleEnding(), X = (byte)@char.Position.X, Y = (byte)@char.Position.Y });
                Logger
                    .ForAccount(session)
                    .Error("Invalid path");
                return;
            }

            @char.Position = Cpos;

            session.SendAsync(new SMove((ushort)session.Player.Account.ID, (byte)Cpos.X, (byte)Cpos.Y, message.Path[0]));
        }

        [MessageHandler(typeof(CChatNickname))]
        public void CChatNickname(GSSession session, CChatNickname message)
        {
            Logger
                .ForAccount(session)
                .Information("Chat [" + message.Character.MakeString() + "] {0}", message.Message.MakeString());

        }

        [MessageHandler(typeof(CNewQuestInfo))]
        public void CNewQuestInfo(GSSession session, CNewQuestInfo message)
        {
            Logger
                .ForAccount(session)
                .Information("Quest S5 {0}", message.Quest);
            session.SendAsync(message);
        }

        [MessageHandler(typeof(CCloseWindow))]
        public void CCloseWindow(GSSession session)
        {

        }

        [MessageHandler(typeof(CClinetClose))]
        public void CClinetClose(GSSession session, CClinetClose message)
        {
            Logger
                .ForAccount(session)
                .Information("User request {0}", message.Type);

            for(int i = 1; i <= 5; i++)
            {
                SubSystem.Instance.AddDelayedMessage(session.Player, TimeSpan.FromSeconds(5-i), new SNotice(NoticeType.Blue, $"Saldras en {i} segundos"));
            }

            SubSystem.Instance.AddDelayedMessage(session.Player, TimeSpan.FromSeconds(5), new SCloseMsg { Type = message.Type });

            session.Player.Status = message.Type==ClientCloseType.SelectChar?LoginStatus.Logged:LoginStatus.NotLogged;
        }

        [MessageHandler(typeof(CMoveItem))]
        public void CMoveItem(GSSession session, CMoveItem message)
        {
            Logger.Debug("Move item {0}:{1} to {2}:{3}", message.sFlag, message.Source, message.tFlag, message.Dest);

            if (session.Player.Character.Inventory.Move(message.sFlag, message.Source, message.tFlag, message.Dest))
            {
                session.SendAsync(new SMoveItem
                {
                    ItemInfo = message.ItemInfo,
                    Position = message.Dest,
                    Result = (byte)message.tFlag
                });
            }
            else
            {
                session.SendAsync(new SMoveItem
                {
                    ItemInfo = message.ItemInfo,
                    Position = 0xff,
                    Result = (byte)message.tFlag
                });
            }
        }

        // lacting
        [MessageHandler(typeof(CUseItem))]
        public void CUseItem(GSSession session, CUseItem message)
        {

        }

        [MessageHandler(typeof(CEventEnterCount))]
        public void CEventEnterCount(GSSession session, CEventEnterCount message)
        {
            session.SendAsync(new SEventEnterCount { Type = message.Type });
        }

        [MessageHandler(typeof(CTalk))]
        public void CTalk(GSSession session, CTalk message)
        {
            var npcs = ResourceCache.Instance.GetNPCs();
            var ObjectIndex = message.Number.ShufleEnding();
            var obj = MonstersMng.Instance.GetMonster(ObjectIndex);
            if (npcs.TryGetValue(obj.Info.Monster, out var npc))
            {
                if(npc.Shop != null)
                {
                    session.Player.Window = npc.Shop.Storage;
                    session.SendAsync(new STalk { Result = 0 });
                    session.SendAsync(new SShopItemList(npc.Shop.Storage.GetInventory()) { ListType = 0 });
                    session.SendAsync(new STax { Type = TaxType.Shop, Rate = 4 });
                }
                else if(npc.Warehouse)
                {
                    session.Player.Window = session.Player.Account.Vault;
                    session.SendAsync(new SNotice(NoticeType.Blue, $"Active Vault: " + (session.Player.Account.ActiveVault + 1)));
                    session.SendAsync(new STalk { Result = 2 });
                    session.SendAsync(new SShopItemList(session.Player.Account.Vault.GetInventory()));
                    session.SendAsync(new SWarehouseMoney { wMoney = 0, iMoney = 0 });
                }
            }
            else
            {
                //session.SendAsync(new STalk { Result = 0 });
                //session.SendAsync(new STax { Type = TaxType.Warehouse, Rate = 4 });

                //session.SendAsync(new SQuestWindow { Type = 0x01, SubType = 0x01 }); S6EP2
            }
        }

        [MessageHandler(typeof(CBuy))]
        public void CBuy(GSSession session, CBuy message)
        {
            var plr = session.Player;
            var @char = plr.Character;

            if (plr.Window == null)
            {
                throw new ArgumentException("Player isn't in buy/trade/box/Quest", nameof(session.Player.Window));
            }

            if(plr.Window.GetType() != typeof(Storage))
            {
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var shop = plr.Window as Storage;
            var item = shop.Items[message.Position];
            var bResult = new SBuy
            {
                Result = 0xff,
                ItemInfo = item.GetBytes()
            };

            if(item.BuyPrice > @char.Money)
            {
                Logger
                    .ForAccount(session)
                    .Information("Insuficient Money");
                session.SendAsync(bResult);
                return;
            }

            bResult.Result = @char.Inventory.Add(item);
            if(bResult.Result == 0xff)
            {
                Logger
                    .ForAccount(session)
                    .Information("Insuficient Space");
                session.SendAsync(bResult);
                return;
            }

            @char.Money -= item.BuyPrice;

            Logger
                .ForAccount(session)
                .Information("Buy {0} for {1}", item.BasicInfo.Number, item.BuyPrice);

            session.SendAsync(bResult);
        }

        [MessageHandler(typeof(CSell))]
        public void CSell(GSSession session, CSell message)
        {
            if (session.Player.Window == null)
            {
                throw new ArgumentException("Player isn't in buy/trade/box", nameof(session.Player.Window));
            }

            if (session.Player.Window.GetType() != typeof(Storage))
            {
                throw new ArgumentException("Player isn't in buy", nameof(session.Player.Window));
            }

            var shop = session.Player.Window as Storage;
            var inve = session.Player.Character.Inventory;
            var item = inve.Get(message.Position);
            inve.Remove(message.Position);

            session.Player.Character.Money += item.SellPrice;
            var result = new SSell { Result = 1, Money = session.Player.Character.Money };

            session.SendAsync(result);
        }
    }
}
