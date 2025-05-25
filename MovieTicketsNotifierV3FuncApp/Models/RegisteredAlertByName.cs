using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class RegisteredAlertByName : MovieAlertBaseModel
    {
        [JsonPropertyName("movie_name")]
        public string MovieName { get; set; }
    }
}