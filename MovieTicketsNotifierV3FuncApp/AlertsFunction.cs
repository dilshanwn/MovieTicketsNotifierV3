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
    public class AlertsFunction
    {
        private readonly ILogger _logger;
        private readonly SupabaseService _supabaseService;

        public AlertsFunction(ILoggerFactory loggerFactory, SupabaseService supabaseService)
        {
            _logger = loggerFactory.CreateLogger<AlertsFunction>();
            _supabaseService = supabaseService;
        }

        [Function("GetAlerts")]
        public async Task<HttpResponseData> GetAlerts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alerts")] HttpRequestData req)
        {
            _logger.LogInformation("GetAlerts function processed a request.");

            try
            {
                // Get email from query parameter or authorization header
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                var email = query["email"];
                
                if (string.IsNullOrEmpty(email))
                {
                    // Try to get from Authorization header (in real app, this would be JWT token)
                    if (req.Headers.TryGetValues("Authorization", out var authHeaders))
                    {
                        var authHeader = authHeaders.FirstOrDefault();
                        // For now, we'll expect email in a simple Bearer format: "Bearer email@example.com"
                        if (authHeader?.StartsWith("Bearer ") == true)
                        {
                            email = authHeader.Substring(7);
                        }
                    }
                }

                if (string.IsNullOrEmpty(email))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email is required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }                var alertsById = await _supabaseService.GetAlertsByEmail(email);
                var alertsByName = await _supabaseService.GetAlertsByNameByEmail(email);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var combinedAlerts = new
                {
                    AlertsById = alertsById,
                    AlertsByName = alertsByName
                };

                var result = new ApiResponse<object>
                {
                    Success = true,
                    Data = combinedAlerts,
                    Message = "Alerts retrieved successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve alerts",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }        [Function("CreateAlert")]
        public async Task<HttpResponseData> CreateAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alerts")] HttpRequestData req)
        {
            _logger.LogInformation("CreateAlert function processed a request.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var createAlertRequest = JsonSerializer.Deserialize<CreateAlertRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (createAlertRequest == null || string.IsNullOrEmpty(createAlertRequest.Email))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email is required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                // Create alert by movie ID if provided, otherwise by movie name
                object alert;
                if (!string.IsNullOrEmpty(createAlertRequest.MovieId))
                {
                    alert = await _supabaseService.CreateAlertById(
                        createAlertRequest.Email, 
                        createAlertRequest.MovieId,
                        createAlertRequest.Location ?? "HCM",
                        createAlertRequest.Experiences ?? new string[] { "Digital" },
                        createAlertRequest.Date ?? DateTime.Today.AddDays(1)
                    );
                }
                else if (!string.IsNullOrEmpty(createAlertRequest.MovieName))
                {
                    alert = await _supabaseService.CreateAlertByName(
                        createAlertRequest.Email, 
                        createAlertRequest.MovieName,
                        createAlertRequest.Location ?? "HCM",
                        createAlertRequest.Experiences ?? new string[] { "Digital" },
                        createAlertRequest.Date ?? DateTime.Today.AddDays(1)
                    );
                }
                else
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Either MovieId or MovieName is required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<object>
                {
                    Success = true,
                    Data = alert,
                    Message = "Alert created successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to create alert",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("UpdateAlert")]
        public async Task<HttpResponseData> UpdateAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "alerts/{id:int}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"UpdateAlert function processed a request for alert ID: {id}");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updateAlertRequest = JsonSerializer.Deserialize<UpdateAlertRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (updateAlertRequest == null || string.IsNullOrEmpty(updateAlertRequest.Email))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email is required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                var success = await _supabaseService.UpdateAlert(id, updateAlertRequest.Email, updateAlertRequest.IsActive);

                if (!success)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    notFoundResponse.Headers.Add("Content-Type", "application/json");
                    notFoundResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var notFoundResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Alert not found or you don't have permission to update it"
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
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<object>
                {
                    Success = true,
                    Data = null,
                    Message = "Alert updated successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating alert with ID: {id}");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to update alert",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("DeleteAlert")]
        public async Task<HttpResponseData> DeleteAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "alerts/{id:int}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"DeleteAlert function processed a request for alert ID: {id}");

            try
            {
                // Get email from query parameter or authorization header
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                var email = query["email"];
                
                if (string.IsNullOrEmpty(email))
                {
                    if (req.Headers.TryGetValues("Authorization", out var authHeaders))
                    {
                        var authHeader = authHeaders.FirstOrDefault();
                        if (authHeader?.StartsWith("Bearer ") == true)
                        {
                            email = authHeader.Substring(7);
                        }
                    }
                }

                if (string.IsNullOrEmpty(email))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email is required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                var success = await _supabaseService.DeleteAlert(id, email);

                if (!success)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    notFoundResponse.Headers.Add("Content-Type", "application/json");
                    notFoundResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var notFoundResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Alert not found or you don't have permission to delete it"
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
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<object>
                {
                    Success = true,
                    Data = null,
                    Message = "Alert deleted successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting alert with ID: {id}");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to delete alert",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("OptionsAlerts")]
        public HttpResponseData OptionsAlerts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "alerts")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return response;
        }

        [Function("CreateAlertByName")]
        public async Task<HttpResponseData> CreateAlertByName(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alerts/by-name")] HttpRequestData req)
        {
            _logger.LogInformation("CreateAlertByName function processed a request.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var createAlertRequest = JsonSerializer.Deserialize<CreateAlertByNameRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (createAlertRequest == null || string.IsNullOrEmpty(createAlertRequest.Email) || string.IsNullOrEmpty(createAlertRequest.MovieName))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email and MovieName are required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                var alert = await _supabaseService.CreateAlertByName(
                    createAlertRequest.Email, 
                    createAlertRequest.MovieName,
                    createAlertRequest.Location ?? "HCM",
                    createAlertRequest.Experiences ?? new string[] { "Digital" },
                    createAlertRequest.Date ?? DateTime.Today.AddDays(1)
                );

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<RegisteredAlertByName>
                {
                    Success = true,
                    Data = alert,
                    Message = "Alert created successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert by name");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to create alert",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("CreateAlertById")]
        public async Task<HttpResponseData> CreateAlertById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alerts/by-id")] HttpRequestData req)
        {
            _logger.LogInformation("CreateAlertById function processed a request.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var createAlertRequest = JsonSerializer.Deserialize<CreateAlertByIdRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (createAlertRequest == null || string.IsNullOrEmpty(createAlertRequest.Email) || string.IsNullOrEmpty(createAlertRequest.MovieId))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email and MovieId are required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                var alert = await _supabaseService.CreateAlertById(
                    createAlertRequest.Email, 
                    createAlertRequest.MovieId,
                    createAlertRequest.Location ?? "HCM",
                    createAlertRequest.Experiences ?? new string[] { "Digital" },
                    createAlertRequest.Date ?? DateTime.Today.AddDays(1)
                );

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<RegisteredAlertById>
                {
                    Success = true,
                    Data = alert,
                    Message = "Alert created successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert by ID");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to create alert",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("GetAlertsByName")]
        public async Task<HttpResponseData> GetAlertsByName(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alerts/by-name/{email}")] HttpRequestData req,
            string email)
        {
            _logger.LogInformation($"GetAlertsByName function processed a request for email: {email}");

            try
            {
                var alerts = await _supabaseService.GetAlertsByNameByEmail(email);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<List<RegisteredAlertByName>>
                {
                    Success = true,
                    Data = alerts,
                    Message = "Alerts by name retrieved successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts by name");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve alerts by name",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("GetAlertsById")]
        public async Task<HttpResponseData> GetAlertsById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alerts/by-id/{email}")] HttpRequestData req,
            string email)
        {
            _logger.LogInformation($"GetAlertsById function processed a request for email: {email}");

            try
            {
                var alerts = await _supabaseService.GetAlertsByEmail(email);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<List<RegisteredAlertById>>
                {
                    Success = true,
                    Data = alerts,
                    Message = "Alerts by ID retrieved successfully"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts by ID");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve alerts by ID",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }

        [Function("DeleteMultipleAlerts")]
        public async Task<HttpResponseData> DeleteMultipleAlerts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "alerts/bulk")] HttpRequestData req)
        {
            _logger.LogInformation("DeleteMultipleAlerts function processed a request.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var deleteRequest = JsonSerializer.Deserialize<DeleteMultipleAlertsRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (deleteRequest == null || string.IsNullOrEmpty(deleteRequest.Email) || deleteRequest.Ids == null || !deleteRequest.Ids.Any())
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                    var badRequestResult = new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Message = "Email and alert IDs are required"
                    };

                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(badRequestResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));

                    return badRequestResponse;
                }

                var success = await _supabaseService.DeleteMultipleAlerts(deleteRequest.Ids, deleteRequest.Email);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                var result = new ApiResponse<object>
                {
                    Success = success,
                    Data = null,
                    Message = success ? "Alerts deleted successfully" : "Failed to delete some alerts"
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple alerts");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");

                var result = new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to delete alerts",
                    Error = ex.Message
                };

                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));

                return errorResponse;
            }
        }
    }    public class CreateAlertRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? MovieId { get; set; }
        public string? MovieName { get; set; }
        public string? Location { get; set; }
        public string[]? Experiences { get; set; }
        public DateTime? Date { get; set; }
    }

    public class UpdateAlertRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateAlertByNameRequest
    {
        public string Email { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string[]? Experiences { get; set; }
        public DateTime? Date { get; set; }
    }

    public class CreateAlertByIdRequest
    {
        public string Email { get; set; } = string.Empty;
        public string MovieId { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string[]? Experiences { get; set; }
        public DateTime? Date { get; set; }
    }

    public class DeleteMultipleAlertsRequest
    {
        public string Email { get; set; } = string.Empty;
        public int[] Ids { get; set; } = Array.Empty<int>();
    }
}
