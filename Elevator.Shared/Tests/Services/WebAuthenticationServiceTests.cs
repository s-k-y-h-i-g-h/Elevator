using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.Implementations;
using Xunit;

namespace Elevator.Shared.Tests.Services;

public class WebAuthenticationServiceTests
{
    private Mock<UserManager<ApplicationUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());
        return mgr;
    }

    [Fact]
    public async Task LoginAsync_ReturnsSuccess_WhenCredentialsValid()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "test@test.com",
            UserName = "test@test.com"
        };

        mockUserManager.Setup(x => x.FindByEmailAsync("test@test.com"))
            .ReturnsAsync(user);
        mockUserManager.Setup(x => x.CheckPasswordAsync(user, "password"))
            .ReturnsAsync(true);
        mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = new WebAuthenticationService(mockUserManager.Object);

        // Act
        var result = await service.LoginAsync("test@test.com", "password");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("test@test.com", result.User.Email);
        Assert.True(service.IsAuthenticated);
        Assert.Equal("user1", service.CurrentUserId);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenUserNotFound()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("test@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var service = new WebAuthenticationService(mockUserManager.Object);

        // Act
        var result = await service.LoginAsync("test@test.com", "password");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.ErrorMessage);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenPasswordInvalid()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "test@test.com",
            UserName = "test@test.com"
        };

        mockUserManager.Setup(x => x.FindByEmailAsync("test@test.com"))
            .ReturnsAsync(user);
        mockUserManager.Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
            .ReturnsAsync(false);

        var service = new WebAuthenticationService(mockUserManager.Object);

        // Act
        var result = await service.LoginAsync("test@test.com", "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.ErrorMessage);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_WhenValid()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "password"))
            .ReturnsAsync(IdentityResult.Success);

        var service = new WebAuthenticationService(mockUserManager.Object);

        // Act
        var result = await service.RegisterAsync("test@test.com", "password", "John", "Doe");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("test@test.com", result.User.Email);
        Assert.Equal("John", result.User.FirstName);
        Assert.Equal("Doe", result.User.LastName);
        Assert.True(service.IsAuthenticated);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenCreationFails()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var errors = new[]
        {
            new IdentityError { Description = "Password too weak" }
        };
        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "weak"))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var service = new WebAuthenticationService(mockUserManager.Object);

        // Act
        var result = await service.RegisterAsync("test@test.com", "weak");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Password too weak", result.ErrorMessage);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public async Task LogoutAsync_ClearsCurrentUser()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var service = new WebAuthenticationService(mockUserManager.Object);

        // Set up authenticated state
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        mockUserManager.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync(user);
        mockUserManager.Setup(x => x.CheckPasswordAsync(user, "password")).ReturnsAsync(true);
        mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        
        await service.LoginAsync("test@test.com", "password");
        Assert.True(service.IsAuthenticated);

        // Act
        await service.LogoutAsync();

        // Assert
        Assert.False(service.IsAuthenticated);
        Assert.Null(service.CurrentUserId);
    }
}