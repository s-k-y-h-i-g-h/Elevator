using System.Net;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui.Services;

public class HttpProtocolService : IProtocolService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpProtocolService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<ProtocolDto>> GetProtocolsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("protocols");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync("protocols");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var protocols = JsonSerializer.Deserialize<IEnumerable<ProtocolDto>>(json, _jsonOptions);
            return protocols ?? new List<ProtocolDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve protocols: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving protocols", ex);
        }
    }

    public async Task<IEnumerable<ProtocolDto>> GetUserProtocolsAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"protocols/user/{userId}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"protocols/user/{userId}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var protocols = JsonSerializer.Deserialize<IEnumerable<ProtocolDto>>(json, _jsonOptions);
            return protocols ?? new List<ProtocolDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve user protocols: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving user protocols", ex);
        }
    }

    public async Task<ProtocolDetailDto?> GetProtocolAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"protocols/{id}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"protocols/{id}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProtocolDetailDto>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve protocol {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving protocol", ex);
        }
    }

    public async Task<ProtocolDto> CreateProtocolAsync(CreateProtocolDto protocol)
    {
        try
        {
            // Set the user ID from the current authenticated user
            if (string.IsNullOrEmpty(protocol.UserId))
            {
                protocol.UserId = _authService.CurrentUserId;
            }

            var json = JsonSerializer.Serialize(protocol, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("protocols", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PostAsync("protocols", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProtocolDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to create protocol: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while creating protocol", ex);
        }
    }

    public async Task<ProtocolDto> UpdateProtocolAsync(int id, UpdateProtocolDto protocol)
    {
        try
        {
            var json = JsonSerializer.Serialize(protocol, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"protocols/{id}", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PutAsync($"protocols/{id}", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProtocolDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to update protocol {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while updating protocol", ex);
        }
    }

    public async Task DeleteProtocolAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"protocols/{id}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.DeleteAsync($"protocols/{id}");
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
            throw new ServiceException($"Failed to delete protocol {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while deleting protocol", ex);
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