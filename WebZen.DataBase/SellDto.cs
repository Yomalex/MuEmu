using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Sell")]
    public class SellDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int CharacterId { get; set; }

        [MaxLength(15)]
        public byte[] Item { get; set; }
        public DateTime Date { get; set; }
        public int Price { get; set; }
        [Column(TypeName = "SMALLINT(5)")]
        public ushort Count { get; set; }
    }
}
