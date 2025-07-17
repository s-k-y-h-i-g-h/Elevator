namespace Elevator.Shared.Services.DTOs;

public class VoteDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? DiscussionId { get; set; }
    public int? CommentId { get; set; }
    public bool IsUpvote { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVoteDto
{
    public int? DiscussionId { get; set; }
    public int? CommentId { get; set; }
    public bool IsUpvote { get; set; }
    public string? UserId { get; set; } // Will be set by the calling service with current user ID
}