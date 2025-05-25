using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class MovieIdNames
    {
        [JsonPropertyName("movie_id")]
        public string MovieId { get; set; }
        
        [JsonPropertyName("movie_name")]
        public string MovieName { get; set; }
    }
}