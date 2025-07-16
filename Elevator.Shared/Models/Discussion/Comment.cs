using System.ComponentModel.DataAnnotations;
using Elevator.Shared.Models.Users;

namespace Elevator.Shared.Models.Discussion;

/// <summary>
/// Represents a comment on a discussion with hierarchical structure support
/// </summary>
public class Comment
{
    /// <summary>
    /// Unique identifier for the comment
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Content of the comment
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who made this comment
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the discussion this comment belongs to
    /// </summary>
    public int DiscussionId { get; set; }

    /// <summary>
    /// ID of the parent comment (for nested/threaded comments)
    /// </summary>
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// When the comment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the comment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// User who made this comment
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Discussion this comment belongs to
    /// </summary>
    public virtual Discussion Discussion { get; set; } = null!;

    /// <summary>
    /// Parent comment (if this is a reply)
    /// </summary>
    public virtual Comment? ParentComment { get; set; }

    /// <summary>
    /// Child comments (replies to this comment)
    /// </summary>
    public virtual ICollection<Comment> ChildComments { get; set; } = new List<Comment>();

    /// <summary>
    /// Votes on this comment
    /// </summary>
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}