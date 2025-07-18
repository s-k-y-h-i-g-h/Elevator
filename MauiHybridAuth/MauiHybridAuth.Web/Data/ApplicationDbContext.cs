using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Shared.Models;

namespace MauiHybridAuth.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Compound> Compounds { get; set; }

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
        }
    }
}
