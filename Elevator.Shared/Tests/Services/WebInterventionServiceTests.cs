using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Services.Implementations;
using Elevator.Shared.Services.DTOs;
using Xunit;

namespace Elevator.Shared.Tests.Services;

public class WebInterventionServiceTests
{
    private ElevatorDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ElevatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ElevatorDbContext(options);
    }

    [Fact]
    public async Task GetInterventionsAsync_ReturnsAllInterventions()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebInterventionService(context);
        
        var compound = new Compound
        {
            Name = "Test Compound",
            Description = "Test Description",
            Duration = "30 days",
            DoseRange = "100-200mg"
        };
        
        context.Interventions.Add(compound);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetInterventionsAsync();

        // Assert
        Assert.Single(result);
        var intervention = result.First();
        Assert.Equal("Test Compound", intervention.Name);
        Assert.Equal("Compound", intervention.InterventionType);
    }

    [Fact]
    public async Task CreateInterventionAsync_CreatesCompound_Successfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebInterventionService(context);
        
        var createDto = new CreateInterventionDto
        {
            Name = "New Compound",
            Description = "New Description",
            InterventionType = "Compound",
            Duration = "60 days",
            DoseRange = "50-100mg"
        };

        // Act
        var result = await service.CreateInterventionAsync(createDto);

        // Assert
        Assert.Equal("New Compound", result.Name);
        Assert.Equal("Compound", result.InterventionType);
        Assert.Equal("60 days", result.Duration);
        
        // Verify in database
        var saved = await context.Interventions.FirstAsync();
        Assert.Equal("New Compound", saved.Name);
    }

    [Fact]
    public async Task CreateInterventionAsync_CreatesPlant_Successfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebInterventionService(context);
        
        var createDto = new CreateInterventionDto
        {
            Name = "Test Plant",
            Description = "Plant Description",
            InterventionType = "Plant",
            ScientificName = "Plantus testicus",
            CommonNames = "Test Plant, Testing Plant",
            TraditionalUses = "Traditional healing"
        };

        // Act
        var result = await service.CreateInterventionAsync(createDto);

        // Assert
        Assert.Equal("Test Plant", result.Name);
        Assert.Equal("Plant", result.InterventionType);
        Assert.Equal("Plantus testicus", result.ScientificName);
        
        // Verify in database
        var saved = await context.Interventions.OfType<Plant>().FirstAsync();
        Assert.Equal("Plantus testicus", saved.ScientificName);
    }

    [Fact]
    public async Task GetInterventionAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebInterventionService(context);

        // Act
        var result = await service.GetInterventionAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteInterventionAsync_ThrowsException_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new WebInterventionService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteInterventionAsync(999));
    }
}