using MU.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class Account
    {
        private Dictionary<byte, Storage> _vaults { get; set; }
        public int ID { get; set; }

        public string Nickname { get; set; }

        public Player Player { get; set; }

        public Dictionary<int, string> Characters { get; set; }

        public byte ActiveVault { get; set; }
        public Storage Vault => _vaults[ActiveVault];

        public Account(Player player, AccountDto accountDto)
        {
            Player = player;
            ActiveVault = 0;

            Characters = new Dictionary<int, string>();
            _vaults = new Dictionary<byte, Storage>();
            _vaults.Add(0, new Storage(Storage.WarehouseSize));
            Vault.Add(new Item(new ItemNumber(0, 0), 0));
            Nickname = accountDto.Account;
            ID = accountDto.ID;

            if (accountDto.Character1 != null)
            {

            }
            if (accountDto.Character2 != null)
            {

            }
            if (accountDto.Character3 != null)
            {

            }
            if (accountDto.Character4 != null)
            {

            }
            if (accountDto.Character5 != null)
            {

            }
        }
    }
}
