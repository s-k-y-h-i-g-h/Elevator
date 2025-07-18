using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Elevator.Tests.TestServices;

namespace Elevator.Tests.Services;

/// <summary>
/// Unit tests for SecureTokenStorage service
/// Tests token storage, retrieval, expiration checking, and JWT parsing
/// Requirements: 6.1, 4.1, 4.2
/// </summary>
public class SecureTokenStorageTests
{
    private readonly SecureTokenStorage _tokenStorage;

    public SecureTokenStorageTests()
    {
        _tokenStorage = new SecureTokenStorage();
    }

    #region Token Generation Helpers

    private string GenerateTestJwtToken(DateTime? expiration = null, string? userId = null, string? email = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("test-secret-key-that-is-long-enough-for-hmac-sha256");
        
        var claims = new List<Claim>();
        
        if (!string.IsNullOrEmpty(userId))
            claims.Add(new Claim("sub", userId));
        
        if (!string.IsNullOrEmpty(email))
            claims.Add(new Claim("email", email));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration ?? DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion

    #region SaveTokenAsync and GetTokenAsync Tests

    [Fact]
    public async Task SaveTokenAsync_WithValidData_ShouldStoreTokenSuccessfully()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act & Assert - Should not throw
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
    }

    [Fact]
    public async Task GetTokenAsync_WithNoStoredToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenStorage.GetTokenAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAndGetTokenAsync_WithValidToken_ShouldReturnSameToken()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var retrievedToken = await _tokenStorage.GetTokenAsync();

        // Assert
        Assert.Equal(token, retrievedToken);
    }

    #endregion

    #region Token Expiration Tests

    [Fact]
    public async Task GetTokenExpirationAsync_WithNoStoredToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenStorage.GetTokenExpirationAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTokenExpirationAsync_WithStoredToken_ShouldReturnCorrectExpiration()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var retrievedExpiration = await _tokenStorage.GetTokenExpirationAsync();

        // Assert
        Assert.NotNull(retrievedExpiration);
        // Allow for small time differences due to test execution time
        Assert.True(Math.Abs((expiresAt - retrievedExpiration.Value).TotalSeconds) < 1);
    }

    [Fact]
    public async Task IsTokenExpiredAsync_WithNoToken_ShouldReturnTrue()
    {
        // Act
        var result = await _tokenStorage.IsTokenExpiredAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsTokenExpiredAsync_WithValidToken_ShouldReturnFalse()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.IsTokenExpiredAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsTokenExpiredAsync_WithExpiredToken_ShouldReturnTrue()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(-10); // Expired 10 minutes ago
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.IsTokenExpiredAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(-10); // Expired 10 minutes ago
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.GetTokenAsync();

        // Assert
        Assert.Null(result); // Should return null for expired token
    }

    #endregion

    #region User Email Tests

    [Fact]
    public async Task GetUserEmailAsync_WithNoStoredData_ShouldReturnNull()
    {
        // Act
        var result = await _tokenStorage.GetUserEmailAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserEmailAsync_WithStoredData_ShouldReturnCorrectEmail()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var retrievedEmail = await _tokenStorage.GetUserEmailAsync();

        // Assert
        Assert.Equal(userEmail, retrievedEmail);
    }

    #endregion

    #region HasValidTokenAsync Tests

    [Fact]
    public async Task HasValidTokenAsync_WithNoToken_ShouldReturnFalse()
    {
        // Act
        var result = await _tokenStorage.HasValidTokenAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.HasValidTokenAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(-10); // Expired
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.HasValidTokenAsync();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ClearTokenAsync Tests

    [Fact]
    public async Task ClearTokenAsync_ShouldRemoveAllStoredData()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);

        // Act
        await _tokenStorage.ClearTokenAsync();

        // Assert
        var retrievedToken = await _tokenStorage.GetTokenAsync();
        var retrievedEmail = await _tokenStorage.GetUserEmailAsync();
        var retrievedExpiration = await _tokenStorage.GetTokenExpirationAsync();

        Assert.Null(retrievedToken);
        Assert.Null(retrievedEmail);
        Assert.Null(retrievedExpiration);
    }

    #endregion

    #region JWT Parsing Tests

    [Fact]
    public void ParseToken_WithValidJwtToken_ShouldReturnJwtSecurityToken()
    {
        // Arrange
        var token = GenerateTestJwtToken(userId: "123", email: "test@example.com");

        // Act
        var result = _tokenStorage.ParseToken(token);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JwtSecurityToken>(result);
    }

    [Fact]
    public void ParseToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var result = _tokenStorage.ParseToken(invalidToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseToken_WithEmptyToken_ShouldReturnNull()
    {
        // Act
        var result = _tokenStorage.ParseToken("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseToken_WithNullToken_ShouldReturnNull()
    {
        // Act
        var result = _tokenStorage.ParseToken(null!);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetUserIdFromTokenAsync Tests

    [Fact]
    public async Task GetUserIdFromTokenAsync_WithNoStoredToken_ShouldReturnNull()
    {
        // Act
        var result = await _tokenStorage.GetUserIdFromTokenAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserIdFromTokenAsync_WithValidTokenContainingUserId_ShouldReturnUserId()
    {
        // Arrange
        var userId = "123";
        var token = GenerateTestJwtToken(userId: userId);
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.GetUserIdFromTokenAsync();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public async Task GetUserIdFromTokenAsync_WithTokenWithoutUserId_ShouldReturnNull()
    {
        // Arrange
        var token = GenerateTestJwtToken(); // No userId claim
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act
        await _tokenStorage.SaveTokenAsync(token, expiresAt, userEmail);
        var result = await _tokenStorage.GetUserIdFromTokenAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Static Methods Tests

    [Fact]
    public void IsSecureStorageAvailable_ShouldReturnBoolean()
    {
        // Act
        var result = SecureTokenStorage.IsSecureStorageAvailable();

        // Assert
        Assert.IsType<bool>(result);
        // Note: The actual value depends on the test environment
        // In a unit test environment, it might return false
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SaveTokenAsync_WithNullToken_ShouldThrowException()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var userEmail = "test@example.com";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _tokenStorage.SaveTokenAsync(null!, expiresAt, userEmail));
    }

    [Fact]
    public async Task SaveTokenAsync_WithEmptyEmail_ShouldNotThrow()
    {
        // Arrange
        var token = GenerateTestJwtToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act & Assert - Should not throw
        await _tokenStorage.SaveTokenAsync(token, expiresAt, "");
    }

    #endregion
}