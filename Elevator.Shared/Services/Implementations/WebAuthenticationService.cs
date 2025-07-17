using Microsoft.AspNetCore.Identity;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Implementations;

public class WebAuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private ApplicationUser? _currentUser;

    public WebAuthenticationService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public bool IsAuthenticated => _currentUser != null;
    public string? CurrentUserId => _currentUser?.Id;

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        
        if (isValidPassword)
        {
            _currentUser = user;
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new AuthResult
            {
                Success = true,
                User = MapToDto(user)
            };
        }

        return new AuthResult
        {
            Success = false,
            ErrorMessage = "Invalid email or password"
        };
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string? firstName = null, string? lastName = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        
        if (result.Succeeded)
        {
            _currentUser = user;

            return new AuthResult
            {
                Success = true,
                User = MapToDto(user)
            };
        }

        return new AuthResult
        {
            Success = false,
            ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        await Task.CompletedTask;
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return MapToDto(_currentUser);

        return await Task.FromResult<UserDto?>(null);
    }

    private static UserDto MapToDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt ?? DateTime.UtcNow
        };
    }
}