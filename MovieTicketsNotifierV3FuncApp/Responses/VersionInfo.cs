using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Responses
{
    public class VersionInfo
    {
        [JsonPropertyName("ios")]
        public int Ios { get; set; }

        [JsonPropertyName("andorid")]
        public int Android { get; set; }
    }
}