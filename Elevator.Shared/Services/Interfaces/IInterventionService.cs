using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Interfaces;

public interface IInterventionService
{
    Task<IEnumerable<InterventionDto>> GetInterventionsAsync();
    Task<InterventionDetailDto?> GetInterventionAsync(int id);
    Task<InterventionDto> CreateInterventionAsync(CreateInterventionDto intervention);
    Task<InterventionDto> UpdateInterventionAsync(int id, UpdateInterventionDto intervention);
    Task DeleteInterventionAsync(int id);
    Task<string?> GetAiInformationAsync(string interventionName);
}