using MuEmu.Network.Game;
using MuEmu.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;

namespace MuEmu.Network.PCPShop
{
    public class PCPShopServices : MessageHandler
    {
        [MessageHandler(typeof(CPCPShopItems))]

        public async Task CPCPShopItems(GSSession session, CPCPShopItems message)
        {
            var its = ResourceCache.Instance.GetPCPointShop().Items;
            var array = from it in ResourceCache.Instance.GetPCPointShop().Items
                        select new PCPShopItemDto
                        {
                            Dur = it.Value.Durability,
                            Exc = it.Value.OptionExe,
                            Index = (byte)it.Value.Number.Index,
                            Opts = (byte)(it.Value.Option28 + (it.Value.Luck?4:0) + it.Value.Plus*8 + (it.Value.Skill ? 128 : 0)),
                            Position = it.Key,
                            Type16 = (byte)((int)it.Value.Number.Type*16),
                            NewOpt1 = it.Value.Slots.Length >= 1 ? it.Value.Slots[0] : SocketOption.None,
                            NewOpt2 = it.Value.Slots.Length >= 2 ? it.Value.Slots[1] : SocketOption.None,
                            NewOpt3 = it.Value.Slots.Length >= 3 ? it.Value.Slots[2] : SocketOption.None,
                            NewOpt4 = it.Value.Slots.Length >= 4 ? it.Value.Slots[3] : SocketOption.None,
                            NewOpt5 = it.Value.Slots.Length >= 5 ? it.Value.Slots[4] : SocketOption.None,
                            unk1 = 0,
                            unk2 = 0,
                        };

            await session.SendAsync(new SPCPShopInfo());
            await session.SendAsync(new SPCPShopItems(array.ToArray()));
        }

        [MessageHandler(typeof(CPCPShopBuy))]
        public async Task CPCPShopBuy(GSSession session, CPCPShopBuy message)
        {
            var PCPS = ResourceCache.Instance.GetPCPointShop();

            var Item = PCPS.Get(message.Position);

            var bResult = new SBuy
            {
                Result = 0xff,
                ItemInfo = Array.Empty<byte>(),
            };

            if(Item.BuyPrice <= session.Player.Character.PCPoints)
            {
                bResult.ItemInfo = Item.GetBytes();
                bResult.Result = session.Player.Character.Inventory.Add(Item);
            }

            await session.SendAsync(bResult);
        }
    }
}
