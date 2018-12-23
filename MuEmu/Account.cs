using MU.DataBase;
using MuEmu.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Account
    {
        private Dictionary<byte, Storage> _vaults { get; set; }
        public int ID { get; set; }

        public string Nickname { get; set; }

        public Player Player { get; set; }

        public Dictionary<byte, CharacterDto> Characters { get; set; }

        public byte ActiveVault { get; set; }
        public Storage Vault => _vaults[ActiveVault];
        public int VaultMoney { get; set; }

        public Account(Player player, AccountDto accountDto)
        {
            Player = player;
            ActiveVault = 0;

            _vaults = new Dictionary<byte, Storage>();

            using (var db = new GameContext())
                for (var i = (byte)0; i < accountDto.VaultCount; i++)
                {
                    _vaults.Add(i, new Storage(Storage.WarehouseSize));
                    var items = db.Items
                        .Where(x => x.VaultId == accountDto.AccountId * 10 + i);

                    foreach(var it in items)
                        _vaults[i].Add(new Item(it));
                }

            Nickname = accountDto.Account;
            ID = accountDto.AccountId;
            byte y = 0;
            Characters = accountDto.Characters.ToDictionary(x => y++);
            VaultMoney = accountDto.VaultMoney;
        }
    }
}
