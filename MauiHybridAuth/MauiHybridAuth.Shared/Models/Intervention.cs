using System.Collections.Generic;
namespace MauiHybridAuth.Shared.Models;

public abstract class Intervention
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    // Navigation property for related ratings
    public ICollection<InterventionRating> InterventionRatings { get; set; } = new List<InterventionRating>();
    
    // Navigation property for related categories
    public ICollection<InterventionCategory> InterventionCategories { get; set; } = new List<InterventionCategory>();

    public double CalculateAverageRating()
    {
        if (InterventionRatings == null || !InterventionRatings.Any())
            return 0;

        return InterventionRatings.Average(r => r.Rating);
    }
    
    public IEnumerable<Category> GetCategories()
    {
        return InterventionCategories?.Select(ic => ic.Category) ?? Enumerable.Empty<Category>();
    }
}