using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Shared.Services
{
    public interface ICompoundService
    {
        Task<List<Compound>> GetAllCompoundsAsync();
        Task<Compound?> GetCompoundByIdAsync(Guid id);
    }
} 