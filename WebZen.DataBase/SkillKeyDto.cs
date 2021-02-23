using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("SkillKey")]
    public class SkillKeyDto
    {
        [Key]
        public int SkillKeyId { get; set; }

        public byte[] SkillKey { get; set; }

        [Column(TypeName = "TINYINT UNSIGNED")]
        public byte GameOption { get; set; }    // E

        [Column(TypeName = "TINYINT UNSIGNED")]
        public byte QkeyDefine { get; set; }    // F

        [Column(TypeName = "TINYINT UNSIGNED")]
        public byte WkeyDefine { get; set; }    // 10

        [Column(TypeName = "TINYINT UNSIGNED")]
        public byte EkeyDefine { get; set; }    // 11

        [Column(TypeName = "TINYINT UNSIGNED")]
        public byte ChatWindow { get; set; }    // 13

        [Column(TypeName = "TINYINT UNSIGNED")]
        public byte RkeyDefine { get; set; }

        public Int64 QWERLevelDefine { get; set; }
    }
}
