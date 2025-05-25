using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Responses
{
    public class ScopeMovieShowtimesResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("version")]
        public VersionInfo Version { get; set; }

        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("movieshowtimes")]
        public MovieShowtimes MovieShowtimes { get; set; }
    }
}