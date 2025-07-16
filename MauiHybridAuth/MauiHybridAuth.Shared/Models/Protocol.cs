using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class Protocol
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = default!;
    
    [Required]
    public string Description { get; set; } = default!;
    
    public bool IsPublic { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key to ApplicationUser
    [Required]
    public string UserId { get; set; } = default!;
    
    // Navigation properties
    public ICollection<InterventionProtocol> InterventionProtocols { get; set; } = new List<InterventionProtocol>();
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
}