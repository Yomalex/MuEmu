using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Item")]
    public class ItemDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateCreation { get; set; }

        public int AccountId { get; set; }
        public AccountDto Account { get; set; }

        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }

        public int VaultId { get; set; }
        public int SlotId { get; set; }

        public ushort Number { get; set; }

        public short Plus { get; set; }

        public bool Luck { get; set; }

        public bool Skill { get; set; }

        public short Durability { get; set; }

        public short Option { get; set; }

        public short OptionExe { get; set; }

        public short HarmonyOption { get; set; }

        public string SocketOptions { get; set; }
    }
}
