using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class Comment
{
    public Guid Id { get; set; }
    
    [Required]
    public string Content { get; set; } = default!;
    
    public int VoteScore { get; set; } = 0;
    
    public int NestingLevel { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    public Guid DiscussionId { get; set; }
    public Discussion Discussion { get; set; } = default!;
    
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    
    [Required]
    public string UserId { get; set; } = default!;
    
    // Navigation properties
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}