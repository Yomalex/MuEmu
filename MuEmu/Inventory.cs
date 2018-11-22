using MU.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class Inventory
    {
        private Dictionary<Equipament, Item> _equipament;
        private Dictionary<byte, Item> _inventory;
        private Dictionary<byte, Item> _vault;
        private Dictionary<byte, Item> _chaosBox;
        private Dictionary<byte, Item> _personalShop;

        public Inventory(Character @char, CharacterDto characterDto)
        {
            _equipament = new Dictionary<Equipament, Item>();
            _inventory = new Dictionary<byte, Item>();
        }        
    }
}
