using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Compound> Compounds { get; set; }
        public DbSet<Intervention> Interventions { get; set; }
        public DbSet<InterventionRating> InterventionRatings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<InterventionCategory> InterventionCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPC inheritance for Intervention hierarchy
            modelBuilder.Entity<Intervention>(entity =>
            {
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Compound>(entity =>
            {
                entity.ToTable("Compound");
                entity.UseTpcMappingStrategy();
            });

            // Configure InterventionRating relationships
            modelBuilder.Entity<InterventionRating>()
                .HasOne(ir => ir.Intervention)
                .WithMany(i => i.InterventionRatings)
                .HasForeignKey(ir => ir.InterventionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterventionRating>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ir => ir.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Category self-referencing relationship for subcategories
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.Subcategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of parent categories
            });

            // Configure InterventionCategory many-to-many relationship
            modelBuilder.Entity<InterventionCategory>(entity =>
            {
                entity.HasKey(ic => new { ic.InterventionId, ic.CategoryId });
                
                entity.HasOne(ic => ic.Intervention)
                    .WithMany(i => i.InterventionCategories)
                    .HasForeignKey(ic => ic.InterventionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ic => ic.Category)
                    .WithMany(c => c.InterventionCategories)
                    .HasForeignKey(ic => ic.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
