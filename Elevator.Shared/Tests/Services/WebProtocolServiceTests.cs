using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Protocols;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.Implementations;
using Elevator.Shared.Services.DTOs;
using Xunit;

namespace Elevator.Shared.Tests.Services;

public class WebProtocolServiceTests
{
    private ElevatorDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ElevatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ElevatorDbContext(options);
    }

    [Fact]
    public async Task GetProtocolsAsync_ReturnsAllProtocols()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebProtocolService(context);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        var protocol = new Protocol
        {
            Name = "Test Protocol",
            Description = "Test Description",
            UserId = user.Id,
            User = user
        };
        
        context.Users.Add(user);
        context.Protocols.Add(protocol);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetProtocolsAsync();

        // Assert
        Assert.Single(result);
        var protocolDto = result.First();
        Assert.Equal("Test Protocol", protocolDto.Name);
        Assert.Equal("user1", protocolDto.UserId);
    }

    [Fact]
    public async Task GetUserProtocolsAsync_ReturnsOnlyUserProtocols()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebProtocolService(context);
        
        var user1 = new ApplicationUser { Id = "user1", Email = "user1@test.com" };
        var user2 = new ApplicationUser { Id = "user2", Email = "user2@test.com" };
        
        var protocol1 = new Protocol { Name = "Protocol 1", UserId = "user1", User = user1 };
        var protocol2 = new Protocol { Name = "Protocol 2", UserId = "user2", User = user2 };
        
        context.Users.AddRange(user1, user2);
        context.Protocols.AddRange(protocol1, protocol2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserProtocolsAsync("user1");

        // Assert
        Assert.Single(result);
        Assert.Equal("Protocol 1", result.First().Name);
    }

    [Fact]
    public async Task GetProtocolAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebProtocolService(context);

        // Act
        var result = await service.GetProtocolAsync(999);

        // Assert
        Assert.Null(result);
    }
}