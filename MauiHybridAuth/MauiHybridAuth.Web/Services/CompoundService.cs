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
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Compound?> GetCompoundByIdAsync(Guid id)
        {
            return await _context.Compounds
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
} 