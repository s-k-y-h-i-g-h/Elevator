namespace Elevator.Shared.Services.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}