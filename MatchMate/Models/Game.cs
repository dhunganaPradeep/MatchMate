using System;
using System.ComponentModel.DataAnnotations;

namespace MatchMate.Models
{
    public class Game
    {
        public int GameId { get; set; }
        [Required]
        [StringLength(100)]
        public string GameName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}