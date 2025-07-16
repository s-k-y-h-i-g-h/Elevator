using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Elevator.Shared.Models.Users;

/// <summary>
/// Application user extending ASP.NET Core Identity User
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name
    /// </summary>
    [StringLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [StringLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Protocols created by this user
    /// </summary>
    public virtual ICollection<Protocols.Protocol> Protocols { get; set; } = new List<Protocols.Protocol>();

    /// <summary>
    /// Discussions created by this user
    /// </summary>
    public virtual ICollection<Discussion.Discussion> Discussions { get; set; } = new List<Discussion.Discussion>();

    /// <summary>
    /// Comments made by this user
    /// </summary>
    public virtual ICollection<Discussion.Comment> Comments { get; set; } = new List<Discussion.Comment>();

    /// <summary>
    /// Ratings submitted by this user
    /// </summary>
    public virtual ICollection<Ratings.Rating> Ratings { get; set; } = new List<Ratings.Rating>();

    /// <summary>
    /// Votes cast by this user
    /// </summary>
    public virtual ICollection<Discussion.Vote> Votes { get; set; } = new List<Discussion.Vote>();
}