using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MU.DataBase
{
    [Table("FavoritesList")]
    public class FavoritesDto
    {
        [Key]
        public int CharacterId { get; set; }
        public CharacterDto Character { get; set; }
        public int Fav01 { get; set; } = -1;
        public int Fav02 { get; set; } = -1;
        public int Fav03 { get; set; } = -1;
        public int Fav04 { get; set; } = -1;
        public int Fav05 { get; set; } = -1;
    }
}
