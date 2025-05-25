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

        public TestFunction(ILogger<TestFunction> logger, IConfiguration configuration, SupabaseService supabaseService)
        {
            _logger = logger;
            _configuration = configuration;
            _supabaseService = supabaseService;
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
            
            // Process alerts by name individually, similar to ScheduledScopeChecker
            foreach (var alert in alertsByName)
            {
                try
                {
                    // Convert movie name to array
                    string[] movieNames = new string[] { alert.MovieName };
                    
                    // Find movie IDs for the movie name
                    var movieIds = await ScopeUtil.FindMovieIDsByName(movieNames, AccessToken, _configuration, _supabaseService);
                    
                    if (movieIds != null && movieIds.Any())
                    {
                        // Convert date to string array for API
                        string[] movieDates = new string[] { alert.Date.ToString("yyyy-MM-dd") };
                        
                        // Check for screenings with this alert's specific criteria
                        var matches = await ScopeUtil.FindFirstScreeningDetails(
                            movieIds.ToArray(),
                            movieDates,
                            alert.Experiance,
                            AccessToken,
                            _configuration);
                        
                        if (matches != null && matches.Any())
                        {
                            // Filter by location if needed
                            var locationMatches = matches.Where(m => m.Theater.VistaCode.Contains(alert.Location)).ToList();
                            allMovieShowTimeMatches.AddRange(locationMatches);
                            
                            // For demonstration purposes, send an email
                            foreach (var match in locationMatches)
                            {
                                await SmtpUtil.SendEmail(_configuration, match, alert.Email);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing alert by name: {ex.Message}");
                }
            }
            
            // Process alerts by ID individually, similar to ScheduledScopeChecker
            foreach (var alert in alertsById)
            {
                try
                {
                    // Movie ID is already provided
                    string[] movieIds = new string[] { alert.MovieId };
                    
                    // Convert date to string array for API
                    string[] movieDates = new string[] { alert.Date.ToString("yyyy-MM-dd") };
                    
                    // Check for screenings with this alert's specific criteria
                    var matches = await ScopeUtil.FindFirstScreeningDetails(
                        movieIds,
                        movieDates,
                        alert.Experiance,
                        AccessToken,
                        _configuration);
                    
                    if (matches != null && matches.Any())
                    {
                        // Filter by location if needed
                        var locationMatches = matches.Where(m => m.Theater.VistaCode.Contains(alert.Location)).ToList();
                        allMovieShowTimeMatches.AddRange(locationMatches);
                        
                        // For demonstration purposes, send an email
                        foreach (var match in locationMatches)
                        {
                            await SmtpUtil.SendEmail(_configuration, match, alert.Email);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing alert by id: {ex.Message}");
                }
            }

            // Get all movies name list from matches
            var movieNameList = allMovieShowTimeMatches
                .Select(x => x.Theater.MovieName)
                .Distinct()
                .ToList();

            var data = new
            {
                AlertsData = new
                {
                    AlertsByName = alertsByName,
                    AlertsById = alertsById
                },
                Matches = new
                {
                    FoundMovieNames = movieNameList,
                    movieShowTimeMatchFoundResponseList = allMovieShowTimeMatches
                }
            };
            
            return new OkObjectResult(JsonSerializer.Serialize(data));
        }
    }
}
