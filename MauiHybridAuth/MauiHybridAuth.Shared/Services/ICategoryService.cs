using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Shared.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<Category>> GetMainCategoriesWithSubcategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<List<Category>> GetSubcategoriesByParentIdAsync(Guid parentId);
} 