using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MovieTicketsNotifierV3FuncApp.Services;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;

namespace MovieTicketsNotifierV3FuncApp
{
    public class MoviesFunction
    {
        private readonly ILogger _logger;
        private readonly SupabaseService _supabaseService;

        public MoviesFunction(ILoggerFactory loggerFactory, SupabaseService supabaseService)
        {
            _logger = loggerFactory.CreateLogger<MoviesFunction>();
            _supabaseService = supabaseService;
        }

        [Function("GetMovies")]
        public async Task<HttpResponseData> GetMovies(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "movies")] HttpRequestData req)
        {
            _logger.LogInformation("GetMovies function processed a request.");

            try
            {
                // Parse query parameters
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                var status = query["status"]; // 'now_showing', 'upcoming', or null for all
                var search = query["search"]; // search term for title or genre

                var movies = await _supabaseService.GetMovies(status, search);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<List<Movie>>
                {
                    Success = true,
                    Data = movies,
                    Message = "Movies retrieved successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movies");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve movies",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("GetMovieById")]
        public async Task<HttpResponseData> GetMovieById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "movies/{id:int}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"GetMovieById function processed a request for movie ID: {id}");

            try
            {
                var movie = await _supabaseService.GetMovieById(id);
                
                if (movie == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    notFoundResponse.Headers.Add("Content-Type", "application/json");
                    notFoundResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var notFoundResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Movie not found"
                    };

                    await notFoundResponse.WriteStringAsync(JsonSerializer.Serialize(notFoundResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<Movie>
                {
                    Success = true,
                    Data = movie,
                    Message = "Movie retrieved successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving movie with ID: {id}");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve movie",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("OptionsMovies")]
        public HttpResponseData OptionsMovies(
            [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "movies")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return response;
        }
    }
}
