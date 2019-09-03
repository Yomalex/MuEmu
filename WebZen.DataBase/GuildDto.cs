using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("Guild")]
    public class GuildDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GuildId { get; set; }

        public string Name { get; set; }

        public int? AllianceId { get; set; }

        public byte[] Mark { get; set; }

        public byte GuildType { get; set; }

        public List<GuildMemberDto> MembersInfo { get; set; }

        //public List<CharacterDto> Characters { get; set; }
    }
}
