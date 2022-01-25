using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VideoGameManager.DataAccess
{
    public class GameGenre
    {
        public int ID { get; set; }

        [MaxLength(150)]
        [Required]
        public string Name { get; set; } = string.Empty;

        // Without [JsonIgnore] The http result would be: System.Text.Json.JsonException: A possible object cycle was detected.        
        [JsonIgnore] // Not from Newtonsoft, but from System!
        public List<Game> Games { get; set; }
    }

    public class Game
    {
        public int ID { get; set; }

        [MaxLength(150)]
        [Required]
        public string Name { get; set; } = string.Empty;

        public GameGenre Genre { get; set; }

        public int PersonalRating { get; set; }


    }

}
