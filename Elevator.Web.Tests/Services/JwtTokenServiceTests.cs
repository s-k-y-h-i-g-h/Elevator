using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Elevator.Web.Services;
using Elevator.Web.Data.Models;

namespace Elevator.Web.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly ApplicationUser _testUser;

    public JwtTokenServiceTests()
    {
        // Setup configuration for testing
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "TestSecretKeyThatIsAtLeast32CharactersLongForSecurity!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        _jwtTokenService = new JwtTokenService(configuration);
        
        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            UserName = "test@example.com"
        };
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidToken()
    {
        // Act
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void GenerateToken_WithValidUser_TokenContainsCorrectClaims()
    {
        // Act
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Assert
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        var email = _jwtTokenService.GetEmailFromToken(token);

        Assert.Equal(_testUser.Id, userId);
        Assert.Equal(_testUser.Email, email);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Act
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.True(principal.Identity?.IsAuthenticated);
        
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        var emailClaim = principal.FindFirst(ClaimTypes.Email);
        
        Assert.NotNull(userIdClaim);
        Assert.NotNull(emailClaim);
        Assert.Equal(_testUser.Id, userIdClaim.Value);
        Assert.Equal(_testUser.Email, emailClaim.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtTokenService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange - Create a token that will be expired by the time we validate it
        // We'll create a token with a very short expiration and then wait for it to expire
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "TestSecretKeyThatIsAtLeast32CharactersLongForSecurity!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationMinutes"] = "1" // 1 minute expiration (minimum)
            })
            .Build();

        var shortExpirationService = new JwtTokenService(configuration);
        
        // Create a token with a past expiration time by manually crafting it
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("TestSecretKeyThatIsAtLeast32CharactersLongForSecurity!");
        
        var pastTime = DateTime.UtcNow.AddMinutes(-10);
        var expiredTokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, _testUser.Id),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, _testUser.Email ?? string.Empty)
            }),
            NotBefore = pastTime,
            Expires = pastTime.AddMinutes(5), // Expired 5 minutes ago
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), 
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var expiredToken = tokenHandler.WriteToken(tokenHandler.CreateToken(expiredTokenDescriptor));

        // Act
        var principal = shortExpirationService.ValidateToken(expiredToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void IsTokenExpired_WithValidToken_ReturnsFalse()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Act
        var isExpired = _jwtTokenService.IsTokenExpired(token);

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsTokenExpired_WithExpiredToken_ReturnsTrue()
    {
        // Arrange - Manually create an expired token
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("TestSecretKeyThatIsAtLeast32CharactersLongForSecurity!");
        
        var pastTime = DateTime.UtcNow.AddMinutes(-10);
        var expiredTokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, _testUser.Id),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, _testUser.Email ?? string.Empty)
            }),
            NotBefore = pastTime,
            Expires = pastTime.AddMinutes(5), // Expired 5 minutes ago
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), 
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var expiredToken = tokenHandler.WriteToken(tokenHandler.CreateToken(expiredTokenDescriptor));

        // Act
        var isExpired = _jwtTokenService.IsTokenExpired(expiredToken);

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void IsTokenExpired_WithInvalidToken_ReturnsTrue()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isExpired = _jwtTokenService.IsTokenExpired(invalidToken);

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsCorrectUserId()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Act
        var userId = _jwtTokenService.GetUserIdFromToken(token);

        // Assert
        Assert.Equal(_testUser.Id, userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _jwtTokenService.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void GetEmailFromToken_WithValidToken_ReturnsCorrectEmail()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Act
        var email = _jwtTokenService.GetEmailFromToken(token);

        // Assert
        Assert.Equal(_testUser.Email, email);
    }

    [Fact]
    public void GetEmailFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var email = _jwtTokenService.GetEmailFromToken(invalidToken);

        // Assert
        Assert.Null(email);
    }

    [Fact]
    public void GetExpirationFromToken_WithValidToken_ReturnsCorrectExpiration()
    {
        // Arrange
        var beforeGeneration = DateTime.UtcNow;
        var token = _jwtTokenService.GenerateToken(_testUser);
        var afterGeneration = DateTime.UtcNow;

        // Act
        var expiration = _jwtTokenService.GetExpirationFromToken(token);

        // Assert
        Assert.NotNull(expiration);
        
        // Should be approximately 60 minutes from now (allowing for test execution time)
        // Add some buffer time for test execution
        var minExpectedExpiration = beforeGeneration.AddMinutes(59); // Allow 1 minute buffer
        var maxExpectedExpiration = afterGeneration.AddMinutes(61); // Allow 1 minute buffer
        
        Assert.True(expiration >= minExpectedExpiration, 
            $"Expiration {expiration} should be >= {minExpectedExpiration}");
        Assert.True(expiration <= maxExpectedExpiration, 
            $"Expiration {expiration} should be <= {maxExpectedExpiration}");
    }

    [Fact]
    public void GetExpirationFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var expiration = _jwtTokenService.GetExpirationFromToken(invalidToken);

        // Assert
        Assert.Null(expiration);
    }

    [Fact]
    public void GenerateToken_WithDifferentUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var user1 = new ApplicationUser { Id = "user1", Email = "user1@example.com" };
        var user2 = new ApplicationUser { Id = "user2", Email = "user2@example.com" };

        // Act
        var token1 = _jwtTokenService.GenerateToken(user1);
        var token2 = _jwtTokenService.GenerateToken(user2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateToken_WithWrongSecretKey_ReturnsNull()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken(_testUser);
        
        // Create service with different secret key
        var wrongKeyConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "DifferentSecretKeyThatIsAtLeast32CharactersLongForSecurity!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var wrongKeyService = new JwtTokenService(wrongKeyConfiguration);

        // Act
        var principal = wrongKeyService.ValidateToken(token);

        // Assert
        Assert.Null(principal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not.a.jwt")]
    [InlineData("invalid")]
    public void ValidateToken_WithMalformedTokens_ReturnsNull(string malformedToken)
    {
        // Act
        var principal = _jwtTokenService.ValidateToken(malformedToken);

        // Assert
        Assert.Null(principal);
    }
}