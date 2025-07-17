using System.Net;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui.Services;

public class HttpRatingService : IRatingService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpRatingService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<decimal> GetAverageRatingAsync(int? interventionId = null, int? protocolId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (interventionId.HasValue)
                queryParams.Add($"interventionId={interventionId.Value}");
            if (protocolId.HasValue)
                queryParams.Add($"protocolId={protocolId.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"ratings/average{queryString}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"ratings/average{queryString}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            
            if (decimal.TryParse(json, out var rating))
            {
                return rating;
            }
            
            // Try to deserialize as a response object
            var ratingResponse = JsonSerializer.Deserialize<decimal>(json, _jsonOptions);
            return ratingResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve average rating: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving average rating", ex);
        }
    }

    public async Task<RatingDto?> GetUserRatingAsync(string userId, int? interventionId = null, int? protocolId = null)
    {
        try
        {
            var queryParams = new List<string> { $"userId={userId}" };
            if (interventionId.HasValue)
                queryParams.Add($"interventionId={interventionId.Value}");
            if (protocolId.HasValue)
                queryParams.Add($"protocolId={protocolId.Value}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"ratings/user{queryString}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"ratings/user{queryString}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RatingDto>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve user rating: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving user rating", ex);
        }
    }

    public async Task<RatingDto> CreateOrUpdateRatingAsync(CreateRatingDto rating)
    {
        try
        {
            // Set the user ID from the current authenticated user
            if (string.IsNullOrEmpty(rating.UserId))
            {
                rating.UserId = _authService.CurrentUserId;
            }

            var json = JsonSerializer.Serialize(rating, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("ratings", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PostAsync("ratings", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RatingDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to create or update rating: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while creating or updating rating", ex);
        }
    }

    public async Task DeleteRatingAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"ratings/{id}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.DeleteAsync($"ratings/{id}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to delete rating {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while deleting rating", ex);
        }
    }

    private async Task<bool> HandleUnauthorizedAsync()
    {
        if (_authService is HttpAuthenticationService httpAuthService)
        {
            return await httpAuthService.HandleUnauthorizedAsync();
        }
        return false;
    }
}