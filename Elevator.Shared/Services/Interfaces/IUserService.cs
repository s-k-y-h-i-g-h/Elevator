using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(string id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserDto user);
}