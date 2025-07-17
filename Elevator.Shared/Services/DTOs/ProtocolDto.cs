namespace Elevator.Shared.Services.DTOs;

public class ProtocolDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<ProtocolInterventionDto> Interventions { get; set; } = new();
}

public class ProtocolDetailDto : ProtocolDto
{
    public List<DiscussionDto> Discussions { get; set; } = new();
}

public class ProtocolInterventionDto
{
    public int InterventionId { get; set; }
    public string InterventionName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Notes { get; set; }
}

public class CreateProtocolDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CreateProtocolInterventionDto> Interventions { get; set; } = new();
}

public class CreateProtocolInterventionDto
{
    public int InterventionId { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProtocolDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<CreateProtocolInterventionDto>? Interventions { get; set; }
}