using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Web.Data;

namespace MauiHybridAuth.Web.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<bool> IsProfilePublicAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.IsPublicProfile ?? false;
        }

        public async Task<IEnumerable<ApplicationUser>> GetPublicUsersAsync()
        {
            return await _userManager.Users
                .Where(u => u.IsPublicProfile)
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
    }
}