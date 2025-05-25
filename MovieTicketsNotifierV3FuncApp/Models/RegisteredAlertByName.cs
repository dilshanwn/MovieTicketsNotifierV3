using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    [Table("registered_alerts_by_name")]
    public class RegisteredAlertByName : MovieAlertBaseModel
    {
        [JsonPropertyName("movie_name")]
        [Column("movie_name")]
        public string MovieName { get; set; }
    }
}