using System.IdentityModel.Tokens.Jwt;

namespace Elevator.Tests.TestServices;

/// <summary>
/// Testable version of SecureTokenStorage that uses in-memory storage instead of platform-specific secure storage
/// </summary>
public class SecureTokenStorage
{
    private readonly Dictionary<string, string> _storage = new();
    private const string TokenKey = "auth_token";
    private const string ExpirationKey = "auth_token_expiration";
    private const string UserEmailKey = "auth_user_email";

    /// <summary>
    /// Save JWT token securely with expiration information
    /// </summary>
    /// <param name="token">JWT token to store</param>
    /// <param name="expiresAt">Token expiration date</param>
    /// <param name="userEmail">User email associated with the token</param>
    public virtual async Task SaveTokenAsync(string token, DateTime expiresAt, string userEmail)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        try
        {
            _storage[TokenKey] = token;
            _storage[ExpirationKey] = expiresAt.ToBinary().ToString();
            _storage[UserEmailKey] = userEmail;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save token: {ex.Message}");
            throw new InvalidOperationException("Failed to save authentication token securely", ex);
        }
    }

    /// <summary>
    /// Retrieve stored JWT token if it exists and is valid
    /// </summary>
    /// <returns>Token if valid, null if expired or not found</returns>
    public virtual async Task<string?> GetTokenAsync()
    {
        try
        {
            if (!_storage.TryGetValue(TokenKey, out var token) || string.IsNullOrWhiteSpace(token))
                return null;

            // Check if token is expired
            if (await IsTokenExpiredAsync())
            {
                await ClearTokenAsync();
                return null;
            }

            return token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to retrieve token: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get the expiration date of the stored token
    /// </summary>
    /// <returns>Expiration date if available, null otherwise</returns>
    public virtual async Task<DateTime?> GetTokenExpirationAsync()
    {
        try
        {
            if (!_storage.TryGetValue(ExpirationKey, out var expirationString) || 
                string.IsNullOrWhiteSpace(expirationString))
                return null;

            if (long.TryParse(expirationString, out var binaryDate))
            {
                return DateTime.FromBinary(binaryDate);
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to retrieve token expiration: {ex.Message}");
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Get the user email associated with the stored token
    /// </summary>
    /// <returns>User email if available, null otherwise</returns>
    public virtual async Task<string?> GetUserEmailAsync()
    {
        try
        {
            _storage.TryGetValue(UserEmailKey, out var email);
            return email;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to retrieve user email: {ex.Message}");
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Check if the stored token is expired
    /// </summary>
    /// <returns>True if token is expired or expiration cannot be determined</returns>
    public virtual async Task<bool> IsTokenExpiredAsync()
    {
        try
        {
            var expiration = await GetTokenExpirationAsync();
            
            if (expiration == null)
                return true;

            // Add a small buffer (5 minutes) to account for clock skew
            return DateTime.UtcNow.AddMinutes(5) >= expiration.Value;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to check token expiration: {ex.Message}");
            return true; // Assume expired if we can't check
        }
    }

    /// <summary>
    /// Check if a valid token exists in storage
    /// </summary>
    /// <returns>True if a valid token exists</returns>
    public virtual async Task<bool> HasValidTokenAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    /// <summary>
    /// Clear all stored authentication data
    /// </summary>
    public virtual async Task ClearTokenAsync()
    {
        try
        {
            _storage.Remove(TokenKey);
            _storage.Remove(ExpirationKey);
            _storage.Remove(UserEmailKey);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear token: {ex.Message}");
            // Don't throw here as clearing should always succeed from user perspective
        }
    }

    /// <summary>
    /// Parse JWT token to extract claims without validation
    /// </summary>
    /// <param name="token">JWT token to parse</param>
    /// <returns>JWT security token if parsing succeeds</returns>
    public virtual JwtSecurityToken? ParseToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            
            // Parse without validation (we're just extracting claims)
            if (handler.CanReadToken(token))
            {
                return handler.ReadJwtToken(token);
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to parse token: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get user ID from stored token
    /// </summary>
    /// <returns>User ID if available in token claims</returns>
    public virtual async Task<string?> GetUserIdFromTokenAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var jwtToken = ParseToken(token);
            if (jwtToken == null)
                return null;

            // Try different claim types for user ID
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => 
                c.Type == "sub" || 
                c.Type == "userId" || 
                c.Type == "id" ||
                c.Type == JwtRegisteredClaimNames.Sub);

            return userIdClaim?.Value;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get user ID from token: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if secure storage is available on the current platform
    /// </summary>
    /// <returns>True if secure storage is available</returns>
    public static bool IsSecureStorageAvailable()
    {
        // In test environment, always return true
        return true;
    }
}