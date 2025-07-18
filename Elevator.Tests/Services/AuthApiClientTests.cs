using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Elevator.Tests.TestServices;
using Elevator.Shared.Models;

namespace Elevator.Tests.Services;

/// <summary>
/// Unit tests for AuthApiClient service
/// Tests HTTP communication, error handling, and retry logic
/// Requirements: 6.1, 4.1, 4.2
/// </summary>
public class AuthApiClientTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly AuthApiClient _authApiClient;

    public AuthApiClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        _authApiClient = new AuthApiClient(_httpClient);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var expectedResponse = new AuthResponse
        {
            Success = true,
            Token = "test-jwt-token",
            Message = "Registration successful",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("api/auth/register")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("test-jwt-token", result.Token);
        Assert.Equal("Registration successful", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithConflictResponse_ShouldReturnErrorResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var errorResponse = new AuthResponse
        {
            Success = false,
            Message = "Email address is already registered"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Email address is already registered", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithNetworkError_ShouldRetryAndReturnError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _authApiClient.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Network error", result.Message);

        // Verify retry attempts (should be called 3 times)
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        var expectedResponse = new AuthResponse
        {
            Success = true,
            Token = "test-jwt-token",
            Message = "Login successful",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("api/auth/login")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("test-jwt-token", result.Token);
        Assert.Equal("Login successful", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithTimeout_ShouldRetryAndReturnTimeoutError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout", new TimeoutException()));

        // Act
        var result = await _authApiClient.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Request timed out", result.Message);
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WithSuccessResponse_ShouldReturnTrue()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("api/auth/logout")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.LogoutAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LogoutAsync_WithServerError_ShouldReturnTrueAnyway()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.LogoutAsync();

        // Assert
        Assert.True(result); // Should return true even on server error
    }

    [Fact]
    public async Task LogoutAsync_WithNetworkException_ShouldReturnTrue()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _authApiClient.LogoutAsync();

        // Assert
        Assert.True(result); // Should return true even on network error
    }

    #endregion

    #region SetAuthorizationHeader Tests

    [Fact]
    public void SetAuthorizationHeader_WithValidToken_ShouldSetBearerToken()
    {
        // Arrange
        var token = "test-jwt-token";

        // Act
        _authApiClient.SetAuthorizationHeader(token);

        // Assert
        Assert.NotNull(_httpClient.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", _httpClient.DefaultRequestHeaders.Authorization.Scheme);
        Assert.Equal(token, _httpClient.DefaultRequestHeaders.Authorization.Parameter);
    }

    [Fact]
    public void SetAuthorizationHeader_WithEmptyToken_ShouldClearHeader()
    {
        // Arrange
        _authApiClient.SetAuthorizationHeader("test-token"); // Set first
        
        // Act
        _authApiClient.SetAuthorizationHeader("");

        // Assert
        Assert.Null(_httpClient.DefaultRequestHeaders.Authorization);
    }

    [Fact]
    public void SetAuthorizationHeader_WithNullToken_ShouldClearHeader()
    {
        // Arrange
        _authApiClient.SetAuthorizationHeader("test-token"); // Set first
        
        // Act
        _authApiClient.SetAuthorizationHeader(null!);

        // Assert
        Assert.Null(_httpClient.DefaultRequestHeaders.Authorization);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task RegisterAsync_WithInvalidJsonResponse_ShouldReturnFormatError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Invalid response format", result.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Invalid request data")]
    [InlineData(HttpStatusCode.Unauthorized, "Invalid email or password")]
    [InlineData(HttpStatusCode.Conflict, "Email address is already registered")]
    [InlineData(HttpStatusCode.InternalServerError, "Server error occurred")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Service is temporarily unavailable")]
    public async Task LoginAsync_WithVariousStatusCodes_ShouldReturnAppropriateErrorMessages(
        HttpStatusCode statusCode, string expectedMessage)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("Error", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authApiClient.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains(expectedMessage, result.Message);
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}