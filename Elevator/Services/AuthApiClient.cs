using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Elevator.Shared.Models;

namespace Elevator.Services;

/// <summary>
/// HTTP client for authentication API communication with proper error handling and retry logic
/// </summary>
public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private const int MaxRetryAttempts = 3;
    private const int TimeoutSeconds = 30;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration request data</param>
    /// <returns>Authentication response with success status and token</returns>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, _jsonOptions);
            return await HandleAuthResponseAsync(response);
        });
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request data</param>
    /// <returns>Authentication response with success status and token</returns>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);
            return await HandleAuthResponseAsync(response);
        });
    }

    /// <summary>
    /// Logout current user session
    /// </summary>
    /// <returns>Success status of logout operation</returns>
    public async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.PostAsync("api/auth/logout", null);
            });

            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            // Even if logout fails on server, we should clear local state
            return true;
        }
    }

    /// <summary>
    /// Set the authorization header for authenticated requests
    /// </summary>
    /// <param name="token">JWT token</param>
    public void SetAuthorizationHeader(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Execute HTTP operation with retry logic for network issues
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (attempt < MaxRetryAttempts)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException && attempt < MaxRetryAttempts)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
        }

        // If all retries failed, throw the last exception
        throw lastException ?? new HttpRequestException("Request failed after maximum retry attempts");
    }

    /// <summary>
    /// Handle authentication API response and parse result
    /// </summary>
    private async Task<AuthResponse> HandleAuthResponseAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, _jsonOptions);
                return authResponse ?? new AuthResponse 
                { 
                    Success = false, 
                    Message = "Invalid response format" 
                };
            }
            else
            {
                // Try to parse error response
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<AuthResponse>(content, _jsonOptions);
                    return errorResponse ?? CreateErrorResponse(response.StatusCode, content);
                }
                catch
                {
                    return CreateErrorResponse(response.StatusCode, content);
                }
            }
        }
        catch (JsonException)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid response format from server" 
            };
        }
        catch (HttpRequestException)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Network error occurred. Please check your connection and try again." 
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Request timed out. Please try again." 
            };
        }
    }

    /// <summary>
    /// Create error response based on HTTP status code
    /// </summary>
    private static AuthResponse CreateErrorResponse(HttpStatusCode statusCode, string content)
    {
        var message = statusCode switch
        {
            HttpStatusCode.BadRequest => "Invalid request data",
            HttpStatusCode.Unauthorized => "Invalid email or password",
            HttpStatusCode.Conflict => "Email address is already registered",
            HttpStatusCode.InternalServerError => "Server error occurred. Please try again later.",
            HttpStatusCode.ServiceUnavailable => "Service is temporarily unavailable. Please try again later.",
            _ => $"Request failed with status: {statusCode}"
        };

        return new AuthResponse 
        { 
            Success = false, 
            Message = message 
        };
    }
}