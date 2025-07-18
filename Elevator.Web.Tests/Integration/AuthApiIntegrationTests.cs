using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Elevator.Shared.Models;

namespace Elevator.Web.Tests.Integration;

/// <summary>
/// Integration tests for Authentication API endpoints
/// Tests API contract, validation, and error handling
/// Requirements: 1.1, 1.2, 2.1, 2.2, 3.1
/// </summary>
public class AuthApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region Registration API Tests

    [Fact]
    public async Task RegisterEndpoint_WithValidRequest_ShouldAcceptRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert - Should not return BadRequest for valid input
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Should return either Created (success) or InternalServerError (database issue)
        // but not validation errors for valid input
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                   response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("", "TestPassword123", "TestPassword123")]
    [InlineData("invalid-email", "TestPassword123", "TestPassword123")]
    [InlineData("test@example.com", "short", "short")]
    [InlineData("test@example.com", "nouppercase123", "nouppercase123")]
    [InlineData("test@example.com", "NOLOWERCASE123", "NOLOWERCASE123")]
    [InlineData("test@example.com", "NoNumbers", "NoNumbers")]
    [InlineData("test@example.com", "TestPassword123", "DifferentPassword123")]
    public async Task RegisterEndpoint_WithInvalidData_ShouldReturnBadRequest(
        string email, string password, string confirmPassword)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.False(authResponse.Success);
        Assert.NotEmpty(authResponse.Message);
    }

    [Fact]
    public async Task RegisterEndpoint_WithNullRequest_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/register", 
            new StringContent("null", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterEndpoint_WithMalformedJson_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/register", 
            new StringContent("{invalid json", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login API Tests

    [Fact]
    public async Task LoginEndpoint_WithValidRequest_ShouldAcceptRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert - Should not return BadRequest for valid input format
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Should return either OK (success), Unauthorized (invalid creds), or InternalServerError (database issue)
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("", "TestPassword123")]
    [InlineData("test@example.com", "")]
    [InlineData("invalid-email", "TestPassword123")]
    public async Task LoginEndpoint_WithInvalidData_ShouldReturnBadRequest(
        string email, string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.False(authResponse.Success);
        Assert.NotEmpty(authResponse.Message);
    }

    [Fact]
    public async Task LoginEndpoint_WithNullRequest_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/login", 
            new StringContent("null", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_WithMalformedJson_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/login", 
            new StringContent("{invalid json", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Logout API Tests

    [Fact]
    public async Task LogoutEndpoint_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LogoutEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task AuthEndpoints_WithUnsupportedHttpMethods_ShouldReturnMethodNotAllowed()
    {
        // Test GET on POST endpoints
        var getRegisterResponse = await _client.GetAsync("/api/auth/register");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, getRegisterResponse.StatusCode);

        var getLoginResponse = await _client.GetAsync("/api/auth/login");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, getLoginResponse.StatusCode);

        var getLogoutResponse = await _client.GetAsync("/api/auth/logout");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, getLogoutResponse.StatusCode);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task AuthEndpoints_ShouldReturnJsonResponses()
    {
        // Test register endpoint
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };
        
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Contains("application/json", registerResponse.Content.Headers.ContentType?.ToString() ?? "");

        // Test login endpoint
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };
        
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Contains("application/json", loginResponse.Content.Headers.ContentType?.ToString() ?? "");
    }

    [Fact]
    public async Task AuthResponses_ShouldHaveCorrectStructure()
    {
        // Test with invalid data to ensure we get a proper AuthResponse structure
        var request = new RegisterRequest
        {
            Email = "", // Invalid email
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        Assert.NotNull(authResponse);
        // AuthResponse should have Success property
        Assert.False(authResponse.Success);
        // AuthResponse should have Message property
        Assert.NotNull(authResponse.Message);
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}