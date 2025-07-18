using System.ComponentModel;
using Moq;
using Elevator.Tests.TestServices;
using Elevator.Shared.Models;

namespace Elevator.Tests.Services;

/// <summary>
/// Unit tests for AuthStateService
/// Tests authentication state management, session restoration, and event handling
/// Requirements: 6.1, 4.1, 4.2
/// </summary>
public class AuthStateServiceTests
{
    private readonly Mock<AuthApiClient> _mockApiClient;
    private readonly Mock<SecureTokenStorage> _mockTokenStorage;
    private readonly AuthStateService _authStateService;

    public AuthStateServiceTests()
    {
        _mockApiClient = new Mock<AuthApiClient>(Mock.Of<HttpClient>());
        _mockTokenStorage = new Mock<SecureTokenStorage>();
        _authStateService = new AuthStateService(_mockApiClient.Object, _mockTokenStorage.Object);
    }

    #region Initialization Tests

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);
        Assert.False(_authStateService.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_WithValidToken_ShouldRestoreSession()
    {
        // Arrange
        var token = "valid-jwt-token";
        var userEmail = "test@example.com";
        var userId = "123";

        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ReturnsAsync(token);
        _mockTokenStorage.Setup(x => x.GetUserEmailAsync()).ReturnsAsync(userEmail);
        _mockTokenStorage.Setup(x => x.GetUserIdFromTokenAsync()).ReturnsAsync(userId);

        // Act
        await _authStateService.InitializeAsync();

        // Assert
        Assert.True(_authStateService.IsInitialized);
        Assert.True(_authStateService.IsAuthenticated);
        Assert.NotNull(_authStateService.CurrentUser);
        Assert.Equal(userEmail, _authStateService.CurrentUser.Email);
        Assert.Equal(123, _authStateService.CurrentUser.Id);

        _mockApiClient.Verify(x => x.SetAuthorizationHeader(token), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithNoToken_ShouldNotAuthenticate()
    {
        // Arrange
        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ReturnsAsync((string?)null);

        // Act
        await _authStateService.InitializeAsync();

        // Assert
        Assert.True(_authStateService.IsInitialized);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);
    }

    [Fact]
    public async Task InitializeAsync_WithException_ShouldSetInitializedAndClearState()
    {
        // Arrange
        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ThrowsAsync(new Exception("Storage error"));

        // Act
        await _authStateService.InitializeAsync();

        // Assert
        Assert.True(_authStateService.IsInitialized);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);
    }

    #endregion

    #region Session Restoration Tests

    [Fact]
    public async Task TryRestoreSessionAsync_WithValidTokenAndUserInfo_ShouldReturnTrue()
    {
        // Arrange
        var token = "valid-jwt-token";
        var userEmail = "test@example.com";
        var userId = "123";

        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ReturnsAsync(token);
        _mockTokenStorage.Setup(x => x.GetUserEmailAsync()).ReturnsAsync(userEmail);
        _mockTokenStorage.Setup(x => x.GetUserIdFromTokenAsync()).ReturnsAsync(userId);

        // Act
        var result = await _authStateService.TryRestoreSessionAsync();

        // Assert
        Assert.True(result);
        Assert.True(_authStateService.IsAuthenticated);
        Assert.NotNull(_authStateService.CurrentUser);
        Assert.Equal(userEmail, _authStateService.CurrentUser.Email);
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WithNoToken_ShouldReturnFalse()
    {
        // Arrange
        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ReturnsAsync((string?)null);

        // Act
        var result = await _authStateService.TryRestoreSessionAsync();

        // Assert
        Assert.False(result);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);
    }

    [Fact]
    public async Task TryRestoreSessionAsync_WithTokenButNoUserEmail_ShouldReturnFalse()
    {
        // Arrange
        var token = "valid-jwt-token";

        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ReturnsAsync(token);
        _mockTokenStorage.Setup(x => x.GetUserEmailAsync()).ReturnsAsync((string?)null);

        // Act
        var result = await _authStateService.TryRestoreSessionAsync();

        // Assert
        Assert.False(result);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);

        _mockTokenStorage.Verify(x => x.ClearTokenAsync(), Times.Once);
    }

    #endregion

    #region Registration Tests

    [Fact]
    public async Task RegisterAsync_WithSuccessfulResponse_ShouldSetAuthenticationState()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var response = new AuthResponse
        {
            Success = true,
            Token = "jwt-token",
            Message = "Registration successful",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockApiClient.Setup(x => x.RegisterAsync(request)).ReturnsAsync(response);
        _mockTokenStorage.Setup(x => x.GetUserIdFromTokenAsync()).ReturnsAsync("123");

        // Act
        var result = await _authStateService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(_authStateService.IsAuthenticated);
        Assert.NotNull(_authStateService.CurrentUser);
        Assert.Equal(request.Email, _authStateService.CurrentUser.Email);

        _mockTokenStorage.Verify(x => x.SaveTokenAsync(response.Token, response.ExpiresAt, request.Email), Times.Once);
        _mockApiClient.Verify(x => x.SetAuthorizationHeader(response.Token), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithFailedResponse_ShouldNotSetAuthenticationState()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var response = new AuthResponse
        {
            Success = false,
            Message = "Email already exists"
        };

        _mockApiClient.Setup(x => x.RegisterAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _authStateService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);

        _mockTokenStorage.Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithException_ShouldReturnErrorResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        _mockApiClient.Setup(x => x.RegisterAsync(request)).ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _authStateService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Registration failed", result.Message);
        Assert.False(_authStateService.IsAuthenticated);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithSuccessfulResponse_ShouldSetAuthenticationState()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        var response = new AuthResponse
        {
            Success = true,
            Token = "jwt-token",
            Message = "Login successful",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockApiClient.Setup(x => x.LoginAsync(request)).ReturnsAsync(response);
        _mockTokenStorage.Setup(x => x.GetUserIdFromTokenAsync()).ReturnsAsync("123");

        // Act
        var result = await _authStateService.LoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(_authStateService.IsAuthenticated);
        Assert.NotNull(_authStateService.CurrentUser);
        Assert.Equal(request.Email, _authStateService.CurrentUser.Email);

        _mockTokenStorage.Verify(x => x.SaveTokenAsync(response.Token, response.ExpiresAt, request.Email), Times.Once);
        _mockApiClient.Verify(x => x.SetAuthorizationHeader(response.Token), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithFailedResponse_ShouldNotSetAuthenticationState()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var response = new AuthResponse
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _mockApiClient.Setup(x => x.LoginAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _authStateService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task LogoutAsync_ShouldClearAuthenticationState()
    {
        // Arrange - Set up authenticated state first
        await SetupAuthenticatedState();

        // Act
        var result = await _authStateService.LogoutAsync();

        // Assert
        Assert.True(result);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);

        _mockApiClient.Verify(x => x.LogoutAsync(), Times.Once);
        _mockTokenStorage.Verify(x => x.ClearTokenAsync(), Times.Once);
        _mockApiClient.Verify(x => x.SetAuthorizationHeader(string.Empty), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithApiFailure_ShouldStillClearLocalState()
    {
        // Arrange
        await SetupAuthenticatedState();
        _mockApiClient.Setup(x => x.LogoutAsync()).ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _authStateService.LogoutAsync();

        // Assert
        Assert.True(result);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);

        _mockTokenStorage.Verify(x => x.ClearTokenAsync(), Times.Once);
    }

    #endregion

    #region Session Validation Tests

    [Fact]
    public async Task IsSessionValidAsync_WithAuthenticatedAndValidToken_ShouldReturnTrue()
    {
        // Arrange
        await SetupAuthenticatedState();
        _mockTokenStorage.Setup(x => x.IsTokenExpiredAsync()).ReturnsAsync(false);

        // Act
        var result = await _authStateService.IsSessionValidAsync();

        // Assert
        Assert.True(result);
        Assert.True(_authStateService.IsAuthenticated);
    }

    [Fact]
    public async Task IsSessionValidAsync_WithUnauthenticatedState_ShouldReturnFalse()
    {
        // Act
        var result = await _authStateService.IsSessionValidAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsSessionValidAsync_WithExpiredToken_ShouldClearStateAndReturnFalse()
    {
        // Arrange
        await SetupAuthenticatedState();
        _mockTokenStorage.Setup(x => x.IsTokenExpiredAsync()).ReturnsAsync(true);

        // Act
        var result = await _authStateService.IsSessionValidAsync();

        // Assert
        Assert.False(result);
        Assert.False(_authStateService.IsAuthenticated);
        Assert.Null(_authStateService.CurrentUser);

        _mockTokenStorage.Verify(x => x.ClearTokenAsync(), Times.Once);
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshTokenIfNeededAsync_WithNoExpiration_ShouldReturnFalse()
    {
        // Arrange
        _mockTokenStorage.Setup(x => x.GetTokenExpirationAsync()).ReturnsAsync((DateTime?)null);

        // Act
        var result = await _authStateService.RefreshTokenIfNeededAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RefreshTokenIfNeededAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddHours(1); // Valid for 1 hour
        _mockTokenStorage.Setup(x => x.GetTokenExpirationAsync()).ReturnsAsync(expiration);

        // Act
        var result = await _authStateService.RefreshTokenIfNeededAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RefreshTokenIfNeededAsync_WithExpiringSoonToken_ShouldClearStateAndReturnFalse()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddMinutes(5); // Expires in 5 minutes (within 10 minute threshold)
        _mockTokenStorage.Setup(x => x.GetTokenExpirationAsync()).ReturnsAsync(expiration);

        // Act
        var result = await _authStateService.RefreshTokenIfNeededAsync();

        // Assert
        Assert.False(result);
        _mockTokenStorage.Verify(x => x.ClearTokenAsync(), Times.Once);
    }

    #endregion

    #region Property Change Events Tests

    [Fact]
    public async Task IsAuthenticated_PropertyChange_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        _authStateService.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = e.PropertyName;
        };

        // Act
        await SetupAuthenticatedState();

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(nameof(AuthStateService.IsAuthenticated), changedPropertyName);
    }

    [Fact]
    public async Task AuthenticationStateChanged_ShouldRaiseEvent()
    {
        // Arrange
        var eventRaised = false;
        bool eventIsAuthenticated = false;

        _authStateService.AuthenticationStateChanged += (sender, e) =>
        {
            eventRaised = true;
            eventIsAuthenticated = e.IsAuthenticated;
        };

        // Act
        await SetupAuthenticatedState();

        // Assert
        Assert.True(eventRaised);
        Assert.True(eventIsAuthenticated);
    }

    #endregion

    #region Helper Methods

    private async Task SetupAuthenticatedState()
    {
        var token = "jwt-token";
        var userEmail = "test@example.com";
        var userId = "123";

        _mockTokenStorage.Setup(x => x.GetTokenAsync()).ReturnsAsync(token);
        _mockTokenStorage.Setup(x => x.GetUserEmailAsync()).ReturnsAsync(userEmail);
        _mockTokenStorage.Setup(x => x.GetUserIdFromTokenAsync()).ReturnsAsync(userId);

        await _authStateService.TryRestoreSessionAsync();
    }

    #endregion
}