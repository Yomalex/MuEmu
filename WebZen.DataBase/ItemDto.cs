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

        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Required]
        public int AccountId { get; set; }
        public AccountDto Account { get; set; }

        public int CharacterId { get; set; } = 0;

        public int VaultId { get; set; }
        public int SlotId { get; set; }

        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort Number { get; set; }

        public byte Plus { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool Luck { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool Skill { get; set; }

        [Column(TypeName = "TINYINT(1) UNSIGNED")]
        public byte Durability { get; set; }

        [Column(TypeName = "TINYINT(1) UNSIGNED")]
        public byte Option { get; set; }

        [Column(TypeName = "TINYINT(1) UNSIGNED")]
        public byte OptionExe { get; set; }

        [Column(TypeName = "TINYINT(1) UNSIGNED")]
        public byte HarmonyOption { get; set; }

        public string SocketOptions { get; set; }
    }
}
