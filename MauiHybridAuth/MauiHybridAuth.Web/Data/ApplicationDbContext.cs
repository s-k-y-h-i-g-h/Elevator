using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Compound> Compounds { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPC (Table Per Concrete) inheritance
            modelBuilder.Entity<Intervention>()
                .UseTpcMappingStrategy();

            modelBuilder.Entity<Substance>()
                .UseTpcMappingStrategy();

            // Configure table for the concrete class
            modelBuilder.Entity<Compound>();

            // Configure Category self-referencing relationship
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid cycles

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique(false); // Allow duplicate names at different levels

            modelBuilder.Entity<Category>()
                .ToTable("Categories");

            // Configure many-to-many relationship between Interventions and Categories
            modelBuilder.Entity<Intervention>()
                .HasMany(i => i.Categories)
                .WithMany(c => c.Interventions)
                .UsingEntity<Dictionary<string, object>>(
                    "InterventionCategory",
                    j => j
                        .HasOne<Category>()
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Intervention>()
                        .WithMany()
                        .HasForeignKey("InterventionId")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}
