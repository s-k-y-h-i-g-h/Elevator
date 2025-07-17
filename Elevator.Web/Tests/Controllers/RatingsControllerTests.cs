using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;
using Elevator.Web.Controllers;

namespace Elevator.Web.Tests.Controllers;

public class RatingsControllerTests
{
    private readonly Mock<IRatingService> _mockRatingService;
    private readonly Mock<ILogger<RatingsController>> _mockLogger;
    private readonly RatingsController _controller;

    public RatingsControllerTests()
    {
        _mockRatingService = new Mock<IRatingService>();
        _mockLogger = new Mock<ILogger<RatingsController>>();
        _controller = new RatingsController(_mockRatingService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAverageRating_ValidInterventionId_ReturnsOkWithRating()
    {
        // Arrange
        var interventionId = 1;
        var expectedRating = 4.5m;

        _mockRatingService.Setup(s => s.GetAverageRatingAsync(interventionId, null))
            .ReturnsAsync(expectedRating);

        // Act
        var result = await _controller.GetAverageRating(interventionId: interventionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetAverageRating_ValidProtocolId_ReturnsOkWithRating()
    {
        // Arrange
        var protocolId = 1;
        var expectedRating = 3.8m;

        _mockRatingService.Setup(s => s.GetAverageRatingAsync(null, protocolId))
            .ReturnsAsync(expectedRating);

        // Act
        var result = await _controller.GetAverageRating(protocolId: protocolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetAverageRating_NoIdProvided_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAverageRating();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAverageRating_BothIdsProvided_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAverageRating(interventionId: 1, protocolId: 1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMyRating_ValidRequest_ReturnsOkWithRating()
    {
        // Arrange
        var userId = "user123";
        var interventionId = 1;
        var expectedRating = new RatingDto
        {
            Id = 1,
            UserId = userId,
            InterventionId = interventionId,
            Value = 4.0m
        };

        _mockRatingService.Setup(s => s.GetUserRatingAsync(userId, interventionId, null))
            .ReturnsAsync(expectedRating);

        // Set up the user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetMyRating(interventionId: interventionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRating = Assert.IsType<RatingDto>(okResult.Value);
        Assert.Equal(1, returnedRating.Id);
        Assert.Equal(4.0m, returnedRating.Value);
    }

    [Fact]
    public async Task GetMyRating_NoRatingFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "user123";
        var interventionId = 1;

        _mockRatingService.Setup(s => s.GetUserRatingAsync(userId, interventionId, null))
            .ReturnsAsync((RatingDto?)null);

        // Set up the user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetMyRating(interventionId: interventionId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOrUpdateRating_ValidRequest_ReturnsOkWithRating()
    {
        // Arrange
        var userId = "user123";
        var createRatingDto = new CreateRatingDto
        {
            InterventionId = 1,
            Value = 4.5m,
            Review = "Great intervention!"
        };

        var expectedRating = new RatingDto
        {
            Id = 1,
            UserId = userId,
            InterventionId = 1,
            Value = 4.5m,
            Review = "Great intervention!"
        };

        _mockRatingService.Setup(s => s.CreateOrUpdateRatingAsync(It.IsAny<CreateRatingDto>()))
            .ReturnsAsync(expectedRating);

        // Set up the user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.CreateOrUpdateRating(createRatingDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRating = Assert.IsType<RatingDto>(okResult.Value);
        Assert.Equal(4.5m, returnedRating.Value);
        Assert.Equal("Great intervention!", returnedRating.Review);
    }

    [Fact]
    public async Task CreateOrUpdateRating_NoIdProvided_ReturnsBadRequest()
    {
        // Arrange
        var createRatingDto = new CreateRatingDto
        {
            // Neither InterventionId nor ProtocolId specified
            Value = 4.5m
        };

        // Act
        var result = await _controller.CreateOrUpdateRating(createRatingDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOrUpdateRating_BothIdsProvided_ReturnsBadRequest()
    {
        // Arrange
        var createRatingDto = new CreateRatingDto
        {
            InterventionId = 1,
            ProtocolId = 1, // Both specified - should be invalid
            Value = 4.5m
        };

        // Act
        var result = await _controller.CreateOrUpdateRating(createRatingDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOrUpdateRating_InvalidRatingValue_ReturnsBadRequest()
    {
        // Arrange
        var createRatingDto = new CreateRatingDto
        {
            InterventionId = 1,
            Value = 6.0m // Invalid - should be between 0 and 5
        };

        // Act
        var result = await _controller.CreateOrUpdateRating(createRatingDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetInterventionAverageRating_ValidId_ReturnsOkWithRating()
    {
        // Arrange
        var interventionId = 1;
        var expectedRating = 4.2m;

        _mockRatingService.Setup(s => s.GetAverageRatingAsync(interventionId, null))
            .ReturnsAsync(expectedRating);

        // Act
        var result = await _controller.GetInterventionAverageRating(interventionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetProtocolAverageRating_ValidId_ReturnsOkWithRating()
    {
        // Arrange
        var protocolId = 1;
        var expectedRating = 3.9m;

        _mockRatingService.Setup(s => s.GetAverageRatingAsync(null, protocolId))
            .ReturnsAsync(expectedRating);

        // Act
        var result = await _controller.GetProtocolAverageRating(protocolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetAverageRating_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockRatingService.Setup(s => s.GetAverageRatingAsync(1, null))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAverageRating(interventionId: 1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}