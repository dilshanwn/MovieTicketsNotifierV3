using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class Experience
    {
        [JsonPropertyName("experience_name")]
        public string ExperienceName { get; set; }

        [JsonPropertyName("experience_id")]
        public string ExperienceId { get; set; }

        [JsonPropertyName("showtimes")]
        public List<Showtime> Showtimes { get; set; }
    }
}