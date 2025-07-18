using Elevator.Web.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Elevator.Web.Data;

/// <summary>
/// Application database context that extends IdentityDbContext to provide
/// ASP.NET Core Identity functionality with custom ApplicationUser.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser entity
        builder.Entity<ApplicationUser>(entity =>
        {
            // Set default value for CreatedAt to current UTC time
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Add index on Email for better query performance
            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_ApplicationUser_Email");

            // Ensure Email is not null and has proper length constraints
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();

            // Configure LastLoginAt as nullable
            entity.Property(e => e.LastLoginAt)
                .IsRequired(false);
        });
    }
}