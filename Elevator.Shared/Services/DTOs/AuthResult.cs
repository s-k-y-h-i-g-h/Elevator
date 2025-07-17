namespace Elevator.Shared.Services.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; } // JWT token for MAUI
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}