using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;
using MovieTicketsNotifierV3FuncApp.Services;

namespace MovieTicketsNotifierV3FuncApp.Utils
{
    public static class AlertProcessingUtil
    {
        /// <summary>
        /// Creates a combined list of alerts by converting alerts by name to alerts by ID and merging with existing alerts by ID
        /// </summary>
        /// <param name="alertsByName">List of alerts by movie name</param>
        /// <param name="alertsById">List of alerts by movie ID</param>
        /// <param name="accessToken">Access token for Scope API</param>
        /// <param name="configuration">Configuration for API endpoints</param>
        /// <param name="supabaseService">Supabase service for database operations</param>
        /// <param name="logger">Logger for error logging</param>
        /// <returns>Combined list of alerts with movie IDs</returns>
        public static async Task<List<RegisteredAlertById>> CombineAlerts(
            List<RegisteredAlertByName> alertsByName, 
            List<RegisteredAlertById> alertsById, 
            string accessToken, 
            IConfiguration configuration, 
            SupabaseService supabaseService,
            ILogger logger = null)
        {
            // Create a list to store all alerts with IDs
            List<RegisteredAlertById> allAlertsList = new List<RegisteredAlertById>();
            
            // Process all alerts by name
            foreach (var alert in alertsByName)
            {
                try
                {
                    // Convert movie name to array
                    string[] movieNames = new string[] { alert.MovieName };
                    
                    // Find movie IDs for the movie name
                    var movieIds = await ScopeUtil.FindMovieIDsByName(movieNames, accessToken, configuration, supabaseService);
                    
                    if (movieIds != null && movieIds.Any())
                    {
                        foreach (var movieId in movieIds)
                        {
                            // Create a new alert by ID for each movie ID found
                            var alertById = new RegisteredAlertById
                            {
                                // Copy base properties
                                Id = alert.Id,
                                CreatedAt = alert.CreatedAt,
                                Active = alert.Active,
                                Email = alert.Email,
                                Location = alert.Location,
                                Experiance = alert.Experiance,
                                Date = alert.Date,
                                
                                // Set the movie ID
                                MovieId = movieId
                            };
                            
                            allAlertsList.Add(alertById);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError($"Error converting alert by name to alert by ID: {ex.Message}");
                }
            }
            
            // Add all existing alerts by ID to the combined list
            allAlertsList.AddRange(alertsById);
            
            return allAlertsList;
        }
        
        /// <summary>
        /// Creates a unique key for a screening based on movie ID, location, date, and experience type
        /// </summary>
        /// <param name="movieId">Movie ID</param>
        /// <param name="location">Location code</param>
        /// <param name="date">Date string in yyyy-MM-dd format</param>
        /// <param name="experience">Experience type</param>
        /// <returns>Unique screening key</returns>
        public static string CreateScreeningKey(string movieId, string location, string date, string experience)
        {
            return $"{movieId}_{location}_{date}_{experience}";
        }
    }
}