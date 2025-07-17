using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui.Services;

public class HttpAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorage _secureStorage;
    private UserDto? _currentUser;
    private string? _currentToken;

    private const string TokenKey = "auth_token";
    private const string UserDataKey = "user_data";

    public HttpAuthenticationService(HttpClient httpClient, ISecureStorage secureStorage)
    {
        _httpClient = httpClient;
        _secureStorage = secureStorage;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken) && _currentUser != null;
    public string? CurrentUserId => _currentUser?.Id;

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (authResult?.Success == true && !string.IsNullOrEmpty(authResult.Token))
                {
                    await SetAuthenticationAsync(authResult.Token, authResult.User);
                    return authResult;
                }
                
                return authResult ?? new AuthResult { Success = false, ErrorMessage = "Invalid response from server" };
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = $"Login failed: {response.StatusCode}" 
            };
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = $"Network error: {ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = $"Login failed: {ex.Message}" 
            };
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string? firstName = null, string? lastName = null)
    {
        try
        {
            var registerRequest = new { Email = email, Password = password, FirstName = firstName, LastName = lastName };
            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/register", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (authResult?.Success == true && !string.IsNullOrEmpty(authResult.Token))
                {
                    await SetAuthenticationAsync(authResult.Token, authResult.User);
                    return authResult;
                }
                
                return authResult ?? new AuthResult { Success = false, ErrorMessage = "Invalid response from server" };
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = $"Registration failed: {response.StatusCode}" 
            };
        }
        catch (HttpRequestException ex)
        {
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = $"Network error: {ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = $"Registration failed: {ex.Message}" 
            };
        }
    }

    public Task LogoutAsync()
    {
        try
        {
            // Clear in-memory state
            _currentToken = null;
            _currentUser = null;
            
            // Clear secure storage
            _secureStorage.Remove(TokenKey);
            _secureStorage.Remove(UserDataKey);
            
            // Clear HTTP client authorization header
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        catch (Exception)
        {
            // Ignore errors during logout - we still want to clear local state
        }
        
        return Task.CompletedTask;
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        if (_currentUser != null) 
            return _currentUser;
        
        // Try to restore from secure storage
        try
        {
            var token = await _secureStorage.GetAsync(TokenKey);
            var userData = await _secureStorage.GetAsync(UserDataKey);
            
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userData))
            {
                var user = JsonSerializer.Deserialize<UserDto>(userData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (user != null)
                {
                    await SetAuthenticationAsync(token, user);
                    return _currentUser;
                }
            }
        }
        catch (Exception)
        {
            // If restoration fails, user needs to login again
            await LogoutAsync();
        }
        
        return null;
    }

    private async Task SetAuthenticationAsync(string token, UserDto? user)
    {
        _currentToken = token;
        _currentUser = user;
        
        // Set authorization header
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Store in secure storage
        await _secureStorage.SetAsync(TokenKey, token);
        if (user != null)
        {
            var userJson = JsonSerializer.Serialize(user);
            await _secureStorage.SetAsync(UserDataKey, userJson);
        }
    }

    /// <summary>
    /// Attempts to refresh the token if it's expired
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_currentToken))
            return false;

        try
        {
            var response = await _httpClient.PostAsync("auth/refresh", null);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (authResult?.Success == true && !string.IsNullOrEmpty(authResult.Token))
                {
                    await SetAuthenticationAsync(authResult.Token, authResult.User ?? _currentUser);
                    return true;
                }
            }
        }
        catch (Exception)
        {
            // Token refresh failed
        }
        
        // If refresh fails, logout the user
        await LogoutAsync();
        return false;
    }

    /// <summary>
    /// Handles HTTP 401 responses by attempting token refresh
    /// </summary>
    public async Task<bool> HandleUnauthorizedAsync()
    {
        if (IsAuthenticated)
        {
            return await RefreshTokenAsync();
        }
        
        return false;
    }
}