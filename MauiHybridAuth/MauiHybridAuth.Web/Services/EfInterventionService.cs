using MauiHybridAuth.Shared.Models;
using MauiHybridAuth.Shared.Services;
using MauiHybridAuth.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Services;

public class EfInterventionService : IInterventionService
{
    private readonly ApplicationDbContext _db;
    public EfInterventionService(ApplicationDbContext db) => _db = db;

    public async Task<List<Intervention>> GetAllAsync()
    {
        return await _db.Interventions
            .Include(i => i.InterventionRatings)
            .Include(i => i.InterventionCategories)
                .ThenInclude(ic => ic.Category)
            .ToListAsync();
    }
}