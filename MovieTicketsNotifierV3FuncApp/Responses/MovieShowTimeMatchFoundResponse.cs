using System.Text.Json.Serialization;
using MovieTicketsNotifierV3FuncApp.Models;

namespace MovieTicketsNotifierV3FuncApp.Responses
{
    public class MovieShowTimeMatchFoundResponse
    {
        [JsonPropertyName("theater")]
        public Theater? Theater { get; set; }

        [JsonPropertyName("experince")]
        public Experience? Experience { get; set; }
    }
}