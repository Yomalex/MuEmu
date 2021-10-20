using MU.Network.MuunSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using System.Linq;
using MuEmu.Util;

namespace MuEmu.Network.GameServices
{
    public partial class GameServices
    {
        [MessageHandler(typeof(CMuunItemRideSelect))]
        public async Task CMuunRideReq(GSSession session, CMuunItemRideSelect message)
        {
            await session.SendAsync(new SMuunRideVP { ViewPort = new MuunRideVPDto[] { new MuunRideVPDto(session.Player.ID, 0xffff) } });
        }

        [MessageHandler(typeof(CMuunItemGet))]
        public async Task CMuunItemGet(GSSession session, CMuunItemGet message)
        {
            var @char = session.Player.Character;
            Item pickup;

            try
            {
                pickup = @char.Map.ItemPickUp(@char, message.Number);
            }
            catch (Exception ex)
            {
                session.Exception(ex);
                return;
            }

            var pos = @char.Inventory.AddMuun(pickup);

            var msg = new SMuunItemGet { Item = pickup.GetBytes(), Result = pos, };
            await session.SendAsync(msg);
        }

    }
}
