using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;


namespace MovieTicketsNotifierV3FuncApp.Models
{
    [Table("registered_alerts_by_id")]
    public class RegisteredAlertById : MovieAlertBaseModel
    {
        [JsonPropertyName("movie_id")]
        [Column("movie_id")]
        public string MovieId { get; set; }
    }
}