namespace Elevator.Shared.Services.DTOs;

public class InterventionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty; // "Compound", "Plant", etc.
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    
    // Substance-specific properties (null if not a substance)
    public string? Duration { get; set; }
    public string? DoseRange { get; set; }
    
    // Plant-specific properties (null if not a plant)
    public string? ScientificName { get; set; }
    public string? CommonNames { get; set; }
    public string? TraditionalUses { get; set; }
}

public class InterventionDetailDto : InterventionDto
{
    public string? AiRetrievedInfo { get; set; }
    public List<int> ConstituentIds { get; set; } = new(); // For plants - IDs of constituent compounds
    public List<DiscussionDto> Discussions { get; set; } = new();
}

public class CreateInterventionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty; // "Compound", "Plant"
    
    // Substance-specific properties
    public string? Duration { get; set; }
    public string? DoseRange { get; set; }
    
    // Plant-specific properties
    public string? ScientificName { get; set; }
    public string? CommonNames { get; set; }
    public string? TraditionalUses { get; set; }
    public List<int> ConstituentIds { get; set; } = new();
}

public class UpdateInterventionDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    // Substance-specific properties
    public string? Duration { get; set; }
    public string? DoseRange { get; set; }
    
    // Plant-specific properties
    public string? ScientificName { get; set; }
    public string? CommonNames { get; set; }
    public string? TraditionalUses { get; set; }
    public List<int>? ConstituentIds { get; set; }
}