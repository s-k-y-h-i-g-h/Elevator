using System.Net;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Moq;
using Moq.Protected;

namespace Elevator.Maui.Tests;

[TestClass]
public class HttpAuthenticationServiceTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private Mock<ISecureStorage> _mockSecureStorage;
    private TestableHttpAuthenticationService _authService;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:7001/api/")
        };
        _mockSecureStorage = new Mock<ISecureStorage>();
        _authService = new TestableHttpAuthenticationService(_httpClient, _mockSecureStorage.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _httpClient?.Dispose();
    }

    [TestMethod]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var expectedUser = new UserDto
        {
            Id = "123",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var expectedAuthResult = new AuthResult
        {
            Success = true,
            Token = "test-jwt-token",
            User = expectedUser
        };

        var responseJson = JsonSerializer.Serialize(expectedAuthResult);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri.ToString().EndsWith("auth/login")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        _mockSecureStorage
            .Setup(x => x.SetAsync("auth_token", "test-jwt-token"))
            .Returns(Task.CompletedTask);

        _mockSecureStorage
            .Setup(x => x.SetAsync("user_data", It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync("test@example.com", "password123");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("test-jwt-token", result.Token);
        Assert.AreEqual("test@example.com", result.User?.Email);
        Assert.IsTrue(_authService.IsAuthenticated);
        Assert.AreEqual("123", _authService.CurrentUserId);

        // Verify secure storage was called
        _mockSecureStorage.Verify(x => x.SetAsync("auth_token", "test-jwt-token"), Times.Once);
        _mockSecureStorage.Verify(x => x.SetAsync("user_data", It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task LoginAsync_InvalidCredentials_ReturnsFailureResult()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri.ToString().EndsWith("auth/login")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _authService.LoginAsync("test@example.com", "wrongpassword");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsFalse(_authService.IsAuthenticated);
        Assert.IsNull(_authService.CurrentUserId);
    }

    [TestMethod]
    public void LogoutAsync_ClearsAuthenticationState()
    {
        // Arrange - Set up authenticated state first
        _authService.GetType()
            .GetField("_currentToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_authService, "test-token");
        
        _authService.GetType()
            .GetField("_currentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_authService, new UserDto { Id = "123", Email = "test@example.com" });

        // Act
        _authService.LogoutAsync();

        // Assert
        Assert.IsFalse(_authService.IsAuthenticated);
        Assert.IsNull(_authService.CurrentUserId);
        Assert.IsNull(_httpClient.DefaultRequestHeaders.Authorization);

        // Verify secure storage was cleared
        _mockSecureStorage.Verify(x => x.Remove("auth_token"), Times.Once);
        _mockSecureStorage.Verify(x => x.Remove("user_data"), Times.Once);
    }
}