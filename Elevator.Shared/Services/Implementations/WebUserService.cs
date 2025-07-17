using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Implementations;

public class WebUserService : IUserService
{
    private readonly ElevatorDbContext _context;

    public WebUserService(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto userDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new ArgumentException($"User with ID {id} not found");

        if (userDto.FirstName != null)
            user.FirstName = userDto.FirstName;
        if (userDto.LastName != null)
            user.LastName = userDto.LastName;

        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    private static UserDto MapToDto(Models.Users.ApplicationUser user)
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