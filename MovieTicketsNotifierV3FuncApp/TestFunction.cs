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

            // Collect all movie IDs and names for checking
            List<string> allMovieIds = new List<string>();
            List<string> allMovieNames = new List<string>();
            List<string> allDates = new List<string>();
            List<string> allExperiences = new List<string>();
            
            // Process alerts by name
            foreach (var alert in alertsByName)
            {
                allMovieNames.Add(alert.MovieName);
                
                foreach (var date in alert.Date)
                {
                    allDates.Add(date.ToString("yyyy-MM-dd"));
                }
                
                allExperiences.AddRange(alert.Experiance);
            }
            
            // Process alerts by ID
            foreach (var alert in alertsById)
            {
                allMovieIds.Add(alert.MovieId);
                
                foreach (var date in alert.Date)
                {
                    allDates.Add(date.ToString("yyyy-MM-dd"));
                }
                
                allExperiences.AddRange(alert.Experiance);
            }

            // Remove duplicates
            allMovieIds = allMovieIds.Distinct().ToList();
            allMovieNames = allMovieNames.Distinct().ToList();
            allDates = allDates.Distinct().ToList();
            allExperiences = allExperiences.Distinct().ToList();

            try
            {
                // Find movie IDs from names
                var movieIdsFromNames = await ScopeUtil.FindMovieIDsByName(
                    allMovieNames.ToArray(), 
                    AccessToken, 
                    _configuration,
                    _supabaseService);
                
                // Combine with direct IDs
                allMovieIds.AddRange(movieIdsFromNames);
                allMovieIds = allMovieIds.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error finding movie IDs: {ex.Message}");
            }

            // If we have no movie IDs, return early
            if (!allMovieIds.Any())
            {
                return new OkObjectResult("No valid movie IDs found");
            }

            // Check for screenings
            var movieShowTimeMatchFoundResponseList = await ScopeUtil.FindFirstScreeningDetails(
                allMovieIds.ToArray(), 
                allDates.ToArray(), 
                allExperiences.ToArray(), 
                AccessToken, 
                _configuration);

            // For demonstration purposes, send an email to the default recipient
            foreach (var movieShowTimeMatchFoundResponse in movieShowTimeMatchFoundResponseList)
            {
                var sentEmail = await SmtpUtil.SendEmail(_configuration, movieShowTimeMatchFoundResponse);
                if (!sentEmail)
                {
                    return new BadRequestResult();
                }
            }

            // Get all movies name list from matches
            var movieNameList = movieShowTimeMatchFoundResponseList
                .Select(x => x.Theater.MovieName)
                .Distinct()
                .ToList();

            var data = new
            {
                ScheduledData = new
                {
                    MoviesScheduledByName = allMovieNames,
                    AllScheduledMovieIds = allMovieIds,
                    AllScheduledMovieDates = allDates,
                    AllScheduledExperiences = allExperiences
                },
                AlertsData = new
                {
                    AlertsByName = alertsByName,
                    AlertsById = alertsById
                },
                Matches = new
                {
                    FoundMovieNames = movieNameList,
                    movieShowTimeMatchFoundResponseList = movieShowTimeMatchFoundResponseList ?? new List<MovieShowTimeMatchFoundResponse>()
                }
            };
            
            return new OkObjectResult(JsonSerializer.Serialize(data));
        }
    }
}
