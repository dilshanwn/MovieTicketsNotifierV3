using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieTicketsNotifierV3FuncApp.Utils;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;
using MovieTicketsNotifierV3FuncApp.Services;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace MovieTicketsNotifierV3FuncApp
{
    public class TestFunction
    {
        private readonly ILogger<TestFunction> _logger;
        private readonly IConfiguration _configuration;
        private readonly SupabaseService _supabaseService;
        private readonly Dictionary<string, List<MovieShowTimeMatchFoundResponse>> _screeningCache;

        public TestFunction(ILogger<TestFunction> logger, IConfiguration configuration, SupabaseService supabaseService)
        {
            _logger = logger;
            _configuration = configuration;
            _supabaseService = supabaseService;
            _screeningCache = new Dictionary<string, List<MovieShowTimeMatchFoundResponse>>();
        }

        [Function("TestFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string AccessToken = String.Empty;
            try
            {
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(_configuration);
                AccessToken = await scopeEndpointInfo.GetAccessToken();
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Failed to get access token");
            }

            // Get alerts from Supabase
            var alertsByName = await _supabaseService.GetActiveAlertsByName();
            var alertsById = await _supabaseService.GetActiveAlertsById();

            // Track all found screenings for the response
            List<MovieShowTimeMatchFoundResponse> allMovieShowTimeMatches = new List<MovieShowTimeMatchFoundResponse>();
            
            try
            {
                // Create a combined list of alerts with movie IDs
                var allAlertsList = await AlertProcessingUtil.CombineAlerts(
                    alertsByName, 
                    alertsById, 
                    AccessToken, 
                    _configuration, 
                    _supabaseService,
                    _logger);
                
                // Create a dictionary to track unique screenings that have been processed
                Dictionary<string, bool> processedScreenings = new Dictionary<string, bool>();
                
                // Process all alerts in the combined list
                foreach (var alert in allAlertsList)
                {
                    try
                    {
                        // Check each experience type for the alert
                        foreach (var experience in alert.Experiance)
                        {
                            // Create a unique key for this screening
                            string screeningKey = AlertProcessingUtil.CreateScreeningKey(
                                alert.MovieId, 
                                alert.Location, 
                                alert.Date.ToString("yyyy-MM-dd"), 
                                experience);
                            
                            // Only process this screening if we haven't seen it before
                            if (!processedScreenings.ContainsKey(screeningKey))
                            {
                                processedScreenings[screeningKey] = true;
                                
                                // Get or fetch screening details
                                var screeningMatches = await GetScreeningDetails(
                                    alert.MovieId,
                                    alert.Date.ToString("yyyy-MM-dd"),
                                    experience,
                                    AccessToken,
                                    alert.Location);
                                
                                // Add to all matches
                                allMovieShowTimeMatches.AddRange(screeningMatches);
                                
                                // For demonstration purposes, send an email
                                foreach (var match in screeningMatches)
                                {
                                    await SmtpUtil.SendEmail(_configuration, match, alert.Email);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing alert: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in alert processing: {ex.Message}");
            }

       
            return new OkObjectResult("ok");
        }
        
        private async Task<List<MovieShowTimeMatchFoundResponse>> GetScreeningDetails(
            string movieId,
            string movieDate,
            string experienceType,
            string accessToken,
            string location)
        {
            // Create a cache key based on the input parameters
            string cacheKey = AlertProcessingUtil.CreateScreeningKey(
                movieId, 
                location, 
                movieDate, 
                experienceType);
            
            List<MovieShowTimeMatchFoundResponse> screeningResponses;
            
            // Check if we have cached results
            if (_screeningCache.ContainsKey(cacheKey))
            {
                screeningResponses = _screeningCache[cacheKey];
            }
            else
            {
                // Get screening details from API for this specific combination
                screeningResponses = await ScopeUtil.FindFirstScreeningDetails(
                    movieId,
                    movieDate,
                    experienceType,
                    accessToken,
                    _configuration);
                
                // Cache the results
                _screeningCache[cacheKey] = screeningResponses;
            }

            // Filter screenings by location
            return screeningResponses
                .Where(r => r.Theater.VistaCode.Contains(location))
                .ToList();
        }
    }
}
