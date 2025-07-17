namespace Elevator.Shared.Services.DTOs;

public class RatingDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int? InterventionId { get; set; }
    public string? InterventionName { get; set; }
    public int? ProtocolId { get; set; }
    public string? ProtocolName { get; set; }
    public decimal Value { get; set; } // 1-5 scale
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRatingDto
{
    public int? InterventionId { get; set; }
    public int? ProtocolId { get; set; }
    public decimal Value { get; set; } // 1-5 scale
    public string? Review { get; set; }
    public string? UserId { get; set; } // Will be set by the calling service with current user ID
}