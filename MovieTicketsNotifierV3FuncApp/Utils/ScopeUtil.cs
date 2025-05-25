using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieTicketsNotifierV3FuncApp.Utils
{
    public static class ScopeUtil
    {

        public static async Task<List<string>> FindMovieIDsByName(string[] MovieNames, string AccessToken, IConfiguration config)
        {
            List<string> MovieIDs = new List<string>();
            foreach (var movie in MovieNames)
            {
                List<MovieIdNames> MovieIds = await GetAllMovieIds(AccessToken, config);

                foreach (var movieId in MovieIds)
                {
                    if (movieId.MovieName.ToLower().Contains(movie.ToLower()))
                    {
                        MovieIDs.Add(movieId.MovieId);
                    }
                }

            }
            return MovieIDs;
        }

        public static async Task<List<MovieIdNames>> GetAllMovieIds(string AccessToken, IConfiguration config)
        {
            List<MovieIdNames> MovieIds = new List<MovieIdNames>();

            try
            {
                //get all now showing movies
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(config);

                string GetNowShowingMoviesURL = scopeEndpointInfo.GetNowShowingMovies();
                string GetUpComingMoviesURL = scopeEndpointInfo.GetUpComingMovies();


                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                    var ScopeNowShowingMoviesResponseResponse = await client.GetStringAsync(GetNowShowingMoviesURL);
                    var ScopeNowShowingMoviesResponseResponseObj = JsonSerializer.Deserialize<ScopeMoviesResponse>(ScopeNowShowingMoviesResponseResponse);
                    if (ScopeNowShowingMoviesResponseResponseObj?.MovieList?.Count > 0)
                    {
                        foreach (var movie in ScopeNowShowingMoviesResponseResponseObj.MovieList)
                        {
                            MovieIds.Add(new MovieIdNames { MovieId = movie.Id, MovieName = movie.Name });
                        }
                    }

                }

                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                    var ScopeUpComingMoviesResponse = await client.GetStringAsync(GetUpComingMoviesURL);
                    var ScopeUpComingMoviesResponseObj = JsonSerializer.Deserialize<ScopeMoviesResponse>(ScopeUpComingMoviesResponse);
                    if (ScopeUpComingMoviesResponseObj?.MovieList?.Count > 0)
                    {
                        foreach (var movie in ScopeUpComingMoviesResponseObj.MovieList)
                        {
                            MovieIds.Add(new MovieIdNames { MovieId = movie.Id, MovieName = movie.Name });
                        }
                    }

                }



                //get all upcoming movies
            }
            catch (Exception)
            {
            }


            return MovieIds;
        }

        public static async Task<List<MovieShowTimeMatchFoundResponse>> FindFirstScreeningDetails(string[] MovieIds, string[] MovieDates, string[] ExpectedExperiances, string AccessToken, IConfiguration config)
        {
            List<MovieShowTimeMatchFoundResponse> ListOfResponse = new List<MovieShowTimeMatchFoundResponse>();

            foreach (var MovieId in MovieIds)
            {
                foreach (var MovieDate in MovieDates)
                {
                    foreach (var ExpectedExperiance in ExpectedExperiances)
                    {
                        List<MovieShowTimeMatchFoundResponse> temp = null;
                        temp = await FindFirstScreeningDetails(MovieId, MovieDate, ExpectedExperiance, AccessToken, config);
                        ListOfResponse.AddRange(temp);
                    }
                }
            }


            return ListOfResponse;
        }


        public static async Task<List<MovieShowTimeMatchFoundResponse>> FindFirstScreeningDetails(string MovieId, string MovieDate, string ExpectedExperiance, string AccessToken, IConfiguration config)
        {
            List<MovieShowTimeMatchFoundResponse> ListOfResponse = new List<MovieShowTimeMatchFoundResponse>();
            try
            {
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(config);

                string URL = scopeEndpointInfo.GetMovieScreeningsByDate(MovieId, MovieDate);

                using (HttpClient client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                    var response = await client.GetStringAsync(URL);
                    var movieShowtimesResponse = JsonSerializer.Deserialize<ScopeMovieShowtimesResponse>(response);
                    if (movieShowtimesResponse?.MovieShowtimes?.Theaters?.Count > 0)
                    {
                        var Theaters = movieShowtimesResponse?.MovieShowtimes?.Theaters ?? new List<Theater> { };
                        foreach (var Theater in Theaters)
                        {
                            var experiances = Theater.Experiences ?? new List<Experience> { };
                            foreach (var experiance in experiances)
                            {
                                if (experiance.ExperienceName.ToLower().Contains(ExpectedExperiance.ToLower()))
                                {
                                    ListOfResponse.Add(new MovieShowTimeMatchFoundResponse
                                    {
                                        Experience = experiance,
                                        Theater = Theater
                                    });
                                }
                            }

                        }

                    }

                }

                return ListOfResponse;
            }
            catch (Exception)
            {

                return ListOfResponse;
            }
        }
    }

    public class ScopeEndpointInfo
    {
        public string ScopeBaseUrl { get; set; } = String.Empty;
        public string ScopeMovieShowTimesUrl { get; set; } = String.Empty;
        public string ScopeUpComingMoviesUrl { get; set; } = String.Empty;
        public string ScopeNowShowingMoviesUrl { get; set; } = String.Empty;
        public string ScopeUserKey { get; set; } = String.Empty;
        public string ScopeTokenUrl { get; set; } = String.Empty;
        public string ScopeClientId { get; set; } = String.Empty;
        public string ScopeClientSecret { get; set; } = String.Empty;

        public ScopeEndpointInfo(IConfiguration config)
        {
            ScopeBaseUrl = config["ScopeBaseUrl"] ?? "";
            ScopeMovieShowTimesUrl = config["ScopeMovieShowTimesUrl"] ?? "";
            ScopeUpComingMoviesUrl = config["ScopeUpComingMoviesUrl"] ?? "";
            ScopeNowShowingMoviesUrl = config["ScopeNowShowingMoviesUrl"] ?? "";
            ScopeUserKey = config["ScopeUserKey"] ?? String.Empty;
            ScopeTokenUrl = config["ScopeTokenUrl"] ?? String.Empty;
            ScopeClientId = config["ScopeClientId"] ?? String.Empty;
            ScopeClientSecret = config["ScopeClientSecret"] ?? String.Empty;


        }

        public string GetMovieScreeningsByDate(string MovieId, string MovieDate)
        {
            return $"{ScopeBaseUrl.Trim()}/{ScopeMovieShowTimesUrl}?user_key={ScopeUserKey}&movie_id={MovieId}&movie_date={MovieDate}";
        }

        public string GetUpComingMovies()
        {
            return $"{ScopeBaseUrl.Trim()}/{ScopeUpComingMoviesUrl}?user_key={ScopeUserKey}";
        }

        public string GetNowShowingMovies()
        {
            return $"{ScopeBaseUrl.Trim()}/{ScopeNowShowingMoviesUrl}?user_key={ScopeUserKey}";
        }

        public async Task<string> GetAccessToken()
        {
            string ScopeAccessToken = "";

            try
            {
                using (var client = new HttpClient())
                {
                    // Prepare the request parameters
                    var requestParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", ScopeClientId),
                new KeyValuePair<string, string>("client_secret", ScopeClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

                    // Create the content of the request
                    var requestContent = new FormUrlEncodedContent(requestParams);

                    // Send POST request to the token URL
                    HttpResponseMessage response = await client.PostAsync(ScopeTokenUrl, requestContent);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the access token from the response JSON
                    var tokenResponse = System.Text.Json.JsonDocument.Parse(responseContent);
                    ScopeAccessToken = tokenResponse.RootElement.GetProperty("access_token").GetString();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                throw new ApplicationException("Failed to retrieve access token", ex);
            }

            return ScopeAccessToken;
        }



    }


    public class MovieShowTimeMatchFoundResponse
    {
        [JsonPropertyName("theater")]
        public Theater? Theater { get; set; }

        [JsonPropertyName("experince")]
        public Experience? Experience { get; set; }
    }

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

    public class VersionInfo
    {
        [JsonPropertyName("ios")]
        public int Ios { get; set; }

        [JsonPropertyName("andorid")]
        public int Android { get; set; }
    }

    public class MovieShowtimes
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("theaters")]
        public List<Theater> Theaters { get; set; }
    }

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

    public class Experience
    {
        [JsonPropertyName("experience_name")]
        public string ExperienceName { get; set; }

        [JsonPropertyName("experience_id")]
        public string ExperienceId { get; set; }

        [JsonPropertyName("showtimes")]
        public List<Showtime> Showtimes { get; set; }
    }

    public class Showtime
    {
        [JsonPropertyName("sid")]
        public int Id { get; set; }

        [JsonPropertyName("showtime_name")]
        public string ShowtimeName { get; set; }

        [JsonPropertyName("cinema_id")]
        public string CinemaId { get; set; }

        [JsonPropertyName("showtime_status")]
        public int ShowtimeStatus { get; set; }
    }

    public class MovieIdNames
    {
        [JsonPropertyName("movie_id")]
        public string MovieId { get; set; }
        [JsonPropertyName("movie_name")]
        public string MovieName { get; set; }
    }


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

    public class Cast
    {
        [JsonPropertyName("actor")]
        public string Actor { get; set; }

        [JsonPropertyName("character")]
        public string Character { get; set; }
    }


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
