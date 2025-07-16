using System.ComponentModel.DataAnnotations;

namespace Elevator.Shared.Models.Interventions;

/// <summary>
/// Abstract base class for all biohacking interventions
/// </summary>
public abstract class Intervention
{
    /// <summary>
    /// Unique identifier for the intervention
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the intervention
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the intervention
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the intervention was created in the system
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the intervention was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Discussions related to this intervention
    /// </summary>
    public virtual ICollection<Discussion.Discussion> Discussions { get; set; } = new List<Discussion.Discussion>();

    /// <summary>
    /// Ratings for this intervention
    /// </summary>
    public virtual ICollection<Ratings.Rating> Ratings { get; set; } = new List<Ratings.Rating>();

    /// <summary>
    /// Protocol associations for this intervention
    /// </summary>
    public virtual ICollection<Protocols.ProtocolIntervention> ProtocolInterventions { get; set; } = new List<Protocols.ProtocolIntervention>();
}