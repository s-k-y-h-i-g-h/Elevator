using System.ComponentModel.DataAnnotations;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Models.Protocols;

namespace Elevator.Shared.Models.Discussion;

/// <summary>
/// Represents a discussion thread about an intervention or protocol
/// </summary>
public class Discussion
{
    /// <summary>
    /// Unique identifier for the discussion
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Title of the discussion
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content of the discussion
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who created this discussion
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the intervention this discussion is about (optional)
    /// </summary>
    public int? InterventionId { get; set; }

    /// <summary>
    /// ID of the protocol this discussion is about (optional)
    /// </summary>
    public int? ProtocolId { get; set; }

    /// <summary>
    /// When the discussion was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the discussion was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// User who created this discussion
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Intervention this discussion is about (if any)
    /// </summary>
    public virtual Intervention? Intervention { get; set; }

    /// <summary>
    /// Protocol this discussion is about (if any)
    /// </summary>
    public virtual Protocol? Protocol { get; set; }

    /// <summary>
    /// Comments on this discussion
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Votes on this discussion
    /// </summary>
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}