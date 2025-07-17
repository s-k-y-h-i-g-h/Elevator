using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Implementations;

public class WebInterventionService : IInterventionService
{
    private readonly ElevatorDbContext _context;

    public WebInterventionService(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InterventionDto>> GetInterventionsAsync()
    {
        var interventions = await _context.Interventions
            .Include(i => i.Ratings)
            .ToListAsync();

        return interventions.Select(MapToDto);
    }

    public async Task<InterventionDetailDto?> GetInterventionAsync(int id)
    {
        var intervention = await _context.Interventions
            .Include(i => i.Ratings)
            .Include(i => i.Discussions)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null)
            return null;

        var dto = MapToDetailDto(intervention);
        
        // Load plant constituents if it's a plant
        if (intervention is Plant plant)
        {
            await _context.Entry(plant)
                .Collection(p => p.Constituents)
                .LoadAsync();
            dto.ConstituentIds = plant.Constituents.Select(c => c.Id).ToList();
        }

        return dto;
    }

    public async Task<InterventionDto> CreateInterventionAsync(CreateInterventionDto interventionDto)
    {
        Intervention intervention = interventionDto.InterventionType.ToLower() switch
        {
            "compound" => new Compound
            {
                Name = interventionDto.Name,
                Description = interventionDto.Description,
                Duration = interventionDto.Duration,
                DoseRange = interventionDto.DoseRange
            },
            "plant" => new Plant
            {
                Name = interventionDto.Name,
                Description = interventionDto.Description,
                Duration = interventionDto.Duration,
                DoseRange = interventionDto.DoseRange,
                ScientificName = interventionDto.ScientificName,
                CommonNames = interventionDto.CommonNames,
                TraditionalUses = interventionDto.TraditionalUses
            },
            _ => throw new ArgumentException($"Unknown intervention type: {interventionDto.InterventionType}")
        };

        intervention.CreatedAt = DateTime.UtcNow;
        intervention.UpdatedAt = DateTime.UtcNow;

        _context.Interventions.Add(intervention);
        await _context.SaveChangesAsync();

        // Handle plant constituents
        if (intervention is Plant plant && interventionDto.ConstituentIds.Any())
        {
            var compounds = await _context.Compounds
                .Where(c => interventionDto.ConstituentIds.Contains(c.Id))
                .ToListAsync();
            
            plant.Constituents = compounds;
            await _context.SaveChangesAsync();
        }

        return MapToDto(intervention);
    }

    public async Task<InterventionDto> UpdateInterventionAsync(int id, UpdateInterventionDto interventionDto)
    {
        var intervention = await _context.Interventions.FindAsync(id);
        if (intervention == null)
            throw new ArgumentException($"Intervention with ID {id} not found");

        // Update common properties
        if (interventionDto.Name != null)
            intervention.Name = interventionDto.Name;
        if (interventionDto.Description != null)
            intervention.Description = interventionDto.Description;

        // Update substance properties
        if (intervention is Substance substance)
        {
            if (interventionDto.Duration != null)
                substance.Duration = interventionDto.Duration;
            if (interventionDto.DoseRange != null)
                substance.DoseRange = interventionDto.DoseRange;
        }

        // Update plant-specific properties
        if (intervention is Plant plant)
        {
            if (interventionDto.ScientificName != null)
                plant.ScientificName = interventionDto.ScientificName;
            if (interventionDto.CommonNames != null)
                plant.CommonNames = interventionDto.CommonNames;
            if (interventionDto.TraditionalUses != null)
                plant.TraditionalUses = interventionDto.TraditionalUses;

            // Update constituents
            if (interventionDto.ConstituentIds != null)
            {
                await _context.Entry(plant)
                    .Collection(p => p.Constituents)
                    .LoadAsync();
                
                plant.Constituents.Clear();
                
                if (interventionDto.ConstituentIds.Any())
                {
                    var compounds = await _context.Compounds
                        .Where(c => interventionDto.ConstituentIds.Contains(c.Id))
                        .ToListAsync();
                    
                    foreach (var compound in compounds)
                    {
                        plant.Constituents.Add(compound);
                    }
                }
            }
        }

        intervention.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(intervention);
    }

    public async Task DeleteInterventionAsync(int id)
    {
        var intervention = await _context.Interventions.FindAsync(id);
        if (intervention == null)
            throw new ArgumentException($"Intervention with ID {id} not found");

        _context.Interventions.Remove(intervention);
        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetAiInformationAsync(string interventionName)
    {
        // TODO: Implement AI information retrieval
        // This would integrate with an AI service to get information about the intervention
        await Task.Delay(1); // Placeholder to avoid compiler warning
        return null;
    }

    private static InterventionDto MapToDto(Intervention intervention)
    {
        var dto = new InterventionDto
        {
            Id = intervention.Id,
            Name = intervention.Name,
            Description = intervention.Description,
            InterventionType = intervention.GetType().Name,
            CreatedAt = intervention.CreatedAt,
            UpdatedAt = intervention.UpdatedAt,
            AverageRating = intervention.Ratings.Any() ? intervention.Ratings.Average(r => r.Value) : 0,
            RatingCount = intervention.Ratings.Count
        };

        if (intervention is Substance substance)
        {
            dto.Duration = substance.Duration;
            dto.DoseRange = substance.DoseRange;
        }

        if (intervention is Plant plant)
        {
            dto.ScientificName = plant.ScientificName;
            dto.CommonNames = plant.CommonNames;
            dto.TraditionalUses = plant.TraditionalUses;
        }

        return dto;
    }

    private static InterventionDetailDto MapToDetailDto(Intervention intervention)
    {
        var dto = new InterventionDetailDto
        {
            Id = intervention.Id,
            Name = intervention.Name,
            Description = intervention.Description,
            InterventionType = intervention.GetType().Name,
            CreatedAt = intervention.CreatedAt,
            UpdatedAt = intervention.UpdatedAt,
            AverageRating = intervention.Ratings.Any() ? intervention.Ratings.Average(r => r.Value) : 0,
            RatingCount = intervention.Ratings.Count,
            AiRetrievedInfo = intervention.AiRetrievedInfo,
            Discussions = intervention.Discussions.Select(d => new DiscussionDto
            {
                Id = d.Id,
                Title = d.Title,
                Content = d.Content,
                UserId = d.UserId,
                UserName = d.User?.Email ?? "Unknown",
                InterventionId = d.InterventionId,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                CommentCount = d.Comments.Count,
                UpvoteCount = d.Votes.Count(v => v.IsUpvote),
                DownvoteCount = d.Votes.Count(v => !v.IsUpvote)
            }).ToList()
        };

        if (intervention is Substance substance)
        {
            dto.Duration = substance.Duration;
            dto.DoseRange = substance.DoseRange;
        }

        if (intervention is Plant plant)
        {
            dto.ScientificName = plant.ScientificName;
            dto.CommonNames = plant.CommonNames;
            dto.TraditionalUses = plant.TraditionalUses;
        }

        return dto;
    }
}