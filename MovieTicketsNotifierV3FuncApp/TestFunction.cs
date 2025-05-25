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

namespace MovieTicketsNotifierV3FuncApp
{
    public class TestFunction
    {
        private readonly ILogger<TestFunction> _logger;
        private readonly IConfiguration _configuration;

        public TestFunction(ILogger<TestFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("TestFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            var ExperianceType = _configuration["ExperianceTypes"]?.Split(',') ?? Array.Empty<string>();
            var MovieIds = _configuration["MovieIds"]?.Split(',') ?? Array.Empty<string>();
            var MovieNames = _configuration["MovieNames"]?.Split(',');
            var MovieDate = _configuration["MovieDates"]?.Split(',') ?? Array.Empty<string>();



            string AccessToken = String.Empty;
            try
            {
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(_configuration);
                AccessToken = await scopeEndpointInfo.GetAccessToken();
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                var movieidsbyName = await ScopeUtil.FindMovieIDsByName(MovieNames, AccessToken, _configuration);
                if (movieidsbyName != null)
                {
                    MovieIds = MovieIds.Concat(movieidsbyName).ToArray();
                    MovieIds = MovieIds.Distinct().ToArray();
                }
            }
            catch (Exception)
            {
            }

            var movieShowTimeMatchFoundResponseList = await ScopeUtil.FindFirstScreeningDetails(MovieIds, MovieDate, ExperianceType, AccessToken, _configuration);

            string jsonString = JsonSerializer.Serialize(movieShowTimeMatchFoundResponseList) ?? "";

            foreach (var movieShowTimeMatchFoundResponse in movieShowTimeMatchFoundResponseList)
            {
                var SentEmail = await SmtpUtil.SendEmail(_configuration, movieShowTimeMatchFoundResponse);
                if (!SentEmail)
                {
                    return new BadRequestResult();
                }
            }

            // get all movies name list from matches
            var movieNameList = movieShowTimeMatchFoundResponseList.Select(x => x.Theater.MovieName).Distinct().ToList();

            var data = new
            {
                ScheduledData = new
                {
                    MoviesScheduledByName = MovieNames,
                    AllScheduledMovieIds = MovieIds,
                    AllScheduledMovieDates = MovieDate,
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
