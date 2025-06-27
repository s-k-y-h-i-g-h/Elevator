namespace MauiHybridAuth.Shared.Models;

public class InterventionCategory
{
    public Guid InterventionId { get; set; }
    public Intervention Intervention { get; set; } = default!;
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
} 