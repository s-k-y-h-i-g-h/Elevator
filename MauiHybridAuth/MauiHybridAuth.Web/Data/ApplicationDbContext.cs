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
        }
    }
}
