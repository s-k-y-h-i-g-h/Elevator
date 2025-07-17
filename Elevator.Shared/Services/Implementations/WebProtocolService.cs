using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Protocols;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Implementations;

public class WebProtocolService : IProtocolService
{
    private readonly ElevatorDbContext _context;

    public WebProtocolService(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProtocolDto>> GetProtocolsAsync()
    {
        var protocols = await _context.Protocols
            .Include(p => p.User)
            .Include(p => p.Ratings)
            .Include(p => p.ProtocolInterventions)
                .ThenInclude(pi => pi.Intervention)
            .ToListAsync();

        return protocols.Select(MapToDto);
    }

    public async Task<IEnumerable<ProtocolDto>> GetUserProtocolsAsync(string userId)
    {
        var protocols = await _context.Protocols
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .Include(p => p.Ratings)
            .Include(p => p.ProtocolInterventions)
                .ThenInclude(pi => pi.Intervention)
            .ToListAsync();

        return protocols.Select(MapToDto);
    }

    public async Task<ProtocolDetailDto?> GetProtocolAsync(int id)
    {
        var protocol = await _context.Protocols
            .Include(p => p.User)
            .Include(p => p.Ratings)
            .Include(p => p.ProtocolInterventions)
                .ThenInclude(pi => pi.Intervention)
            .Include(p => p.Discussions)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (protocol == null)
            return null;

        return MapToDetailDto(protocol);
    }

    public async Task<ProtocolDto> CreateProtocolAsync(CreateProtocolDto protocolDto)
    {
        var protocol = new Protocol
        {
            Name = protocolDto.Name,
            Description = protocolDto.Description,
            UserId = "", // This should be set by the calling service with current user ID
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Protocols.Add(protocol);
        await _context.SaveChangesAsync();

        // Add protocol interventions
        if (protocolDto.Interventions.Any())
        {
            var protocolInterventions = protocolDto.Interventions.Select(i => new ProtocolIntervention
            {
                ProtocolId = protocol.Id,
                InterventionId = i.InterventionId,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Notes = i.Notes
            });

            _context.ProtocolInterventions.AddRange(protocolInterventions);
            await _context.SaveChangesAsync();
        }

        // Reload with includes for proper DTO mapping
        var createdProtocol = await _context.Protocols
            .Include(p => p.User)
            .Include(p => p.Ratings)
            .Include(p => p.ProtocolInterventions)
                .ThenInclude(pi => pi.Intervention)
            .FirstAsync(p => p.Id == protocol.Id);

        return MapToDto(createdProtocol);
    }

    public async Task<ProtocolDto> UpdateProtocolAsync(int id, UpdateProtocolDto protocolDto)
    {
        var protocol = await _context.Protocols
            .Include(p => p.ProtocolInterventions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (protocol == null)
            throw new ArgumentException($"Protocol with ID {id} not found");

        // Update basic properties
        if (protocolDto.Name != null)
            protocol.Name = protocolDto.Name;
        if (protocolDto.Description != null)
            protocol.Description = protocolDto.Description;

        // Update interventions if provided
        if (protocolDto.Interventions != null)
        {
            // Remove existing interventions
            _context.ProtocolInterventions.RemoveRange(protocol.ProtocolInterventions);

            // Add new interventions
            if (protocolDto.Interventions.Any())
            {
                var newProtocolInterventions = protocolDto.Interventions.Select(i => new ProtocolIntervention
                {
                    ProtocolId = protocol.Id,
                    InterventionId = i.InterventionId,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Notes = i.Notes
                });

                _context.ProtocolInterventions.AddRange(newProtocolInterventions);
            }
        }

        protocol.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload with includes for proper DTO mapping
        var updatedProtocol = await _context.Protocols
            .Include(p => p.User)
            .Include(p => p.Ratings)
            .Include(p => p.ProtocolInterventions)
                .ThenInclude(pi => pi.Intervention)
            .FirstAsync(p => p.Id == protocol.Id);

        return MapToDto(updatedProtocol);
    }

    public async Task DeleteProtocolAsync(int id)
    {
        var protocol = await _context.Protocols.FindAsync(id);
        if (protocol == null)
            throw new ArgumentException($"Protocol with ID {id} not found");

        _context.Protocols.Remove(protocol);
        await _context.SaveChangesAsync();
    }

    private static ProtocolDto MapToDto(Protocol protocol)
    {
        return new ProtocolDto
        {
            Id = protocol.Id,
            Name = protocol.Name,
            Description = protocol.Description,
            UserId = protocol.UserId,
            UserName = protocol.User?.Email ?? "Unknown",
            CreatedAt = protocol.CreatedAt,
            UpdatedAt = protocol.UpdatedAt,
            AverageRating = protocol.Ratings.Any() ? protocol.Ratings.Average(r => r.Value) : 0,
            RatingCount = protocol.Ratings.Count,
            Interventions = protocol.ProtocolInterventions.Select(pi => new ProtocolInterventionDto
            {
                InterventionId = pi.InterventionId,
                InterventionName = pi.Intervention?.Name ?? "Unknown",
                Dosage = pi.Dosage,
                Frequency = pi.Frequency,
                Notes = pi.Notes
            }).ToList()
        };
    }

    private static ProtocolDetailDto MapToDetailDto(Protocol protocol)
    {
        return new ProtocolDetailDto
        {
            Id = protocol.Id,
            Name = protocol.Name,
            Description = protocol.Description,
            UserId = protocol.UserId,
            UserName = protocol.User?.Email ?? "Unknown",
            CreatedAt = protocol.CreatedAt,
            UpdatedAt = protocol.UpdatedAt,
            AverageRating = protocol.Ratings.Any() ? protocol.Ratings.Average(r => r.Value) : 0,
            RatingCount = protocol.Ratings.Count,
            Interventions = protocol.ProtocolInterventions.Select(pi => new ProtocolInterventionDto
            {
                InterventionId = pi.InterventionId,
                InterventionName = pi.Intervention?.Name ?? "Unknown",
                Dosage = pi.Dosage,
                Frequency = pi.Frequency,
                Notes = pi.Notes
            }).ToList(),
            Discussions = protocol.Discussions.Select(d => new DiscussionDto
            {
                Id = d.Id,
                Title = d.Title,
                Content = d.Content,
                UserId = d.UserId,
                UserName = d.User?.Email ?? "Unknown",
                ProtocolId = d.ProtocolId,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                CommentCount = d.Comments.Count,
                UpvoteCount = d.Votes.Count(v => v.IsUpvote),
                DownvoteCount = d.Votes.Count(v => !v.IsUpvote)
            }).ToList()
        };
    }
}