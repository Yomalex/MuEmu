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
        public int Serial { get; set; }

        public DateTime DateCreation { get; set; }

        public byte Map { get; set; }

        public int? Owner { get; set; }

        public ushort Item { get; set; }

        public bool Luck { get; set; }

        public bool Skill { get; set; }

        public int Option { get; set; }
    }
}
