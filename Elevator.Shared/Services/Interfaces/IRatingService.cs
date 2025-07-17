using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Interfaces;

public interface IRatingService
{
    Task<decimal> GetAverageRatingAsync(int? interventionId = null, int? protocolId = null);
    Task<RatingDto?> GetUserRatingAsync(string userId, int? interventionId = null, int? protocolId = null);
    Task<RatingDto> CreateOrUpdateRatingAsync(CreateRatingDto rating);
    Task DeleteRatingAsync(int id);
}