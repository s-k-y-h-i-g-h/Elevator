using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Shared.Services
{
    public interface ICompoundService
    {
        Task<List<Compound>> GetAllCompoundsAsync();
        Task<Compound?> GetCompoundByIdAsync(Guid id);
        Task<List<Compound>> GetCompoundsByCategoryAsync(string? category1 = null, string? category2 = null, string? category3 = null);
    }
} 