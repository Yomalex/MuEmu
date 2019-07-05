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

        public byte GameOption { get; set; }    // E

        public byte QkeyDefine { get; set; }    // F

        public byte WkeyDefine { get; set; }    // 10

        public byte EkeyDefine { get; set; }    // 11

        public byte ChatWindow { get; set; }    // 13

        public byte RkeyDefine { get; set; }

        public Int64 QWERLevelDefine { get; set; }
    }
}
