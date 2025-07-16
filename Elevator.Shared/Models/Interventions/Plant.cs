using System.ComponentModel.DataAnnotations;

namespace Elevator.Shared.Models.Interventions;

/// <summary>
/// Represents a plant-based intervention
/// </summary>
public class Plant : Substance
{
    /// <summary>
    /// Scientific name of the plant (e.g., "Panax ginseng")
    /// </summary>
    [StringLength(255)]
    public string? ScientificName { get; set; }

    /// <summary>
    /// Common names for the plant (e.g., "Ginseng, Asian Ginseng, Korean Ginseng")
    /// </summary>
    [StringLength(500)]
    public string? CommonNames { get; set; }

    /// <summary>
    /// Traditional uses of the plant
    /// </summary>
    public string? TraditionalUses { get; set; }

    /// <summary>
    /// Many-to-many relationship: compounds that are constituents of this plant
    /// </summary>
    public virtual ICollection<Compound> Constituents { get; set; } = new List<Compound>();
}