using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;
using Elevator.Web.Controllers;

namespace Elevator.Web.Tests.Controllers;

public class InterventionsControllerTests
{
    private readonly Mock<IInterventionService> _mockInterventionService;
    private readonly Mock<ILogger<InterventionsController>> _mockLogger;
    private readonly InterventionsController _controller;

    public InterventionsControllerTests()
    {
        _mockInterventionService = new Mock<IInterventionService>();
        _mockLogger = new Mock<ILogger<InterventionsController>>();
        _controller = new InterventionsController(_mockInterventionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetInterventions_ReturnsOkWithInterventions()
    {
        // Arrange
        var interventions = new List<InterventionDto>
        {
            new InterventionDto { Id = 1, Name = "Test Intervention 1", Description = "Description 1" },
            new InterventionDto { Id = 2, Name = "Test Intervention 2", Description = "Description 2" }
        };

        _mockInterventionService.Setup(s => s.GetInterventionsAsync())
            .ReturnsAsync(interventions);

        // Act
        var result = await _controller.GetInterventions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedInterventions = Assert.IsAssignableFrom<IEnumerable<InterventionDto>>(okResult.Value);
        Assert.Equal(2, returnedInterventions.Count());
    }

    [Fact]
    public async Task GetIntervention_ValidId_ReturnsOkWithIntervention()
    {
        // Arrange
        var intervention = new InterventionDetailDto 
        { 
            Id = 1, 
            Name = "Test Intervention", 
            Description = "Test Description" 
        };

        _mockInterventionService.Setup(s => s.GetInterventionAsync(1))
            .ReturnsAsync(intervention);

        // Act
        var result = await _controller.GetIntervention(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedIntervention = Assert.IsType<InterventionDetailDto>(okResult.Value);
        Assert.Equal(1, returnedIntervention.Id);
        Assert.Equal("Test Intervention", returnedIntervention.Name);
    }

    [Fact]
    public async Task GetIntervention_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockInterventionService.Setup(s => s.GetInterventionAsync(999))
            .ReturnsAsync((InterventionDetailDto?)null);

        // Act
        var result = await _controller.GetIntervention(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAiInformation_ValidName_ReturnsOkWithInfo()
    {
        // Arrange
        var aiInfo = "This is AI-generated information about the intervention.";
        _mockInterventionService.Setup(s => s.GetAiInformationAsync("test-intervention"))
            .ReturnsAsync(aiInfo);

        // Act
        var result = await _controller.GetAiInformation("test-intervention");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetAiInformation_NoInfoFound_ReturnsNotFound()
    {
        // Arrange
        _mockInterventionService.Setup(s => s.GetAiInformationAsync("unknown-intervention"))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.GetAiInformation("unknown-intervention");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetInterventions_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockInterventionService.Setup(s => s.GetInterventionsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetInterventions();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}