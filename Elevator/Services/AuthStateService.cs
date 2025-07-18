using System.ComponentModel;
using Elevator.Shared.Models;

namespace Elevator.Services;

/// <summary>
/// Service for managing authentication state across the application
/// </summary>
public class AuthStateService : INotifyPropertyChanged
{
    private readonly AuthApiClient _apiClient;
    private readonly SecureTokenStorage _tokenStorage;
    private bool _isAuthenticated;
    private UserInfo? _currentUser;
    private bool _isInitialized;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<AuthenticationStateChangedEventArgs>? AuthenticationStateChanged;

    public AuthStateService(AuthApiClient apiClient, SecureTokenStorage tokenStorage)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
    }

    /// <summary>
    /// Gets whether the user is currently authenticated
    /// </summary>
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (_isAuthenticated != value)
            {
                _isAuthenticated = value;
                OnPropertyChanged(nameof(IsAuthenticated));
                AuthenticationStateChanged?.Invoke(this, new AuthenticationStateChangedEventArgs(value));
            }
        }
    }

    /// <summary>
    /// Gets the current authenticated user information
    /// </summary>
    public UserInfo? CurrentUser
    {
        get => _currentUser;
        private set
        {
            if (_currentUser != value)
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
    }

    /// <summary>
    /// Gets whether the service has been initialized
    /// </summary>
    public bool IsInitialized
    {
        get => _isInitialized;
        private set
        {
            if (_isInitialized != value)
            {
                _isInitialized = value;
                OnPropertyChanged(nameof(IsInitialized));
            }
        }
    }

    /// <summary>
    /// Initialize the authentication state service and attempt to restore session
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await TryRestoreSessionAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize auth state: {ex.Message}");
            // Don't throw - just ensure we're in a clean unauthenticated state
            await ClearAuthenticationStateAsync();
        }
        finally
        {
            IsInitialized = true;
        }
    }

    /// <summary>
    /// Attempt to restore authentication session from stored token
    /// </summary>
    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var token = await _tokenStorage.GetTokenAsync();
            
            if (string.IsNullOrWhiteSpace(token))
            {
                await ClearAuthenticationStateAsync();
                return false;
            }

            // Set the authorization header for API calls
            _apiClient.SetAuthorizationHeader(token);

            // Extract user information from token
            var userEmail = await _tokenStorage.GetUserEmailAsync();
            var userId = await _tokenStorage.GetUserIdFromTokenAsync();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                CurrentUser = new UserInfo
                {
                    Id = int.TryParse(userId, out var id) ? id : 0,
                    Email = userEmail,
                    CreatedAt = DateTime.UtcNow // We don't have this from token, could be enhanced
                };

                IsAuthenticated = true;
                return true;
            }

            // If we can't extract user info, clear the invalid token
            await ClearAuthenticationStateAsync();
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to restore session: {ex.Message}");
            await ClearAuthenticationStateAsync();
            return false;
        }
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration request data</param>
    /// <returns>Authentication response</returns>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _apiClient.RegisterAsync(request);
            
            if (response.Success && !string.IsNullOrWhiteSpace(response.Token))
            {
                await SetAuthenticationStateAsync(response, request.Email);
            }

            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = "Registration failed. Please try again."
            };
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request data</param>
    /// <returns>Authentication response</returns>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _apiClient.LoginAsync(request);
            
            if (response.Success && !string.IsNullOrWhiteSpace(response.Token))
            {
                await SetAuthenticationStateAsync(response, request.Email);
            }

            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login failed: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = "Login failed. Please try again."
            };
        }
    }

    /// <summary>
    /// Logout the current user
    /// </summary>
    public async Task<bool> LogoutAsync()
    {
        try
        {
            // Call API logout (don't fail if this doesn't work)
            await _apiClient.LogoutAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API logout failed: {ex.Message}");
            // Continue with local logout even if API call fails
        }

        // Always clear local authentication state
        await ClearAuthenticationStateAsync();
        return true;
    }

    /// <summary>
    /// Check if the current session is still valid
    /// </summary>
    public async Task<bool> IsSessionValidAsync()
    {
        try
        {
            if (!IsAuthenticated)
                return false;

            // Check if token is expired
            if (await _tokenStorage.IsTokenExpiredAsync())
            {
                await ClearAuthenticationStateAsync();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Session validation failed: {ex.Message}");
            await ClearAuthenticationStateAsync();
            return false;
        }
    }

    /// <summary>
    /// Refresh the authentication token if needed
    /// </summary>
    public async Task<bool> RefreshTokenIfNeededAsync()
    {
        try
        {
            // Check if token will expire soon (within 10 minutes)
            var expiration = await _tokenStorage.GetTokenExpirationAsync();
            if (expiration == null)
                return false;

            if (DateTime.UtcNow.AddMinutes(10) >= expiration.Value)
            {
                // Token is expiring soon - for now, we'll just clear the session
                // In a full implementation, you might call a refresh endpoint
                await ClearAuthenticationStateAsync();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Token refresh check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Set authentication state after successful login/registration
    /// </summary>
    private async Task SetAuthenticationStateAsync(AuthResponse response, string userEmail)
    {
        try
        {
            // Store token securely
            await _tokenStorage.SaveTokenAsync(response.Token, response.ExpiresAt, userEmail);

            // Set API client authorization header
            _apiClient.SetAuthorizationHeader(response.Token);

            // Extract user information from token
            var userId = await _tokenStorage.GetUserIdFromTokenAsync();

            // Set current user
            CurrentUser = new UserInfo
            {
                Id = int.TryParse(userId, out var id) ? id : 0,
                Email = userEmail,
                CreatedAt = DateTime.UtcNow // Could be enhanced to get from token
            };

            // Set authenticated state
            IsAuthenticated = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set authentication state: {ex.Message}");
            await ClearAuthenticationStateAsync();
            throw;
        }
    }

    /// <summary>
    /// Clear all authentication state
    /// </summary>
    private async Task ClearAuthenticationStateAsync()
    {
        try
        {
            // Clear stored token
            await _tokenStorage.ClearTokenAsync();

            // Clear API client authorization header
            _apiClient.SetAuthorizationHeader(string.Empty);

            // Clear current user
            CurrentUser = null;

            // Set unauthenticated state
            IsAuthenticated = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear authentication state: {ex.Message}");
            // Still set the state even if clearing fails
            CurrentUser = null;
            IsAuthenticated = false;
        }
    }

    /// <summary>
    /// Raise property changed event
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Event arguments for authentication state changes
/// </summary>
public class AuthenticationStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; }

    public AuthenticationStateChangedEventArgs(bool isAuthenticated)
    {
        IsAuthenticated = isAuthenticated;
    }
}