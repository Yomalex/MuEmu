using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu.Events.LuckyCoins
{
    public class LuckyCoins
    {
        private Dictionary<Player, uint> _register;
        public static LuckyCoins Instance { get; set; }

        private LuckyCoins()
        {
            _register = new Dictionary<Player, uint>();
        }

        public static async Task<uint> Registered(Player plr)
        {
            if(Instance._register.ContainsKey(plr))
                return Instance._register[plr];

            return 0;
        }

        public static async Task<uint> Registre(Player plr)
        {
            var inv = plr.Character.Inventory;
            var luckyItemsPos = inv.FindAll(new ItemNumber(14, 100));
            var luckyItems = inv.Get(luckyItemsPos);
            var reg = Instance._register;

            if (!reg.ContainsKey(plr))
                reg.Add(plr, 0);

            foreach (var luckyCoin in luckyItems)
            {
                if(luckyCoin.Durability > 0)
                {
                    reg[plr] += luckyCoin.Durability;
                    await inv.Delete(luckyCoin);
                }
            }

            return reg[plr];
        }

        public static void Initialize()
        {
            if (Instance != null)
                throw new Exception("LuckyCoins Already Initialized!");

            Instance = new LuckyCoins();
        }
    }
}
