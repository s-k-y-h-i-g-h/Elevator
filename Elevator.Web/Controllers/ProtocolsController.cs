using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProtocolsController : ControllerBase
{
    private readonly IProtocolService _protocolService;
    private readonly ILogger<ProtocolsController> _logger;

    public ProtocolsController(
        IProtocolService protocolService,
        ILogger<ProtocolsController> logger)
    {
        _protocolService = protocolService;
        _logger = logger;
    }

    /// <summary>
    /// Get all public protocols
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProtocolDto>>> GetProtocols()
    {
        try
        {
            var protocols = await _protocolService.GetProtocolsAsync();
            return Ok(protocols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving protocols");
            return StatusCode(500, new { message = "An error occurred while retrieving protocols" });
        }
    }

    /// <summary>
    /// Get protocols for the current user (requires authentication)
    /// </summary>
    [HttpGet("my-protocols")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<IEnumerable<ProtocolDto>>> GetMyProtocols()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var protocols = await _protocolService.GetUserProtocolsAsync(userId);
            return Ok(protocols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user protocols");
            return StatusCode(500, new { message = "An error occurred while retrieving your protocols" });
        }
    }

    /// <summary>
    /// Get protocols for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ProtocolDto>>> GetUserProtocols(string userId)
    {
        try
        {
            var protocols = await _protocolService.GetUserProtocolsAsync(userId);
            return Ok(protocols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving protocols for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user protocols" });
        }
    }

    /// <summary>
    /// Get protocol by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProtocolDetailDto>> GetProtocol(int id)
    {
        try
        {
            var protocol = await _protocolService.GetProtocolAsync(id);
            if (protocol == null)
            {
                return NotFound(new { message = $"Protocol with ID {id} not found" });
            }

            return Ok(protocol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving protocol {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the protocol" });
        }
    }

    /// <summary>
    /// Create a new protocol (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ProtocolDto>> CreateProtocol([FromBody] CreateProtocolDto createDto)
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

            var protocol = await _protocolService.CreateProtocolAsync(createDto);
            return CreatedAtAction(nameof(GetProtocol), new { id = protocol.Id }, protocol);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid protocol data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating protocol");
            return StatusCode(500, new { message = "An error occurred while creating the protocol" });
        }
    }

    /// <summary>
    /// Update an existing protocol (requires authentication and ownership)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ProtocolDto>> UpdateProtocol(int id, [FromBody] UpdateProtocolDto updateDto)
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

            // Check if the protocol exists and belongs to the user
            var existingProtocol = await _protocolService.GetProtocolAsync(id);
            if (existingProtocol == null)
            {
                return NotFound(new { message = $"Protocol with ID {id} not found" });
            }

            if (existingProtocol.UserId != userId)
            {
                return Forbid("You can only update your own protocols");
            }

            var protocol = await _protocolService.UpdateProtocolAsync(id, updateDto);
            return Ok(protocol);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid protocol data provided for ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Protocol {Id} not found for update", id);
            return NotFound(new { message = $"Protocol with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating protocol {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the protocol" });
        }
    }

    /// <summary>
    /// Delete a protocol (requires authentication and ownership)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult> DeleteProtocol(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Check if the protocol exists and belongs to the user
            var existingProtocol = await _protocolService.GetProtocolAsync(id);
            if (existingProtocol == null)
            {
                return NotFound(new { message = $"Protocol with ID {id} not found" });
            }

            if (existingProtocol.UserId != userId)
            {
                return Forbid("You can only delete your own protocols");
            }

            await _protocolService.DeleteProtocolAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Protocol {Id} not found for deletion", id);
            return NotFound(new { message = $"Protocol with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting protocol {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the protocol" });
        }
    }
}