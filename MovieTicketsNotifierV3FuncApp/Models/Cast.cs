using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class Cast
    {
        [JsonPropertyName("actor")]
        public string Actor { get; set; }

        [JsonPropertyName("character")]
        public string Character { get; set; }
    }
}