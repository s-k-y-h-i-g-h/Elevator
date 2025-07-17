using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(
        IRatingService ratingService,
        ILogger<RatingsController> logger)
    {
        _ratingService = ratingService;
        _logger = logger;
    }

    /// <summary>
    /// Get average rating for an intervention or protocol
    /// </summary>
    [HttpGet("average")]
    public async Task<ActionResult<decimal>> GetAverageRating(
        [FromQuery] int? interventionId = null,
        [FromQuery] int? protocolId = null)
    {
        try
        {
            if (!interventionId.HasValue && !protocolId.HasValue)
            {
                return BadRequest(new { message = "Must specify either interventionId or protocolId" });
            }

            if (interventionId.HasValue && protocolId.HasValue)
            {
                return BadRequest(new { message = "Cannot specify both interventionId and protocolId" });
            }

            var averageRating = await _ratingService.GetAverageRatingAsync(interventionId, protocolId);
            return Ok(new { averageRating });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving average rating for interventionId: {InterventionId}, protocolId: {ProtocolId}", 
                interventionId, protocolId);
            return StatusCode(500, new { message = "An error occurred while retrieving the average rating" });
        }
    }

    /// <summary>
    /// Get current user's rating for an intervention or protocol (requires authentication)
    /// </summary>
    [HttpGet("my-rating")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<RatingDto>> GetMyRating(
        [FromQuery] int? interventionId = null,
        [FromQuery] int? protocolId = null)
    {
        try
        {
            if (!interventionId.HasValue && !protocolId.HasValue)
            {
                return BadRequest(new { message = "Must specify either interventionId or protocolId" });
            }

            if (interventionId.HasValue && protocolId.HasValue)
            {
                return BadRequest(new { message = "Cannot specify both interventionId and protocolId" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var rating = await _ratingService.GetUserRatingAsync(userId, interventionId, protocolId);
            if (rating == null)
            {
                return NotFound(new { message = "No rating found for this user" });
            }

            return Ok(rating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user rating for interventionId: {InterventionId}, protocolId: {ProtocolId}", 
                interventionId, protocolId);
            return StatusCode(500, new { message = "An error occurred while retrieving your rating" });
        }
    }

    /// <summary>
    /// Create or update a rating (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<RatingDto>> CreateOrUpdateRating([FromBody] CreateRatingDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that either interventionId or protocolId is provided, but not both
            if (!createDto.InterventionId.HasValue && !createDto.ProtocolId.HasValue)
            {
                return BadRequest(new { message = "Must specify either interventionId or protocolId" });
            }

            if (createDto.InterventionId.HasValue && createDto.ProtocolId.HasValue)
            {
                return BadRequest(new { message = "Cannot rate both intervention and protocol simultaneously" });
            }

            // Validate rating value
            if (createDto.Value < 0 || createDto.Value > 5)
            {
                return BadRequest(new { message = "Rating value must be between 0 and 5" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Set the user ID from the authenticated user
            createDto.UserId = userId;

            var rating = await _ratingService.CreateOrUpdateRatingAsync(createDto);
            return Ok(rating);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid rating data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Intervention or protocol not found for rating");
            return NotFound(new { message = "Intervention or protocol not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or updating rating");
            return StatusCode(500, new { message = "An error occurred while processing the rating" });
        }
    }

    /// <summary>
    /// Delete a rating (requires authentication and ownership)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult> DeleteRating(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Note: The service should validate that the rating belongs to the current user
            await _ratingService.DeleteRatingAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Rating {Id} not found for deletion", id);
            return NotFound(new { message = $"Rating with ID {id} not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User attempted to delete rating {Id} they don't own", id);
            return Forbid("You can only delete your own ratings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rating {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the rating" });
        }
    }

    /// <summary>
    /// Get average rating for a specific intervention
    /// </summary>
    [HttpGet("intervention/{interventionId}/average")]
    public async Task<ActionResult<decimal>> GetInterventionAverageRating(int interventionId)
    {
        try
        {
            var averageRating = await _ratingService.GetAverageRatingAsync(interventionId: interventionId);
            return Ok(new { averageRating });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving average rating for intervention {InterventionId}", interventionId);
            return StatusCode(500, new { message = "An error occurred while retrieving the average rating" });
        }
    }

    /// <summary>
    /// Get average rating for a specific protocol
    /// </summary>
    [HttpGet("protocol/{protocolId}/average")]
    public async Task<ActionResult<decimal>> GetProtocolAverageRating(int protocolId)
    {
        try
        {
            var averageRating = await _ratingService.GetAverageRatingAsync(protocolId: protocolId);
            return Ok(new { averageRating });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving average rating for protocol {ProtocolId}", protocolId);
            return StatusCode(500, new { message = "An error occurred while retrieving the average rating" });
        }
    }
}