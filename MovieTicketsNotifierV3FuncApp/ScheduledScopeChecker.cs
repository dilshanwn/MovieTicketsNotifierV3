using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieTicketsNotifierV3FuncApp.Utils;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;
using MovieTicketsNotifierV3FuncApp.Services;
using System;

namespace MovieTicketsNotifierV3FuncApp
{
    public class ScheduledScopeChecker
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public ScheduledScopeChecker(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<ScheduledScopeChecker>();
            _configuration = configuration;
        }

        [Function("ScheduledScopeChecker")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
        {
            var ExperianceType = _configuration["ExperianceTypes"]?.Split(',');
            var MovieIds = _configuration["MovieIds"]?.Split(',');
            var MovieNames = _configuration["MovieNames"]?.Split(',');
            var MovieDate = _configuration["MovieDates"]?.Split(',');

            if (MovieIds == null && MovieNames == null)
            {
                return;
            }

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

            foreach (var movieShowTimeMatchFoundResponse in movieShowTimeMatchFoundResponseList)
            {
                var SentEmail = await SmtpUtil.SendEmail(_configuration, movieShowTimeMatchFoundResponse);
            }

        }
    }
}
