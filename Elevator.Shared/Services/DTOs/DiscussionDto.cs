namespace Elevator.Shared.Services.DTOs;

public class DiscussionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int? InterventionId { get; set; }
    public string? InterventionName { get; set; }
    public int? ProtocolId { get; set; }
    public string? ProtocolName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; }
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }
}

public class DiscussionDetailDto : DiscussionDto
{
    public List<CommentDto> Comments { get; set; } = new();
}

public class CreateDiscussionDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? InterventionId { get; set; }
    public int? ProtocolId { get; set; }
}