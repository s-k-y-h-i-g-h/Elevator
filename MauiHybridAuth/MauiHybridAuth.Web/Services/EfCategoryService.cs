using MauiHybridAuth.Shared.Models;
using MauiHybridAuth.Shared.Services;
using MauiHybridAuth.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Services;

public class EfCategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;

    public EfCategoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _db.Categories
            .Include(c => c.Subcategories)
            .Include(c => c.ParentCategory)
            .ToListAsync();
    }

    public async Task<List<Category>> GetMainCategoriesWithSubcategoriesAsync()
    {
        return await _db.Categories
            .Where(c => c.ParentCategoryId == null) // Only main categories
            .Include(c => c.Subcategories.OrderBy(s => s.Name))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _db.Categories
            .Include(c => c.Subcategories)
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Category>> GetSubcategoriesByParentIdAsync(Guid parentId)
    {
        return await _db.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
} 