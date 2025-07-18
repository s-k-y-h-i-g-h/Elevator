using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Shared.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Category>> GetRootCategoriesAsync(); // Categories with no parent
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<List<Category>> GetSubcategoriesAsync(Guid parentId);
    }
} 