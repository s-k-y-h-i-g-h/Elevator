using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.Implementations;
using Elevator.Shared.Services.DTOs;
using Xunit;

namespace Elevator.Shared.Tests.Services;

public class WebUserServiceTests
{
    private ElevatorDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ElevatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ElevatorDbContext(options);
    }

    [Fact]
    public async Task GetUserAsync_ReturnsUser_WhenExists()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebUserService(context);
        
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserAsync("user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.Id);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task GetUserAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebUserService(context);

        // Act
        var result = await service.GetUserAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUser_WhenExists()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebUserService(context);
        
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.Id);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebUserService(context);

        // Act
        var result = await service.GetUserByEmailAsync("nonexistent@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUser_Successfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebUserService(context);
        
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var updateDto = new UpdateUserDto
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var result = await service.UpdateUserAsync("user1", updateDto);

        // Assert
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        
        // Verify in database
        var updated = await context.Users.FindAsync("user1");
        Assert.Equal("Jane", updated!.FirstName);
        Assert.Equal("Smith", updated.LastName);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebUserService(context);
        
        var updateDto = new UpdateUserDto
        {
            FirstName = "Jane"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateUserAsync("nonexistent", updateDto));
    }
}