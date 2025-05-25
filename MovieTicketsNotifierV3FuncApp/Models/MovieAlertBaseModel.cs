using Supabase.Postgrest.Models;
using System;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class MovieAlertBaseModel : BaseModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; } = true;

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("experiance")]
        public string[] Experiance { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}