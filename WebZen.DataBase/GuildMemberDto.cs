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
        public CharacterDto Memb { get; set; }

        public int GuildId { get; set; }
        public GuildDto Guild { get; set; }

        public int Rank { get; set; }
    }
}
