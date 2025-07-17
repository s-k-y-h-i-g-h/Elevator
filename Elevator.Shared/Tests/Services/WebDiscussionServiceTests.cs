using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Discussion;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Services.Implementations;
using Elevator.Shared.Services.DTOs;
using Xunit;

namespace Elevator.Shared.Tests.Services;

public class WebDiscussionServiceTests
{
    private ElevatorDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ElevatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ElevatorDbContext(options);
    }

    [Fact]
    public async Task GetDiscussionsAsync_ReturnsAllDiscussions_WhenNoFilters()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebDiscussionService(context);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        var intervention = new Compound { Name = "Test Compound" };
        
        var discussion = new Discussion
        {
            Title = "Test Discussion",
            Content = "Test Content",
            UserId = user.Id,
            User = user,
            InterventionId = 1,
            Intervention = intervention
        };
        
        context.Users.Add(user);
        context.Interventions.Add(intervention);
        context.Discussions.Add(discussion);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDiscussionsAsync();

        // Assert
        Assert.Single(result);
        var discussionDto = result.First();
        Assert.Equal("Test Discussion", discussionDto.Title);
        Assert.Equal("user1", discussionDto.UserId);
    }

    [Fact]
    public async Task GetDiscussionsAsync_FiltersBy_InterventionId()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebDiscussionService(context);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        var intervention1 = new Compound { Name = "Compound 1" };
        var intervention2 = new Compound { Name = "Compound 2" };
        
        var discussion1 = new Discussion { Title = "Discussion 1", UserId = "user1", User = user, InterventionId = 1, Intervention = intervention1 };
        var discussion2 = new Discussion { Title = "Discussion 2", UserId = "user1", User = user, InterventionId = 2, Intervention = intervention2 };
        
        context.Users.Add(user);
        context.Interventions.AddRange(intervention1, intervention2);
        context.Discussions.AddRange(discussion1, discussion2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDiscussionsAsync(interventionId: 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Discussion 1", result.First().Title);
    }

    [Fact]
    public async Task GetDiscussionAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebDiscussionService(context);

        // Act
        var result = await service.GetDiscussionAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateDiscussionAsync_CreatesDiscussion_Successfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebDiscussionService(context);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        var intervention = new Compound { Name = "Test Compound" };
        
        context.Users.Add(user);
        context.Interventions.Add(intervention);
        await context.SaveChangesAsync();
        
        var createDto = new CreateDiscussionDto
        {
            Title = "New Discussion",
            Content = "Discussion content",
            InterventionId = 1,
            UserId = "user1"
        };

        // Act
        var result = await service.CreateDiscussionAsync(createDto);

        // Assert
        Assert.Equal("New Discussion", result.Title);
        Assert.Equal("Discussion content", result.Content);
        Assert.Equal(1, result.InterventionId);
        
        // Verify in database
        var saved = await context.Discussions.FirstAsync();
        Assert.Equal("New Discussion", saved.Title);
    }

    [Fact]
    public async Task AddCommentAsync_CreatesComment_Successfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebDiscussionService(context);
        
        var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
        var discussion = new Discussion { Title = "Test", UserId = "user1", User = user };
        
        context.Users.Add(user);
        context.Discussions.Add(discussion);
        await context.SaveChangesAsync();
        
        var createDto = new CreateCommentDto
        {
            Content = "Test comment",
            DiscussionId = 1,
            UserId = "user1"
        };

        // Act
        var result = await service.AddCommentAsync(createDto);

        // Assert
        Assert.Equal("Test comment", result.Content);
        Assert.Equal(1, result.DiscussionId);
        
        // Verify in database
        var saved = await context.Comments.FirstAsync();
        Assert.Equal("Test comment", saved.Content);
    }
}