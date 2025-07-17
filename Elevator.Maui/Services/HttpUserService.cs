using System.Net;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui.Services;

public class HttpUserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpUserService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<UserDto?> GetUserAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"users/{id}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"users/{id}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDto>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve user {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving user", ex);
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var encodedEmail = Uri.EscapeDataString(email);
            var response = await _httpClient.GetAsync($"users/by-email/{encodedEmail}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"users/by-email/{encodedEmail}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDto>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve user by email {email}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving user by email", ex);
        }
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto user)
    {
        try
        {
            var json = JsonSerializer.Serialize(user, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"users/{id}", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PutAsync($"users/{id}", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to update user {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while updating user", ex);
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