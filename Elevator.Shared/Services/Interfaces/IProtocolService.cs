using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Interfaces;

public interface IProtocolService
{
    Task<IEnumerable<ProtocolDto>> GetProtocolsAsync();
    Task<IEnumerable<ProtocolDto>> GetUserProtocolsAsync(string userId);
    Task<ProtocolDetailDto?> GetProtocolAsync(int id);
    Task<ProtocolDto> CreateProtocolAsync(CreateProtocolDto protocol);
    Task<ProtocolDto> UpdateProtocolAsync(int id, UpdateProtocolDto protocol);
    Task DeleteProtocolAsync(int id);
}