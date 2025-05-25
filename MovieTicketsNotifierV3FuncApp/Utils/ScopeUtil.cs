using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;
using MovieTicketsNotifierV3FuncApp.Services;

namespace MovieTicketsNotifierV3FuncApp.Utils
{
    public static class ScopeUtil
    {
        public static async Task<List<string>> FindMovieIDsByName(string[] MovieNames, string AccessToken, IConfiguration config)
        {
            List<string> MovieIDs = new List<string>();
            foreach (var movie in MovieNames)
            {
                List<MovieIdNames> MovieIds = await GetAllMovieIds(AccessToken, config);

                foreach (var movieId in MovieIds)
                {
                    if (movieId.MovieName.ToLower().Contains(movie.ToLower()))
                    {
                        MovieIDs.Add(movieId.MovieId);
                    }
                }
            }
            return MovieIDs;
        }

        public static async Task<List<MovieIdNames>> GetAllMovieIds(string AccessToken, IConfiguration config)
        {
            List<MovieIdNames> MovieIds = new List<MovieIdNames>();

            try
            {
                //get all now showing movies
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(config);

                string GetNowShowingMoviesURL = scopeEndpointInfo.GetNowShowingMovies();
                string GetUpComingMoviesURL = scopeEndpointInfo.GetUpComingMovies();

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                    var ScopeNowShowingMoviesResponseResponse = await client.GetStringAsync(GetNowShowingMoviesURL);
                    var ScopeNowShowingMoviesResponseResponseObj = JsonSerializer.Deserialize<ScopeMoviesResponse>(ScopeNowShowingMoviesResponseResponse);
                    if (ScopeNowShowingMoviesResponseResponseObj?.MovieList?.Count > 0)
                    {
                        foreach (var movie in ScopeNowShowingMoviesResponseResponseObj.MovieList)
                        {
                            MovieIds.Add(new MovieIdNames { MovieId = movie.Id, MovieName = movie.Name });
                        }
                    }
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                    var ScopeUpComingMoviesResponse = await client.GetStringAsync(GetUpComingMoviesURL);
                    var ScopeUpComingMoviesResponseObj = JsonSerializer.Deserialize<ScopeMoviesResponse>(ScopeUpComingMoviesResponse);
                    if (ScopeUpComingMoviesResponseObj?.MovieList?.Count > 0)
                    {
                        foreach (var movie in ScopeUpComingMoviesResponseObj.MovieList)
                        {
                            MovieIds.Add(new MovieIdNames { MovieId = movie.Id, MovieName = movie.Name });
                        }
                    }
                }
                //get all upcoming movies
            }
            catch (Exception)
            {
            }

            return MovieIds;
        }

        public static async Task<List<MovieShowTimeMatchFoundResponse>> FindFirstScreeningDetails(string[] MovieIds, string[] MovieDates, string[] ExpectedExperiances, string AccessToken, IConfiguration config)
        {
            List<MovieShowTimeMatchFoundResponse> ListOfResponse = new List<MovieShowTimeMatchFoundResponse>();

            foreach (var MovieId in MovieIds)
            {
                foreach (var MovieDate in MovieDates)
                {
                    foreach (var ExpectedExperiance in ExpectedExperiances)
                    {
                        List<MovieShowTimeMatchFoundResponse> temp = null;
                        temp = await FindFirstScreeningDetails(MovieId, MovieDate, ExpectedExperiance, AccessToken, config);
                        ListOfResponse.AddRange(temp);
                    }
                }
            }

            return ListOfResponse;
        }

        public static async Task<List<MovieShowTimeMatchFoundResponse>> FindFirstScreeningDetails(string MovieId, string MovieDate, string ExpectedExperiance, string AccessToken, IConfiguration config)
        {
            List<MovieShowTimeMatchFoundResponse> ListOfResponse = new List<MovieShowTimeMatchFoundResponse>();
            try
            {
                ScopeEndpointInfo scopeEndpointInfo = new ScopeEndpointInfo(config);

                string URL = scopeEndpointInfo.GetMovieScreeningsByDate(MovieId, MovieDate);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                    var response = await client.GetStringAsync(URL);
                    var movieShowtimesResponse = JsonSerializer.Deserialize<ScopeMovieShowtimesResponse>(response);
                    if (movieShowtimesResponse?.MovieShowtimes?.Theaters?.Count > 0)
                    {
                        var Theaters = movieShowtimesResponse?.MovieShowtimes?.Theaters ?? new List<Theater> { };
                        foreach (var Theater in Theaters)
                        {
                            var experiances = Theater.Experiences ?? new List<Experience> { };
                            foreach (var experiance in experiances)
                            {
                                if (experiance.ExperienceName.ToLower().Contains(ExpectedExperiance.ToLower()))
                                {
                                    ListOfResponse.Add(new MovieShowTimeMatchFoundResponse
                                    {
                                        Experience = experiance,
                                        Theater = Theater
                                    });
                                }
                            }
                        }
                    }
                }

                return ListOfResponse;
            }
            catch (Exception)
            {
                return ListOfResponse;
            }
        }
    }
}
