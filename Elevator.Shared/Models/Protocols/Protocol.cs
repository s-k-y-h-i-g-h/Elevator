using System.ComponentModel.DataAnnotations;
using Elevator.Shared.Models.Users;

namespace Elevator.Shared.Models.Protocols;

/// <summary>
/// Represents a biohacking protocol created by a user
/// </summary>
public class Protocol
{
    /// <summary>
    /// Unique identifier for the protocol
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the protocol
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the protocol
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID of the user who created this protocol
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the protocol is publicly visible
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// When the protocol was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the protocol was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// User who created this protocol
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Interventions included in this protocol
    /// </summary>
    public virtual ICollection<ProtocolIntervention> ProtocolInterventions { get; set; } = new List<ProtocolIntervention>();

    /// <summary>
    /// Discussions about this protocol
    /// </summary>
    public virtual ICollection<Discussion.Discussion> Discussions { get; set; } = new List<Discussion.Discussion>();

    /// <summary>
    /// Ratings for this protocol
    /// </summary>
    public virtual ICollection<Ratings.Rating> Ratings { get; set; } = new List<Ratings.Rating>();
}