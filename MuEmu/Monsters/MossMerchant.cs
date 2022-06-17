using MU.Network;
using MU.Network.Game;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MuEmu.Monsters
{
    internal class MossMerchant
    {
        internal static void Talk(Player player)
        {
            _ = player.Session.SendAsync(new STalk { Result = (NPCWindow)0x38 });
            var ibags = Resources.ResourceCache.Instance.GetItemBags();
            var availableBags = ibags
                .Where(x => x.Monster == 0x1EC)
                .Select(x => x.Plus);

            var list = new byte[11];
            foreach(var x in availableBags)
            {
                list[x] = 1;
            }    

            _ = player.Session.SendAsync(new SMossMerchantOpen
            {
                List = list
            });
            var msg = VersionSelector.CreateMessage<SItemGet>(player.Character.Money, (ushort)0xffff);
            _ = player.Session.SendAsync(msg);
        }

        internal static Item Gamble(Player player, int section)
        {
            var ibags = Resources.ResourceCache.Instance.GetItemBags();
            var bag = ibags.Where(x => x.Monster == 0x1EC && x.Plus == section).Single();
            return bag.GetReward().First();
        }
    }
}
