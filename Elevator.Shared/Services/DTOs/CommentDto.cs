namespace Elevator.Shared.Services.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int DiscussionId { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public int DiscussionId { get; set; }
    public int? ParentCommentId { get; set; }
    public string? UserId { get; set; } // Will be set by the calling service with current user ID
}