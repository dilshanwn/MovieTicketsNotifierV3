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

            // Get alerts by name and by ID from Supabase
            var alertsByName = await _supabaseService.GetActiveAlertsByName();
            var alertsById = await _supabaseService.GetActiveAlertsById();
            
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
                
                // Process all alerts in the combined list
                if (allAlertsList.Any())
                {
                    // Create a dictionary to track unique screenings that have been processed
                    Dictionary<string, bool> processedScreenings = new Dictionary<string, bool>();
                    
                    foreach (var alert in allAlertsList)
                    {
                        try
                        {
                            // Convert date to string array for API
                            string[] movieDates = new string[] { alert.Date.ToString("yyyy-MM-dd") };
                            string[] movieIds = new string[] { alert.MovieId };
                            
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
                                    
                                    // Process screenings for this specific combination
                                    await ProcessScreeningAndSendEmail(
                                        alert.MovieId,
                                        alert.Date.ToString("yyyy-MM-dd"),
                                        experience,
                                        AccessToken,
                                        alert.Email,
                                        alert.Location
                                    );
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing alert: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in alert processing: {ex.Message}");
            }
        }

        private async Task ProcessScreeningAndSendEmail(
            string movieId,
            string movieDate,
            string experienceType,
            string accessToken,
            string email,
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
            screeningResponses = screeningResponses
                .Where(r => r.Theater.VistaCode.Contains(location))
                .ToList();

            // Send emails for matching screenings
            foreach (var response in screeningResponses)
            {
                await SmtpUtil.SendEmail(_configuration, response, email);
            }
        }
    }
}
