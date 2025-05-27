using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Responses
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
