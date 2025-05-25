using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieTicketsNotifierV3FuncApp.Utils;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;
using MovieTicketsNotifierV3FuncApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTicketsNotifierV3FuncApp
{
    public class ScheduledScopeChecker
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly SupabaseService _supabaseService;
        private readonly Dictionary<string, List<MovieShowTimeMatchFoundResponse>> _screeningCache;

        public ScheduledScopeChecker(ILoggerFactory loggerFactory, IConfiguration configuration, SupabaseService supabaseService)
        {
            _logger = loggerFactory.CreateLogger<ScheduledScopeChecker>();
            _configuration = configuration;
            _supabaseService = supabaseService;
            _screeningCache = new Dictionary<string, List<MovieShowTimeMatchFoundResponse>>();
        }

        [Function("ScheduledScopeChecker")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
        {
            string AccessToken = String.Empty;
            try
            {
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(_configuration);
                AccessToken = await scopeEndpointInfo.GetAccessToken();
            }
            catch (Exception)
            {
                _logger.LogError("Failed to get access token");
                return;
            }

            // Get alerts by name from Supabase
            var alertsByName = await _supabaseService.GetActiveAlertsByName();
            
            if (alertsByName.Any())
            {
                foreach (var alert in alertsByName)
                {
                    try
                    {
                        // Convert movie names to array
                        string[] movieNames = new string[] { alert.MovieName };
                        
                        // Find movie IDs for the movie name
                        var movieIds = await ScopeUtil.FindMovieIDsByName(movieNames, AccessToken, _configuration, _supabaseService);
                        
                        if (movieIds != null && movieIds.Any())
                        {
                            // Convert date to string array for API
                            string[] movieDates = new string[] { alert.Date.ToString("yyyy-MM-dd") };
                            
                            // Process screenings for this alert
                            await ProcessScreeningsAndSendEmails(
                                movieIds.ToArray(), 
                                movieDates, 
                                alert.Experiance, 
                                AccessToken, 
                                alert.Email,
                                new string[] { alert.Location }
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing alert by name: {ex.Message}");
                    }
                }
            }

            // Get alerts by ID from Supabase
            var alertsById = await _supabaseService.GetActiveAlertsById();
            
            if (alertsById.Any())
            {
                foreach (var alert in alertsById)
                {
                    try
                    {
                        // Movie ID is already provided
                        string[] movieIds = new string[] { alert.MovieId };
                        
                        // Convert date to string array for API
                        string[] movieDates = new string[] { alert.Date.ToString("yyyy-MM-dd") };
                        
                        // Process screenings for this alert
                        await ProcessScreeningsAndSendEmails(
                            movieIds, 
                            movieDates, 
                            alert.Experiance, 
                            AccessToken, 
                            alert.Email,
                            new string[] { alert.Location }
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing alert by id: {ex.Message}");
                    }
                }
            }
        }

        private async Task ProcessScreeningsAndSendEmails(
            string[] movieIds, 
            string[] movieDates, 
            string[] experienceTypes, 
            string accessToken, 
            string email,
            string[] locations)
        {
            // Create a cache key based on the input parameters
            string cacheKey = string.Join("_", 
                string.Join(",", movieIds), 
                string.Join(",", movieDates), 
                string.Join(",", experienceTypes));
            
            List<MovieShowTimeMatchFoundResponse> screeningResponses;
            
            // Check if we have cached results
            if (_screeningCache.ContainsKey(cacheKey))
            {
                screeningResponses = _screeningCache[cacheKey];
            }
            else
            {
                // Get screening details from API
                screeningResponses = await ScopeUtil.FindFirstScreeningDetails(
                    movieIds, 
                    movieDates, 
                    experienceTypes, 
                    accessToken, 
                    _configuration);
                
                // Cache the results
                _screeningCache[cacheKey] = screeningResponses;
            }

            // Filter screenings by location if needed
            if (locations != null && locations.Length > 0)
            {
                screeningResponses = screeningResponses
                    .Where(r => locations.Any(l => r.Theater.VistaCode.Contains(l)))
                    .ToList();
            }

            // Send emails for matching screenings
            foreach (var response in screeningResponses)
            {
                await SmtpUtil.SendEmail(_configuration, response, email);
            }
        }
    }
}
