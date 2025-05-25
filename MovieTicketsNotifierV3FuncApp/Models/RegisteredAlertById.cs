using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class RegisteredAlertById : MovieAlertBaseModel
    {
        [JsonPropertyName("movie_id")]
        public string MovieId { get; set; }
    }
}