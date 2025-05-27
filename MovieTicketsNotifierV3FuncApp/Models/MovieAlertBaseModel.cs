using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class MovieAlertBaseModel : BaseModel
    {
        [JsonPropertyName("id")]
        [JsonIgnore]
        [Column("id")]
        [PrimaryKey]
        public long Id { get; set; }

        [JsonPropertyName("created_at")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("active")]
        [Column("active")]
        public bool Active { get; set; } = true;

        [JsonPropertyName("email")]
        [Column("email")]
        public string Email { get; set; }

        [JsonPropertyName("location")]
        [Column("location")]
        public string Location { get; set; }

        [JsonPropertyName("experiance")]
        [Column("experiance")]
        public string[] Experiance { get; set; }

        [JsonPropertyName("date")]
        [Column("date")]
        public DateTime Date { get; set; }
    }
}