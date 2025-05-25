using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class Showtime
    {
        [JsonPropertyName("sid")]
        public int Id { get; set; }

        [JsonPropertyName("showtime_name")]
        public string ShowtimeName { get; set; }

        [JsonPropertyName("cinema_id")]
        public string CinemaId { get; set; }

        [JsonPropertyName("showtime_status")]
        public int ShowtimeStatus { get; set; }
    }
}