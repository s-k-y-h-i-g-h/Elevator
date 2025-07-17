using System.Net;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui.Services;

public class HttpInterventionService : IInterventionService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpInterventionService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<InterventionDto>> GetInterventionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("interventions");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync("interventions");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var interventions = JsonSerializer.Deserialize<IEnumerable<InterventionDto>>(json, _jsonOptions);
            return interventions ?? new List<InterventionDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve interventions: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving interventions", ex);
        }
    }

    public async Task<InterventionDetailDto?> GetInterventionAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"interventions/{id}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"interventions/{id}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<InterventionDetailDto>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve intervention {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving intervention", ex);
        }
    }

    public async Task<InterventionDto> CreateInterventionAsync(CreateInterventionDto intervention)
    {
        try
        {
            var json = JsonSerializer.Serialize(intervention, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("interventions", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PostAsync("interventions", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InterventionDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to create intervention: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while creating intervention", ex);
        }
    }

    public async Task<InterventionDto> UpdateInterventionAsync(int id, UpdateInterventionDto intervention)
    {
        try
        {
            var json = JsonSerializer.Serialize(intervention, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"interventions/{id}", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PutAsync($"interventions/{id}", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InterventionDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to update intervention {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while updating intervention", ex);
        }
    }

    public async Task DeleteInterventionAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"interventions/{id}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.DeleteAsync($"interventions/{id}");
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
            throw new ServiceException($"Failed to delete intervention {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while deleting intervention", ex);
        }
    }

    public async Task<string?> GetAiInformationAsync(string interventionName)
    {
        try
        {
            var encodedName = Uri.EscapeDataString(interventionName);
            var response = await _httpClient.GetAsync($"interventions/ai-info/{encodedName}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"interventions/ai-info/{encodedName}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve AI information for {interventionName}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving AI information", ex);
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

public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, Exception innerException) : base(message, innerException) { }
}