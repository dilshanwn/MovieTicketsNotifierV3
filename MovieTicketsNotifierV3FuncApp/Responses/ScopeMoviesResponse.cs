using System.Collections.Generic;
using System.Text.Json.Serialization;
using MovieTicketsNotifierV3FuncApp.Models;

namespace MovieTicketsNotifierV3FuncApp.Responses
{
    public class ScopeMoviesResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("version")]
        public VersionInfo Version { get; set; }

        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("movielist")]
        public List<Movie> MovieList { get; set; }
    }
}