using System.ComponentModel.DataAnnotations;

namespace Elevator.Shared.Models.Interventions;

/// <summary>
/// Abstract base class for substance-based interventions (compounds, plants, etc.)
/// </summary>
public abstract class Substance : Intervention
{
    /// <summary>
    /// Duration information for the substance (e.g., "30 minutes", "2-4 hours")
    /// </summary>
    [StringLength(100)]
    public string? Duration { get; set; }

    /// <summary>
    /// Dose range information for the substance (e.g., "10-20mg", "1-2 capsules")
    /// </summary>
    [StringLength(100)]
    public string? DoseRange { get; set; }
}