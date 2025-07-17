using System.ComponentModel.DataAnnotations;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Models.Protocols;

namespace Elevator.Shared.Models.Ratings;

/// <summary>
/// Represents a user rating for an intervention or protocol
/// </summary>
public class Rating
{
    /// <summary>
    /// Unique identifier for the rating
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the user who submitted this rating
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the intervention being rated (optional)
    /// </summary>
    public int? InterventionId { get; set; }

    /// <summary>
    /// ID of the protocol being rated (optional)
    /// </summary>
    public int? ProtocolId { get; set; }

    /// <summary>
    /// Rating value from 0.0 to 5.0 with 0.5 increments
    /// </summary>
    [Range(0.0, 5.0)]
    public decimal Value { get; set; }

    /// <summary>
    /// When the rating was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the rating was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional review text for the rating
    /// </summary>
    public string? Review { get; set; }

    // Navigation properties
    /// <summary>
    /// User who submitted this rating
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Intervention being rated (if any)
    /// </summary>
    public virtual Intervention? Intervention { get; set; }

    /// <summary>
    /// Protocol being rated (if any)
    /// </summary>
    public virtual Protocol? Protocol { get; set; }
}