using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("GremoryCase")]
    public class GremoryCaseDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long GiftId { get; set; }

        public int AccountId { get; set; }
        public AccountDto Account { get; set; }

        public byte Inventory { get; set; }
        public byte Source { get; set; }

        public int? CharacterId { get; set; }
        public uint Auth { get; set; }
        //public uint ItemGUID { get; set; }

        // Item Information
        [Column(TypeName = "SMALLINT(5) UNSIGNED")]
        public ushort ItemNumber { get; set; }
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

        public DateTime ExpireTime { get; set; }
    }
}
