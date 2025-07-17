using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;
    private readonly ILogger<DiscussionsController> _logger;

    public DiscussionsController(
        IDiscussionService discussionService,
        ILogger<DiscussionsController> logger)
    {
        _discussionService = discussionService;
        _logger = logger;
    }

    /// <summary>
    /// Get discussions, optionally filtered by intervention or protocol
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiscussionDto>>> GetDiscussions(
        [FromQuery] int? interventionId = null,
        [FromQuery] int? protocolId = null)
    {
        try
        {
            var discussions = await _discussionService.GetDiscussionsAsync(interventionId, protocolId);
            return Ok(discussions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discussions with interventionId: {InterventionId}, protocolId: {ProtocolId}", 
                interventionId, protocolId);
            return StatusCode(500, new { message = "An error occurred while retrieving discussions" });
        }
    }

    /// <summary>
    /// Get discussion by ID with comments
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DiscussionDetailDto>> GetDiscussion(int id)
    {
        try
        {
            var discussion = await _discussionService.GetDiscussionAsync(id);
            if (discussion == null)
            {
                return NotFound(new { message = $"Discussion with ID {id} not found" });
            }

            return Ok(discussion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discussion {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the discussion" });
        }
    }

    /// <summary>
    /// Create a new discussion (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<DiscussionDto>> CreateDiscussion([FromBody] CreateDiscussionDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Set the user ID from the authenticated user
            createDto.UserId = userId;

            var discussion = await _discussionService.CreateDiscussionAsync(createDto);
            return CreatedAtAction(nameof(GetDiscussion), new { id = discussion.Id }, discussion);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid discussion data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discussion");
            return StatusCode(500, new { message = "An error occurred while creating the discussion" });
        }
    }

    /// <summary>
    /// Add a comment to a discussion (requires authentication)
    /// </summary>
    [HttpPost("comments")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Set the user ID from the authenticated user
            createDto.UserId = userId;

            var comment = await _discussionService.AddCommentAsync(createDto);
            return CreatedAtAction(nameof(GetDiscussion), new { id = createDto.DiscussionId }, comment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid comment data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discussion {DiscussionId} not found for comment", createDto.DiscussionId);
            return NotFound(new { message = $"Discussion with ID {createDto.DiscussionId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to discussion {DiscussionId}", createDto.DiscussionId);
            return StatusCode(500, new { message = "An error occurred while adding the comment" });
        }
    }

    /// <summary>
    /// Vote on a discussion or comment (requires authentication)
    /// </summary>
    [HttpPost("votes")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<VoteDto>> Vote([FromBody] CreateVoteDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Validate that either discussionId or commentId is provided, but not both
            if (createDto.DiscussionId.HasValue && createDto.CommentId.HasValue)
            {
                return BadRequest(new { message = "Cannot vote on both discussion and comment simultaneously" });
            }

            if (!createDto.DiscussionId.HasValue && !createDto.CommentId.HasValue)
            {
                return BadRequest(new { message = "Must specify either discussionId or commentId" });
            }

            // Set the user ID from the authenticated user
            createDto.UserId = userId;

            var vote = await _discussionService.VoteAsync(createDto);
            return Ok(vote);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid vote data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Discussion or comment not found for vote");
            return NotFound(new { message = "Discussion or comment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing vote");
            return StatusCode(500, new { message = "An error occurred while processing the vote" });
        }
    }

    /// <summary>
    /// Get discussions for a specific intervention
    /// </summary>
    [HttpGet("intervention/{interventionId}")]
    public async Task<ActionResult<IEnumerable<DiscussionDto>>> GetInterventionDiscussions(int interventionId)
    {
        try
        {
            var discussions = await _discussionService.GetDiscussionsAsync(interventionId: interventionId);
            return Ok(discussions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discussions for intervention {InterventionId}", interventionId);
            return StatusCode(500, new { message = "An error occurred while retrieving intervention discussions" });
        }
    }

    /// <summary>
    /// Get discussions for a specific protocol
    /// </summary>
    [HttpGet("protocol/{protocolId}")]
    public async Task<ActionResult<IEnumerable<DiscussionDto>>> GetProtocolDiscussions(int protocolId)
    {
        try
        {
            var discussions = await _discussionService.GetDiscussionsAsync(protocolId: protocolId);
            return Ok(discussions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discussions for protocol {ProtocolId}", protocolId);
            return StatusCode(500, new { message = "An error occurred while retrieving protocol discussions" });
        }
    }
}