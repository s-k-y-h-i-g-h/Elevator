using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Shared.Services
{
    public interface IInterventionService
    {
        Task<List<Intervention>> GetAllInterventionsAsync();
        Task<Intervention?> GetInterventionByIdAsync(Guid id);
        Task<List<Intervention>> GetInterventionsByCategoryAsync(string? category1 = null, string? category2 = null, string? category3 = null);
        Task<List<Compound>> GetAllCompoundsAsync();
        Task<List<Plant>> GetAllPlantsAsync();
        Task<List<Formulation>> GetAllFormulationsAsync();
    }
} 