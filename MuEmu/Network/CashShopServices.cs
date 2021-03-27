using MU.Network.CashShop;
using System;
using System.Collections.Generic;
using System.Text;
using WebZen.Handlers;

namespace MuEmu.Network
{
    public class CashShopServices : MessageHandler
    {
        [MessageHandler(typeof(CCashOpen))]
        public void CashOpen(GSSession session)
        {
            var result = session.Player.Window == null;
            session.SendAsync(new SCashOpen { Result = (byte)(result?1:0) }).Wait();
            session.Player.Window = typeof(MuEmu.CashShop);
        }
        [MessageHandler(typeof(CCashPoints))]
        public void CashPoints(GSSession session)
        {
            session.Player.Character.CashShop.SendPoints();
        }
        [MessageHandler(typeof(CCashInventoryItem))]
        public void CashInventoryItem(GSSession session, CCashInventoryItem message)
        {
            session.Player.Character.CashShop.SendInventory(message);
        }
        [MessageHandler(typeof(CCashItemBuy))]
        public void CashItemBuy(GSSession session, CCashItemBuy message)
        {
            session.Player.Character.CashShop.BuyItem(message);
        }
    }
}
