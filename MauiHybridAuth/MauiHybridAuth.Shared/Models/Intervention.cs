using System.Collections.Generic;

namespace MauiHybridAuth.Shared.Models;

public abstract class Intervention
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    // Navigation property for related ratings
    public ICollection<InterventionRating> InterventionRatings { get; set; } = new List<InterventionRating>();

    public double CalculateAverageRating()
    {
        if (InterventionRatings == null || !InterventionRatings.Any())
            return 0;

        return InterventionRatings.Average(r => r.Rating);
    }
}