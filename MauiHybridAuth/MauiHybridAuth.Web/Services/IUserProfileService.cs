using MauiHybridAuth.Web.Data;

namespace MauiHybridAuth.Web.Services
{
    public interface IUserProfileService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        Task<bool> IsProfilePublicAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetPublicUsersAsync();
    }
}