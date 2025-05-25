using System.Collections.Generic;
using System.Text.Json.Serialization;
using MovieTicketsNotifierV3FuncApp.Models;

namespace MovieTicketsNotifierV3FuncApp.Responses
{
    public class MovieShowtimes
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("theaters")]
        public List<Theater> Theaters { get; set; }
    }
}