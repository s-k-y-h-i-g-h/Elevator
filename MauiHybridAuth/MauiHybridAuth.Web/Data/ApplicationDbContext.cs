using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Compound> Compounds { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Formulation> Formulations { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPC (Table Per Concrete) inheritance
            modelBuilder.Entity<Intervention>()
                .UseTpcMappingStrategy();

            modelBuilder.Entity<Substance>()
                .UseTpcMappingStrategy();

            // Configure classification tags as JSON for enum collection
            modelBuilder.Entity<Substance>()
                .Property(s => s.ClassificationTags)
                .HasConversion(
                    v => string.Join(',', v.Select(e => e.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => Enum.Parse<ClassificationTag>(e))
                        .ToList()
                )
                .Metadata.SetValueComparer(
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<ClassificationTag>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );

            // Configure tables for the concrete classes
            modelBuilder.Entity<Compound>();
            modelBuilder.Entity<Plant>();
            modelBuilder.Entity<Formulation>();

            // Configure many-to-many relationship between Plants and Compounds (constituents)
            modelBuilder.Entity<Plant>()
                .HasMany(p => p.Constituents)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "PlantConstituent",
                    j => j
                        .HasOne<Compound>()
                        .WithMany()
                        .HasForeignKey("CompoundId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Plant>()
                        .WithMany()
                        .HasForeignKey("PlantId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

            // Configure many-to-many relationship between Formulations and Compounds (constituents)
            modelBuilder.Entity<Formulation>()
                .HasMany(f => f.Constituents)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FormulationConstituent",
                    j => j
                        .HasOne<Compound>()
                        .WithMany()
                        .HasForeignKey("CompoundId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Formulation>()
                        .WithMany()
                        .HasForeignKey("FormulationId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

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
