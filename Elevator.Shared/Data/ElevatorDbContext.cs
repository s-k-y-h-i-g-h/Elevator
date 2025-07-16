using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Models.Protocols;
using Elevator.Shared.Models.Discussion;
using Elevator.Shared.Models.Ratings;

namespace Elevator.Shared.Data;

/// <summary>
/// Entity Framework DbContext for the Elevator application
/// </summary>
public class ElevatorDbContext : IdentityDbContext<ApplicationUser>
{
    public ElevatorDbContext(DbContextOptions<ElevatorDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<Intervention> Interventions { get; set; }
    public DbSet<Compound> Compounds { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Protocol> Protocols { get; set; }
    public DbSet<ProtocolIntervention> ProtocolInterventions { get; set; }
    public DbSet<Models.Discussion.Discussion> Discussions { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Important for Identity

        // Configure Table Per Class (TPC) inheritance for Intervention hierarchy
        modelBuilder.Entity<Intervention>().UseTpcMappingStrategy();
        modelBuilder.Entity<Substance>().UseTpcMappingStrategy();

        // Configure many-to-many relationship for Plant constituents
        modelBuilder.Entity<Plant>()
            .HasMany(p => p.Constituents)
            .WithMany()
            .UsingEntity(j => j.ToTable("PlantConstituents"));

        // Configure Protocol relationships
        modelBuilder.Entity<Protocol>()
            .HasOne(p => p.User)
            .WithMany(u => u.Protocols)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProtocolIntervention>()
            .HasOne(pi => pi.Protocol)
            .WithMany(p => p.ProtocolInterventions)
            .HasForeignKey(pi => pi.ProtocolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProtocolIntervention>()
            .HasOne(pi => pi.Intervention)
            .WithMany(i => i.ProtocolInterventions)
            .HasForeignKey(pi => pi.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Discussion relationships
        modelBuilder.Entity<Models.Discussion.Discussion>()
            .HasOne(d => d.User)
            .WithMany(u => u.Discussions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Models.Discussion.Discussion>()
            .HasOne(d => d.Intervention)
            .WithMany(i => i.Discussions)
            .HasForeignKey(d => d.InterventionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Models.Discussion.Discussion>()
            .HasOne(d => d.Protocol)
            .WithMany(p => p.Discussions)
            .HasForeignKey(d => d.ProtocolId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Comment relationships
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Discussion)
            .WithMany(d => d.Comments)
            .HasForeignKey(c => c.DiscussionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.ChildComments)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes for hierarchical comments

        // Configure Vote relationships
        modelBuilder.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany(u => u.Votes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Discussion)
            .WithMany(d => d.Votes)
            .HasForeignKey(v => v.DiscussionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Comment)
            .WithMany(c => c.Votes)
            .HasForeignKey(v => v.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Rating relationships
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Intervention)
            .WithMany(i => i.Ratings)
            .HasForeignKey(r => r.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Protocol)
            .WithMany(p => p.Ratings)
            .HasForeignKey(r => r.ProtocolId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraints
        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.UserId, r.InterventionId, r.ProtocolId })
            .IsUnique()
            .HasDatabaseName("IX_Rating_User_Intervention_Protocol");

        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.UserId, v.DiscussionId, v.CommentId })
            .IsUnique()
            .HasDatabaseName("IX_Vote_User_Discussion_Comment");

        // Configure check constraints for Rating value
        modelBuilder.Entity<Rating>()
            .ToTable(t => t.HasCheckConstraint("CK_Rating_Value", "[Value] >= 0.0 AND [Value] <= 5.0"));

        // Ensure either InterventionId or ProtocolId is set for Rating, but not both
        modelBuilder.Entity<Rating>()
            .ToTable(t => t.HasCheckConstraint("CK_Rating_Target", 
                "([InterventionId] IS NOT NULL AND [ProtocolId] IS NULL) OR ([InterventionId] IS NULL AND [ProtocolId] IS NOT NULL)"));

        // Ensure either DiscussionId or CommentId is set for Vote, but not both
        modelBuilder.Entity<Vote>()
            .ToTable(t => t.HasCheckConstraint("CK_Vote_Target", 
                "([DiscussionId] IS NOT NULL AND [CommentId] IS NULL) OR ([DiscussionId] IS NULL AND [CommentId] IS NOT NULL)"));

        // Ensure either InterventionId or ProtocolId is set for Discussion, but not both (optional)
        modelBuilder.Entity<Models.Discussion.Discussion>()
            .ToTable(t => t.HasCheckConstraint("CK_Discussion_Target", 
                "([InterventionId] IS NULL AND [ProtocolId] IS NULL) OR ([InterventionId] IS NOT NULL AND [ProtocolId] IS NULL) OR ([InterventionId] IS NULL AND [ProtocolId] IS NOT NULL)"));
    }
}