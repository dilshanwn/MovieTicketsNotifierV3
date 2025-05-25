using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieTicketsNotifierV3FuncApp.Models
{
    public class Movie
    {
        [JsonPropertyName("mid")]
        public string Id { get; set; }

        [JsonPropertyName("vista_code")]
        public List<string> VistaCode { get; set; }

        [JsonPropertyName("m_name")]
        public string Name { get; set; }

        [JsonPropertyName("adult")]
        public int Adult { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("movie_dates")]
        public List<string> MovieDates { get; set; }

        [JsonPropertyName("you_tube_link")]
        public string YouTubeLink { get; set; }

        [JsonPropertyName("you_tube_id")]
        public string YouTubeId { get; set; }

        [JsonPropertyName("runtime")]
        public string Runtime { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("imdb")]
        public float Imdb { get; set; }

        [JsonPropertyName("is3d_movie")]
        public int Is3dMovie { get; set; }

        [JsonPropertyName("synopsis")]
        public string Synopsis { get; set; }

        [JsonPropertyName("m_img")]
        public string Image { get; set; }

        [JsonPropertyName("m_large_img")]
        public string LargeImage { get; set; }

        [JsonPropertyName("mobile_img")]
        public string MobileImage { get; set; }

        [JsonPropertyName("trailer")]
        public string Trailer { get; set; }

        [JsonPropertyName("genre")]
        public List<string> Genre { get; set; }

        [JsonPropertyName("fact")]
        public string Fact { get; set; }

        [JsonPropertyName("featured")]
        public int Featured { get; set; }

        [JsonPropertyName("directors")]
        public List<string> Directors { get; set; }

        [JsonPropertyName("producers")]
        public List<string> Producers { get; set; }

        [JsonPropertyName("musicians")]
        public List<string> Musicians { get; set; }

        [JsonPropertyName("writters")]
        public List<string> Writers { get; set; }

        [JsonPropertyName("cast")]
        public List<Cast> Cast { get; set; }

        [JsonPropertyName("gallery")]
        public List<string> Gallery { get; set; }
    }
}