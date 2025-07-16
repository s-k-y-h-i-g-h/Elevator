using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class Vote
{
    public Guid Id { get; set; }
    
    public VoteType Type { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys - either DiscussionId or CommentId will be set, not both
    public Guid? DiscussionId { get; set; }
    public Discussion? Discussion { get; set; }
    
    public Guid? CommentId { get; set; }
    public Comment? Comment { get; set; }
    
    [Required]
    public string UserId { get; set; } = default!;
}