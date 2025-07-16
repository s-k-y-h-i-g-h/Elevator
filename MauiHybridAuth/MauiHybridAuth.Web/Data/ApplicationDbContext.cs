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
        public DbSet<Device> Devices { get; set; }
        public DbSet<InterventionRating> InterventionRatings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<InterventionCategory> InterventionCategories { get; set; }
        public DbSet<Protocol> Protocols { get; set; }
        public DbSet<InterventionProtocol> InterventionProtocols { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }

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

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Device");
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

            // Configure Protocol relationships
            modelBuilder.Entity<Protocol>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure InterventionProtocol relationships
            modelBuilder.Entity<InterventionProtocol>(entity =>
            {
                entity.HasKey(ip => ip.Id);
                
                entity.HasOne(ip => ip.Intervention)
                    .WithMany(i => i.InterventionProtocols)
                    .HasForeignKey(ip => ip.InterventionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ip => ip.Protocol)
                    .WithMany(p => p.InterventionProtocols)
                    .HasForeignKey(ip => ip.ProtocolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Discussion relationships
            modelBuilder.Entity<Discussion>(entity =>
            {
                entity.HasOne(d => d.Intervention)
                    .WithMany(i => i.Discussions)
                    .HasForeignKey(d => d.InterventionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(d => d.Protocol)
                    .WithMany(p => p.Discussions)
                    .HasForeignKey(d => d.ProtocolId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Comment relationships
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Discussion)
                    .WithMany(d => d.Comments)
                    .HasForeignKey(c => c.DiscussionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of parent comments
                
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Vote relationships
            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasOne(v => v.Discussion)
                    .WithMany(d => d.Votes)
                    .HasForeignKey(v => v.DiscussionId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne(v => v.Comment)
                    .WithMany(c => c.Votes)
                    .HasForeignKey(v => v.CommentId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Ensure a user can only vote once per discussion or comment
                entity.HasIndex(v => new { v.UserId, v.DiscussionId })
                    .IsUnique()
                    .HasFilter("[DiscussionId] IS NOT NULL");
                
                entity.HasIndex(v => new { v.UserId, v.CommentId })
                    .IsUnique()
                    .HasFilter("[CommentId] IS NOT NULL");
            });
        }
    }
}
