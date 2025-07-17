using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterventionsController : ControllerBase
{
    private readonly IInterventionService _interventionService;
    private readonly ILogger<InterventionsController> _logger;

    public InterventionsController(
        IInterventionService interventionService,
        ILogger<InterventionsController> logger)
    {
        _interventionService = interventionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all interventions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterventionDto>>> GetInterventions()
    {
        try
        {
            var interventions = await _interventionService.GetInterventionsAsync();
            return Ok(interventions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interventions");
            return StatusCode(500, new { message = "An error occurred while retrieving interventions" });
        }
    }

    /// <summary>
    /// Get intervention by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InterventionDetailDto>> GetIntervention(int id)
    {
        try
        {
            var intervention = await _interventionService.GetInterventionAsync(id);
            if (intervention == null)
            {
                return NotFound(new { message = $"Intervention with ID {id} not found" });
            }

            return Ok(intervention);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving intervention {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the intervention" });
        }
    }

    /// <summary>
    /// Create a new intervention (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<InterventionDto>> CreateIntervention([FromBody] CreateInterventionDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var intervention = await _interventionService.CreateInterventionAsync(createDto);
            return CreatedAtAction(nameof(GetIntervention), new { id = intervention.Id }, intervention);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid intervention data provided");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating intervention");
            return StatusCode(500, new { message = "An error occurred while creating the intervention" });
        }
    }

    /// <summary>
    /// Update an existing intervention (requires authentication)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<InterventionDto>> UpdateIntervention(int id, [FromBody] UpdateInterventionDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var intervention = await _interventionService.UpdateInterventionAsync(id, updateDto);
            return Ok(intervention);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid intervention data provided for ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Intervention {Id} not found for update", id);
            return NotFound(new { message = $"Intervention with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating intervention {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the intervention" });
        }
    }

    /// <summary>
    /// Delete an intervention (requires authentication)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult> DeleteIntervention(int id)
    {
        try
        {
            await _interventionService.DeleteInterventionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Intervention {Id} not found for deletion", id);
            return NotFound(new { message = $"Intervention with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting intervention {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the intervention" });
        }
    }

    /// <summary>
    /// Get AI information for an intervention
    /// </summary>
    [HttpGet("{name}/ai-info")]
    public async Task<ActionResult<string>> GetAiInformation(string name)
    {
        try
        {
            var aiInfo = await _interventionService.GetAiInformationAsync(name);
            if (string.IsNullOrEmpty(aiInfo))
            {
                return NotFound(new { message = $"No AI information found for intervention '{name}'" });
            }

            return Ok(new { aiInformation = aiInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI information for intervention '{Name}'", name);
            return StatusCode(500, new { message = "An error occurred while retrieving AI information" });
        }
    }
}