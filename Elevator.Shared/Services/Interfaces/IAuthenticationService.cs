using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string password, string? firstName = null, string? lastName = null);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    bool IsAuthenticated { get; }
    string? CurrentUserId { get; }
}