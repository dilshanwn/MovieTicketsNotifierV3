using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MovieTicketsNotifierV3FuncApp.Services
{
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
}