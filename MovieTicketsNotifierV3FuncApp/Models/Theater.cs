using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class Theater
    {
        [JsonPropertyName("t_name")]
        public string Name { get; set; }

        [JsonPropertyName("t_vista_code")]
        public string VistaCode { get; set; }

        [JsonPropertyName("t_city")]
        public string City { get; set; }

        [JsonPropertyName("mid")]
        public string MovieId { get; set; }

        [JsonPropertyName("tid")]
        public string TheaterId { get; set; }

        [JsonPropertyName("m_name")]
        public string MovieName { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("booking_end_date")]
        public string BookingEndDate { get; set; }

        [JsonPropertyName("t_img")]
        public string TheaterImage { get; set; }

        [JsonPropertyName("m_img")]
        public string MovieImage { get; set; }

        [JsonPropertyName("experinces")]
        public List<Experience> Experiences { get; set; }
    }
}