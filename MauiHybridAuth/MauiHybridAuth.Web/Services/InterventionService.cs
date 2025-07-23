using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Shared.Models;
using MauiHybridAuth.Shared.Services;
using MauiHybridAuth.Web.Data;

namespace MauiHybridAuth.Web.Services
{
    public class InterventionService : IInterventionService
    {
        private readonly ApplicationDbContext _context;

        public InterventionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Intervention>> GetAllInterventionsAsync()
        {
            var compounds = await _context.Compounds
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Cast<Intervention>()
                .ToListAsync();

            var plants = await _context.Plants
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(p => p.Constituents)
                .Cast<Intervention>()
                .ToListAsync();

            var formulations = await _context.Formulations
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(f => f.Constituents)
                .Cast<Intervention>()
                .ToListAsync();

            var allInterventions = compounds.Concat(plants).Concat(formulations).ToList();
            return allInterventions.OrderBy(i => i.Name).ToList();
        }

        public async Task<Intervention?> GetInterventionByIdAsync(Guid id)
        {
            // Try to find in compounds first
            var compound = await _context.Compounds
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compound != null) return compound;

            // Try plants
            var plant = await _context.Plants
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(p => p.Constituents)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plant != null) return plant;

            // Try formulations
            var formulation = await _context.Formulations
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(f => f.Constituents)
                .FirstOrDefaultAsync(f => f.Id == id);

            return formulation;
        }

        public async Task<List<Intervention>> GetInterventionsByCategoryAsync(string? category1 = null, string? category2 = null, string? category3 = null)
        {
            // Use the most specific category that's provided
            string? targetCategory = category3 ?? category2 ?? category1;

            // If no categories specified, return all interventions
            if (string.IsNullOrEmpty(targetCategory))
            {
                return await GetAllInterventionsAsync();
            }

            var compounds = await _context.Compounds
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Where(c => c.Categories.Any(cat => 
                    cat.Name.ToLower() == targetCategory.ToLower()
                ))
                .Cast<Intervention>()
                .ToListAsync();

            var plants = await _context.Plants
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(p => p.Constituents)
                .Where(p => p.Categories.Any(cat => 
                    cat.Name.ToLower() == targetCategory.ToLower()
                ))
                .Cast<Intervention>()
                .ToListAsync();

            var formulations = await _context.Formulations
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(f => f.Constituents)
                .Where(f => f.Categories.Any(cat => 
                    cat.Name.ToLower() == targetCategory.ToLower()
                ))
                .Cast<Intervention>()
                .ToListAsync();

            var allInterventions = compounds.Concat(plants).Concat(formulations).ToList();
            return allInterventions.OrderBy(i => i.Name).ToList();
        }

        public async Task<List<Compound>> GetAllCompoundsAsync()
        {
            return await _context.Compounds
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            return await _context.Plants
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(p => p.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(p => p.Constituents)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<Formulation>> GetAllFormulationsAsync()
        {
            return await _context.Formulations
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(f => f.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .Include(f => f.Constituents)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
    }
} 