using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieTicketsNotifierV3FuncApp.Models;

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
    }
}