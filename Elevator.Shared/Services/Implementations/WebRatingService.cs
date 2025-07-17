using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Ratings;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Implementations;

public class WebRatingService : IRatingService
{
    private readonly ElevatorDbContext _context;

    public WebRatingService(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetAverageRatingAsync(int? interventionId = null, int? protocolId = null)
    {
        var query = _context.Ratings.AsQueryable();

        if (interventionId.HasValue)
            query = query.Where(r => r.InterventionId == interventionId);
        if (protocolId.HasValue)
            query = query.Where(r => r.ProtocolId == protocolId);

        return await query.AverageAsync(r => (decimal?)r.Value) ?? 0;
    }

    public async Task<RatingDto?> GetUserRatingAsync(string userId, int? interventionId = null, int? protocolId = null)
    {
        var query = _context.Ratings
            .Include(r => r.User)
            .Include(r => r.Intervention)
            .Include(r => r.Protocol)
            .Where(r => r.UserId == userId);

        if (interventionId.HasValue)
            query = query.Where(r => r.InterventionId == interventionId);
        if (protocolId.HasValue)
            query = query.Where(r => r.ProtocolId == protocolId);

        var rating = await query.FirstOrDefaultAsync();
        return rating == null ? null : MapToDto(rating);
    }

    public async Task<RatingDto> CreateOrUpdateRatingAsync(CreateRatingDto ratingDto)
    {
        var userId = ratingDto.UserId ?? throw new ArgumentException("UserId is required");
        
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId &&
                                    r.InterventionId == ratingDto.InterventionId &&
                                    r.ProtocolId == ratingDto.ProtocolId);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Value = ratingDto.Value;
            existingRating.Review = ratingDto.Review;
            existingRating.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload with includes
            var updatedRating = await _context.Ratings
                .Include(r => r.User)
                .Include(r => r.Intervention)
                .Include(r => r.Protocol)
                .FirstAsync(r => r.Id == existingRating.Id);

            return MapToDto(updatedRating);
        }

        // Create new rating
        var rating = new Rating
        {
            UserId = userId,
            InterventionId = ratingDto.InterventionId,
            ProtocolId = ratingDto.ProtocolId,
            Value = ratingDto.Value,
            Review = ratingDto.Review,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Reload with includes
        var createdRating = await _context.Ratings
            .Include(r => r.User)
            .Include(r => r.Intervention)
            .Include(r => r.Protocol)
            .FirstAsync(r => r.Id == rating.Id);

        return MapToDto(createdRating);
    }

    public async Task DeleteRatingAsync(int id)
    {
        var rating = await _context.Ratings.FindAsync(id);
        if (rating == null)
            throw new ArgumentException($"Rating with ID {id} not found");

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
    }

    private static RatingDto MapToDto(Rating rating)
    {
        return new RatingDto
        {
            Id = rating.Id,
            UserId = rating.UserId,
            UserName = rating.User?.Email ?? "Unknown",
            InterventionId = rating.InterventionId,
            InterventionName = rating.Intervention?.Name,
            ProtocolId = rating.ProtocolId,
            ProtocolName = rating.Protocol?.Name,
            Value = rating.Value,
            Review = rating.Review,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };
    }
}