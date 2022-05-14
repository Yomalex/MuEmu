using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("GuildMatching")]
    public class GuildMatchingDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GuildId { get; set; }
        public GuildDto Guild { get; set; }

        public string Title { get; set; }
        public byte InterstType { get; set; }
        public byte LevelRange { get; set; }
        public ushort Class { get; set; }
    }
    [Table("GuildMatchingJoin")]
    public class GuildMatchingJoinDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GuildMatchingId { get; set; }
        public GuildMatchingDto GuildMatching { get; set; }
        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }
        public byte State { get; set; }
    }
}
