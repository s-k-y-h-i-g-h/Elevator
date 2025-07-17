using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.DTOs;
using Elevator.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Elevator.Web.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        // Setup UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

        // Setup SignInManager mock
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(_mockUserManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

        // Setup Configuration mock
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("your-super-secret-jwt-key-that-should-be-at-least-32-characters-long");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("ElevatorApp");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("ElevatorApp");

        // Setup Logger mock
        _mockLogger = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(_mockUserManager.Object, _mockSignInManager.Object, _mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsSuccessWithToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(okResult.Value);
        
        Assert.True(authResult.Success);
        Assert.NotNull(authResult.Token);
        Assert.NotNull(authResult.User);
        Assert.Equal("test@example.com", authResult.User.Email);
        Assert.Equal("Test", authResult.User.FirstName);
        Assert.Equal("User", authResult.User.LastName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "TestPassword123",
            FirstName = "Test",
            LastName = "User"
        };

        var existingUser = new ApplicationUser { Email = registerRequest.Email };
        _mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(badRequestResult.Value);
        
        Assert.False(authResult.Success);
        Assert.Contains("already exists", authResult.ErrorMessage);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "TestPassword123"
        };

        var user = new ApplicationUser 
        { 
            Id = "1",
            Email = loginRequest.Email,
            UserName = loginRequest.Email,
            FirstName = "Login",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(okResult.Value);
        
        Assert.True(authResult.Success);
        Assert.NotNull(authResult.Token);
        Assert.NotNull(authResult.User);
        Assert.Equal("login@example.com", authResult.User.Email);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(unauthorizedResult.Value);
        
        Assert.False(authResult.Success);
        Assert.Equal("Invalid email or password", authResult.ErrorMessage);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123"
        };

        var user = new ApplicationUser 
        { 
            Id = "1",
            Email = loginRequest.Email,
            UserName = loginRequest.Email,
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(false); // Wrong password

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(unauthorizedResult.Value);
        
        Assert.False(authResult.Success);
        Assert.Equal("Invalid email or password", authResult.ErrorMessage);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            Token = "invalid-token"
        };

        // Act
        var result = await _controller.RefreshToken(refreshRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(unauthorizedResult.Value);
        
        Assert.False(authResult.Success);
        Assert.Equal("Invalid token", authResult.ErrorMessage);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "weak@example.com",
            Password = "123", // Too weak
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var identityError = new IdentityError { Description = "Password too weak" };
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var authResult = Assert.IsType<AuthResult>(badRequestResult.Value);
        
        Assert.False(authResult.Success);
        Assert.Contains("Password too weak", authResult.ErrorMessage);
    }
}