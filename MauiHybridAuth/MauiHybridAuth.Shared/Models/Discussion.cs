using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class Discussion
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = default!;
    
    [Required]
    public string Content { get; set; } = default!;
    
    public int VoteScore { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    public Guid? InterventionId { get; set; }
    public Intervention? Intervention { get; set; }
    
    public Guid? ProtocolId { get; set; }
    public Protocol? Protocol { get; set; }
    
    [Required]
    public string UserId { get; set; } = default!;
    
    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}