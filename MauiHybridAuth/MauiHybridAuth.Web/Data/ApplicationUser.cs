using Microsoft.AspNetCore.Identity;

namespace MauiHybridAuth.Web.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? Bio { get; set; }
        public bool IsPublicProfile { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
