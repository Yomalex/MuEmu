using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("GuildMember")]
    public class GuildMemberDto
    {
        [Key]
        public int MembId { get; set; }

        public int GuildId { get; set; }

        public byte Rank { get; set; }
    }
}
