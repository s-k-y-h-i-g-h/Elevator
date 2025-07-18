namespace Elevator.Shared.Models;

/// <summary>
/// Represents a standardized API error response
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    public string? TraceId { get; set; }
}