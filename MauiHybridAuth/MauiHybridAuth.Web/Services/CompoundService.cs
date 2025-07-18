using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Shared.Models;
using MauiHybridAuth.Shared.Services;
using MauiHybridAuth.Web.Data;

namespace MauiHybridAuth.Web.Services
{
    public class CompoundService : ICompoundService
    {
        private readonly ApplicationDbContext _context;

        public CompoundService(ApplicationDbContext context)
        {
            _context = context;
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

        public async Task<Compound?> GetCompoundByIdAsync(Guid id)
        {
            return await _context.Compounds
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Compound>> GetCompoundsByCategoryAsync(string? category1 = null, string? category2 = null, string? category3 = null)
        {
            var query = _context.Compounds
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Parent)
                .Include(c => c.Categories)
                    .ThenInclude(cat => cat.Subcategories)
                .AsQueryable();

            // Use the most specific category that's provided
            string? targetCategory = category3 ?? category2 ?? category1;

            // If no categories specified, return all compounds
            if (string.IsNullOrEmpty(targetCategory))
            {
                return await query.OrderBy(c => c.Name).ToListAsync();
            }

            // Filter by the target category
            query = query.Where(c => c.Categories.Any(cat => 
                cat.Name.ToLower() == targetCategory.ToLower()
            ));

            return await query.OrderBy(c => c.Name).ToListAsync();
        }
    }
} 