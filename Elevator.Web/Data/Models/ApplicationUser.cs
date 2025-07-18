using Microsoft.AspNetCore.Identity;

namespace Elevator.Web.Data.Models;

/// <summary>
/// Custom user model extending IdentityUser with additional properties
/// for tracking user creation and last login times.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The date and time when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the user last logged in.
    /// Null if the user has never logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}