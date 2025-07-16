using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class Advertisement
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = default!;
    
    [Required]
    public string Content { get; set; } = default!;
    
    [StringLength(500)]
    public string? ImageUrl { get; set; }
    
    [Required]
    [StringLength(500)]
    public string TargetUrl { get; set; } = default!;
    
    [Required]
    [StringLength(100)]
    public string Placement { get; set; } = default!; // e.g., "sidebar", "header", "between-content"
    
    public bool IsActive { get; set; } = true;
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    
    public int ClickCount { get; set; } = 0;
}