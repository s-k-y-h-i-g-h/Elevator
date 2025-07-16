using Elevator.Shared.Models.Users;

namespace Elevator.Shared.Models.Discussion;

/// <summary>
/// Represents a vote (upvote/downvote) on a discussion or comment
/// </summary>
public class Vote
{
    /// <summary>
    /// Unique identifier for the vote
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the user who cast this vote
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the discussion this vote is for (optional)
    /// </summary>
    public int? DiscussionId { get; set; }

    /// <summary>
    /// ID of the comment this vote is for (optional)
    /// </summary>
    public int? CommentId { get; set; }

    /// <summary>
    /// True for upvote, false for downvote
    /// </summary>
    public bool IsUpvote { get; set; }

    /// <summary>
    /// When the vote was cast
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// User who cast this vote
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Discussion this vote is for (if any)
    /// </summary>
    public virtual Discussion? Discussion { get; set; }

    /// <summary>
    /// Comment this vote is for (if any)
    /// </summary>
    public virtual Comment? Comment { get; set; }
}