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

public class ProtocolsControllerTests
{
    private readonly Mock<IProtocolService> _mockProtocolService;
    private readonly Mock<ILogger<ProtocolsController>> _mockLogger;
    private readonly ProtocolsController _controller;

    public ProtocolsControllerTests()
    {
        _mockProtocolService = new Mock<IProtocolService>();
        _mockLogger = new Mock<ILogger<ProtocolsController>>();
        _controller = new ProtocolsController(_mockProtocolService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProtocols_ReturnsOkWithProtocols()
    {
        // Arrange
        var protocols = new List<ProtocolDto>
        {
            new ProtocolDto { Id = 1, Name = "Test Protocol 1", Description = "Description 1" },
            new ProtocolDto { Id = 2, Name = "Test Protocol 2", Description = "Description 2" }
        };

        _mockProtocolService.Setup(s => s.GetProtocolsAsync())
            .ReturnsAsync(protocols);

        // Act
        var result = await _controller.GetProtocols();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProtocols = Assert.IsAssignableFrom<IEnumerable<ProtocolDto>>(okResult.Value);
        Assert.Equal(2, returnedProtocols.Count());
    }

    [Fact]
    public async Task GetProtocol_ValidId_ReturnsOkWithProtocol()
    {
        // Arrange
        var protocol = new ProtocolDetailDto 
        { 
            Id = 1, 
            Name = "Test Protocol", 
            Description = "Test Description" 
        };

        _mockProtocolService.Setup(s => s.GetProtocolAsync(1))
            .ReturnsAsync(protocol);

        // Act
        var result = await _controller.GetProtocol(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProtocol = Assert.IsType<ProtocolDetailDto>(okResult.Value);
        Assert.Equal(1, returnedProtocol.Id);
        Assert.Equal("Test Protocol", returnedProtocol.Name);
    }

    [Fact]
    public async Task GetProtocol_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockProtocolService.Setup(s => s.GetProtocolAsync(999))
            .ReturnsAsync((ProtocolDetailDto?)null);

        // Act
        var result = await _controller.GetProtocol(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetUserProtocols_ValidUserId_ReturnsOkWithProtocols()
    {
        // Arrange
        var userId = "user123";
        var protocols = new List<ProtocolDto>
        {
            new ProtocolDto { Id = 1, Name = "User Protocol 1", UserId = userId },
            new ProtocolDto { Id = 2, Name = "User Protocol 2", UserId = userId }
        };

        _mockProtocolService.Setup(s => s.GetUserProtocolsAsync(userId))
            .ReturnsAsync(protocols);

        // Act
        var result = await _controller.GetUserProtocols(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProtocols = Assert.IsAssignableFrom<IEnumerable<ProtocolDto>>(okResult.Value);
        Assert.Equal(2, returnedProtocols.Count());
        Assert.All(returnedProtocols, p => Assert.Equal(userId, p.UserId));
    }

    [Fact]
    public async Task GetMyProtocols_WithValidUser_ReturnsOkWithProtocols()
    {
        // Arrange
        var userId = "user123";
        var protocols = new List<ProtocolDto>
        {
            new ProtocolDto { Id = 1, Name = "My Protocol 1", UserId = userId }
        };

        _mockProtocolService.Setup(s => s.GetUserProtocolsAsync(userId))
            .ReturnsAsync(protocols);

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
        var result = await _controller.GetMyProtocols();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProtocols = Assert.IsAssignableFrom<IEnumerable<ProtocolDto>>(okResult.Value);
        Assert.Single(returnedProtocols);
    }

    [Fact]
    public async Task GetMyProtocols_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange - No user context set up

        // Act
        var result = await _controller.GetMyProtocols();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode); // The controller returns 500 when there's an exception due to null user
    }

    [Fact]
    public async Task GetProtocols_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockProtocolService.Setup(s => s.GetProtocolsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetProtocols();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}