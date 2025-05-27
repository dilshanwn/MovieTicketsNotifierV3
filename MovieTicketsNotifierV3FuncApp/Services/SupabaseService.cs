using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MovieTicketsNotifierV3FuncApp.Models;
using System.Text.Json;
using MovieTicketsNotifierV3FuncApp.Responses;
using MovieTicketsNotifierV3FuncApp.Services;

namespace MovieTicketsNotifierV3FuncApp.Services
{
    public class SupabaseService
    {
        private readonly Client _client;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, List<MovieIdNames>> _movieIdsCache;

        public SupabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            var url = _configuration["Supabase:Url"];
            var key = _configuration["Supabase:Key"];
            
            _client = new Client(url, key);
            _movieIdsCache = new Dictionary<string, List<MovieIdNames>>();
        }

        public async Task<List<RegisteredAlertByName>> GetActiveAlertsByName()
        {
            try
            {
                var response = await _client.From<RegisteredAlertByName>()
                    .Where(x => x.Active == true)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return new List<RegisteredAlertByName>();
            }
        }

        public async Task<List<RegisteredAlertById>> GetActiveAlertsById()
        {
            try
            {
                var response = await _client.From<RegisteredAlertById>()
                    .Where(x => x.Active == true)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return new List<RegisteredAlertById>();
            }
        }

        public void CacheMovieIds(string key, List<MovieIdNames> movieIds)
        {
            if (!_movieIdsCache.ContainsKey(key))
            {
                _movieIdsCache.Add(key, movieIds);
            }
        }

        public List<MovieIdNames> GetCachedMovieIds(string key)
        {
            if (_movieIdsCache.ContainsKey(key))
            {
                return _movieIdsCache[key];
            }
            return null;
        }

        // Alert management methods for the web API

        public async Task<List<RegisteredAlertById>> GetAlertsByEmail(string email)
        {
            try
            {
                var response = await _client.From<RegisteredAlertById>()
                    .Where(x => x.Email == email)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return new List<RegisteredAlertById>();
            }
        }

        public async Task<RegisteredAlertById> CreateAlert(string email, int movieId)
        {
            try
            {
                var newAlert = new RegisteredAlertById
                {
                    Email = email,
                    MovieId = movieId.ToString(),
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    Location = "HCM", // Default location - should be configurable
                    Experiance = new string[] { "Digital" }, // Default experience - should be configurable
                    Date = DateTime.Today.AddDays(1) // Default to tomorrow - should be configurable
                };

                var response = await _client.From<RegisteredAlertById>()
                    .Insert(newAlert);

                return response.Models.FirstOrDefault() ?? newAlert;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                throw new Exception($"Failed to create alert: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAlert(int id, string email, bool isActive)
        {
            try
            {
                var updateData = new RegisteredAlertById
                {
                    Id = id,
                    Active = isActive
                };

                var response = await _client.From<RegisteredAlertById>()
                    .Where(x => x.Id == id && x.Email == email)
                    .Update(updateData);

                return response.Models.Any();
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return false;
            }
        }        public async Task<bool> DeleteAlert(int id, string email)
        {
            try
            {
                await _client.From<RegisteredAlertById>()
                    .Where(x => x.Id == id && x.Email == email)
                    .Delete();

                return true; // If no exception, deletion was successful
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return false;
            }
        }

        // Additional methods for alert management by name

        public async Task<List<RegisteredAlertByName>> GetAlertsByNameByEmail(string email)
        {
            try
            {
                var response = await _client.From<RegisteredAlertByName>()
                    .Where(x => x.Email == email)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return new List<RegisteredAlertByName>();
            }
        }

        public async Task<RegisteredAlertByName> CreateAlertByName(string email, string movieName, string location, string[] experiences, DateTime date)
        {
            try
            {
                var newAlert = new RegisteredAlertByName
                {
                    Email = email,
                    MovieName = movieName,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    Location = location,
                    Experiance = experiences,
                    Date = date
                };

                var response = await _client.From<RegisteredAlertByName>()
                    .Insert(newAlert);

                return response.Models.FirstOrDefault() ?? newAlert;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                throw new Exception($"Failed to create alert by name: {ex.Message}", ex);
            }
        }

        public async Task<RegisteredAlertById> CreateAlertById(string email, string movieId, string location, string[] experiences, DateTime date)
        {
            try
            {
                var newAlert = new RegisteredAlertById
                {
                    Email = email,
                    MovieId = movieId,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    Location = location,
                    Experiance = experiences,
                    Date = date
                };

                var response = await _client.From<RegisteredAlertById>()
                    .Insert(newAlert);

                return response.Models.FirstOrDefault() ?? newAlert;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                throw new Exception($"Failed to create alert by ID: {ex.Message}", ex);
            }
        }

        // Bulk operations for multiple alerts

        public async Task<bool> DeleteMultipleAlerts(int[] alertIds, string email)
        {
            try
            {
                foreach (var id in alertIds)
                {
                    await _client.From<RegisteredAlertById>()
                        .Where(x => x.Id == id && x.Email == email)
                        .Delete();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return false;
            }
        }

        public async Task<bool> ToggleAlertStatus(int id, string email)
        {
            try
            {
                // First get the current alert to know its status
                var currentAlert = await _client.From<RegisteredAlertById>()
                    .Where(x => x.Id == id && x.Email == email)
                    .Single();

                if (currentAlert != null)
                {
                    var updateData = new RegisteredAlertById
                    {
                        Id = id,
                        Active = !currentAlert.Active
                    };

                    var response = await _client.From<RegisteredAlertById>()
                        .Where(x => x.Id == id && x.Email == email)
                        .Update(updateData);

                    return response.Models.Any();
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return false;
            }
        }

        // Movie management methods

        /// <summary>
        /// Get movies from Scope API with optional filtering
        /// </summary>
        /// <param name="status">Movie status: 'now_showing', 'upcoming', or null for all</param>
        /// <param name="search">Search term for movie title or genre</param>
        /// <returns>List of movies</returns>
        public async Task<List<Movie>> GetMovies(string? status = null, string? search = null)
        {
            try
            {
                // Get access token for Scope API
                var scopeEndpointInfo = new ScopeEndpointInfo(_configuration);
                var accessToken = await scopeEndpointInfo.GetAccessToken();

                List<Movie> allMovies = new List<Movie>();

                // Determine which endpoints to call based on status filter
                bool getNowShowing = status == null || status.ToLower() == "now_showing";
                bool getUpcoming = status == null || status.ToLower() == "upcoming";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // Get now showing movies
                    if (getNowShowing)
                    {
                        try
                        {
                            string nowShowingUrl = scopeEndpointInfo.GetNowShowingMovies();
                            var nowShowingResponse = await client.GetStringAsync(nowShowingUrl);
                            var nowShowingMoviesObj = JsonSerializer.Deserialize<ScopeMoviesResponse>(nowShowingResponse);
                            
                            if (nowShowingMoviesObj?.MovieList?.Count > 0)
                            {
                                allMovies.AddRange(nowShowingMoviesObj.MovieList);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with upcoming movies
                        }
                    }

                    // Get upcoming movies
                    if (getUpcoming)
                    {
                        try
                        {
                            string upcomingUrl = scopeEndpointInfo.GetUpComingMovies();
                            var upcomingResponse = await client.GetStringAsync(upcomingUrl);
                            var upcomingMoviesObj = JsonSerializer.Deserialize<ScopeMoviesResponse>(upcomingResponse);
                            
                            if (upcomingMoviesObj?.MovieList?.Count > 0)
                            {
                                allMovies.AddRange(upcomingMoviesObj.MovieList);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue
                        }
                    }
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(search))
                {
                    var searchTerm = search.ToLower();
                    allMovies = allMovies.Where(m => 
                        m.Name.ToLower().Contains(searchTerm) ||
                        (m.Genre?.Any(g => g.ToLower().Contains(searchTerm)) == true)
                    ).ToList();
                }

                // Remove duplicates based on movie ID
                allMovies = allMovies
                    .GroupBy(m => m.Id)
                    .Select(g => g.First())
                    .ToList();

                return allMovies;
            }
            catch (Exception ex)
            {
                // Log error here if needed
                throw new Exception($"Failed to retrieve movies: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get a specific movie by ID from Scope API
        /// </summary>
        /// <param name="movieId">The movie ID to retrieve</param>
        /// <returns>Movie object or null if not found</returns>
        public async Task<Movie?> GetMovieById(int movieId)
        {
            try
            {
                // Get all movies and find the one with matching ID
                var allMovies = await GetMovies();
                return allMovies.FirstOrDefault(m => m.Id == movieId.ToString());
            }
            catch (Exception ex)
            {
                // Log error here if needed
                throw new Exception($"Failed to retrieve movie by ID: {ex.Message}", ex);
            }
        }
    }
}