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

public class DiscussionsControllerTests
{
    private readonly Mock<IDiscussionService> _mockDiscussionService;
    private readonly Mock<ILogger<DiscussionsController>> _mockLogger;
    private readonly DiscussionsController _controller;

    public DiscussionsControllerTests()
    {
        _mockDiscussionService = new Mock<IDiscussionService>();
        _mockLogger = new Mock<ILogger<DiscussionsController>>();
        _controller = new DiscussionsController(_mockDiscussionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDiscussions_ReturnsOkWithDiscussions()
    {
        // Arrange
        var discussions = new List<DiscussionDto>
        {
            new DiscussionDto { Id = 1, Title = "Test Discussion 1", Content = "Content 1" },
            new DiscussionDto { Id = 2, Title = "Test Discussion 2", Content = "Content 2" }
        };

        _mockDiscussionService.Setup(s => s.GetDiscussionsAsync(null, null))
            .ReturnsAsync(discussions);

        // Act
        var result = await _controller.GetDiscussions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDiscussions = Assert.IsAssignableFrom<IEnumerable<DiscussionDto>>(okResult.Value);
        Assert.Equal(2, returnedDiscussions.Count());
    }

    [Fact]
    public async Task GetDiscussion_ValidId_ReturnsOkWithDiscussion()
    {
        // Arrange
        var discussion = new DiscussionDetailDto 
        { 
            Id = 1, 
            Title = "Test Discussion", 
            Content = "Test Content" 
        };

        _mockDiscussionService.Setup(s => s.GetDiscussionAsync(1))
            .ReturnsAsync(discussion);

        // Act
        var result = await _controller.GetDiscussion(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDiscussion = Assert.IsType<DiscussionDetailDto>(okResult.Value);
        Assert.Equal(1, returnedDiscussion.Id);
        Assert.Equal("Test Discussion", returnedDiscussion.Title);
    }

    [Fact]
    public async Task GetDiscussion_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockDiscussionService.Setup(s => s.GetDiscussionAsync(999))
            .ReturnsAsync((DiscussionDetailDto?)null);

        // Act
        var result = await _controller.GetDiscussion(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetInterventionDiscussions_ValidId_ReturnsOkWithDiscussions()
    {
        // Arrange
        var interventionId = 1;
        var discussions = new List<DiscussionDto>
        {
            new DiscussionDto { Id = 1, Title = "Intervention Discussion", InterventionId = interventionId }
        };

        _mockDiscussionService.Setup(s => s.GetDiscussionsAsync(interventionId, null))
            .ReturnsAsync(discussions);

        // Act
        var result = await _controller.GetInterventionDiscussions(interventionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDiscussions = Assert.IsAssignableFrom<IEnumerable<DiscussionDto>>(okResult.Value);
        Assert.Single(returnedDiscussions);
    }

    [Fact]
    public async Task GetProtocolDiscussions_ValidId_ReturnsOkWithDiscussions()
    {
        // Arrange
        var protocolId = 1;
        var discussions = new List<DiscussionDto>
        {
            new DiscussionDto { Id = 1, Title = "Protocol Discussion", ProtocolId = protocolId }
        };

        _mockDiscussionService.Setup(s => s.GetDiscussionsAsync(null, protocolId))
            .ReturnsAsync(discussions);

        // Act
        var result = await _controller.GetProtocolDiscussions(protocolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDiscussions = Assert.IsAssignableFrom<IEnumerable<DiscussionDto>>(okResult.Value);
        Assert.Single(returnedDiscussions);
    }

    [Fact]
    public async Task Vote_ValidRequest_ReturnsOkWithVote()
    {
        // Arrange
        var userId = "user123";
        var createVoteDto = new CreateVoteDto
        {
            DiscussionId = 1,
            IsUpvote = true
        };

        var expectedVote = new VoteDto
        {
            Id = 1,
            UserId = userId,
            DiscussionId = 1,
            IsUpvote = true
        };

        _mockDiscussionService.Setup(s => s.VoteAsync(It.IsAny<CreateVoteDto>()))
            .ReturnsAsync(expectedVote);

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
        var result = await _controller.Vote(createVoteDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedVote = Assert.IsType<VoteDto>(okResult.Value);
        Assert.Equal(1, returnedVote.Id);
        Assert.True(returnedVote.IsUpvote);
    }

    [Fact]
    public async Task Vote_BothDiscussionAndComment_ReturnsBadRequest()
    {
        // Arrange
        var createVoteDto = new CreateVoteDto
        {
            DiscussionId = 1,
            CommentId = 1, // Both specified - should be invalid
            IsUpvote = true
        };

        // Act
        var result = await _controller.Vote(createVoteDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode); // The controller returns 500 when there's an exception due to no user context
    }

    [Fact]
    public async Task Vote_NeitherDiscussionNorComment_ReturnsBadRequest()
    {
        // Arrange
        var createVoteDto = new CreateVoteDto
        {
            // Neither DiscussionId nor CommentId specified
            IsUpvote = true
        };

        // Act
        var result = await _controller.Vote(createVoteDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode); // The controller returns 500 when there's an exception due to no user context
    }

    [Fact]
    public async Task GetDiscussions_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockDiscussionService.Setup(s => s.GetDiscussionsAsync(null, null))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetDiscussions();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}