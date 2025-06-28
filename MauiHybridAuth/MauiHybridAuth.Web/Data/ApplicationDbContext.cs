using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            modelBuilder.Entity<Substance>(entity =>
            {
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Compound>(entity =>
            {
                entity.ToTable("Compound");
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Augmentation>(entity =>
            {
                entity.ToTable("Augmentation");
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Behavioral>(entity =>
            {
                entity.ToTable("Behavioral");
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Dietary>(entity =>
            {
                entity.ToTable("Dietary");
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Physiological>(entity =>
            {
                entity.ToTable("Physiological");
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<Wearable>(entity =>
            {
                entity.ToTable("Wearable");
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

            // Configure Substance ClassificationTags enum collection
            modelBuilder.Entity<Substance>(entity =>
            {
                entity.Property(e => e.ClassificationTags)
                    .HasConversion(
                        v => string.Join(',', v.Select(tag => tag.ToString())),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(tag => (ClassificationTag)Enum.Parse(typeof(ClassificationTag), tag))
                              .ToList()
                    )
                    .HasColumnType("nvarchar(max)")
                    .Metadata.SetValueComparer(new ValueComparer<ICollection<ClassificationTag>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            });
        }
    }
}
