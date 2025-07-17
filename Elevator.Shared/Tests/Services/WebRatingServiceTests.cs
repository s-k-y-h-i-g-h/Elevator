using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Ratings;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Services.Implementations;
using Xunit;

namespace Elevator.Shared.Tests.Services;

public class WebRatingServiceTests
{
    private ElevatorDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ElevatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ElevatorDbContext(options);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ReturnsCorrectAverage()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebRatingService(context);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        var intervention = new Compound { Name = "Test Compound" };
        
        var rating1 = new Rating { UserId = "user1", InterventionId = 1, Value = 4.0m, User = user, Intervention = intervention };
        var rating2 = new Rating { UserId = "user1", InterventionId = 1, Value = 5.0m, User = user, Intervention = intervention };
        
        context.Users.Add(user);
        context.Interventions.Add(intervention);
        context.Ratings.AddRange(rating1, rating2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAverageRatingAsync(interventionId: 1);

        // Assert
        Assert.Equal(4.5m, result);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ReturnsZero_WhenNoRatings()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebRatingService(context);

        // Act
        var result = await service.GetAverageRatingAsync(interventionId: 999);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task GetUserRatingAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebRatingService(context);

        // Act
        var result = await service.GetUserRatingAsync("user1", interventionId: 999);

        // Assert
        Assert.Null(result);
    }
}