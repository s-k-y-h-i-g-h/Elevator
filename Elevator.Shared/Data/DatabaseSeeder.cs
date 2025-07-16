using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Models.Interventions;
using Elevator.Shared.Models.Protocols;
using Elevator.Shared.Models.Discussion;
using Elevator.Shared.Models.Ratings;

namespace Elevator.Shared.Data;

/// <summary>
/// Database seeder for creating sample data during development
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ElevatorDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ElevatorDbContext>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping seeding.");
                return;
            }

            logger.LogInformation("Starting database seeding...");

            // Seed Users
            var users = await SeedUsersAsync(userManager, logger);

            // Seed Interventions (Compounds and Plants)
            var interventions = await SeedInterventionsAsync(context, logger);

            // Seed Protocols
            var protocols = await SeedProtocolsAsync(context, users, interventions, logger);

            // Seed Discussions
            var discussions = await SeedDiscussionsAsync(context, users, interventions, protocols, logger);

            // Seed Ratings
            await SeedRatingsAsync(context, users, interventions, protocols, logger);

            // Seed Comments and Votes
            await SeedCommentsAndVotesAsync(context, users, discussions, logger);

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task<List<ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        logger.LogInformation("Seeding users...");

        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                UserName = "admin@elevator.com",
                Email = "admin@elevator.com",
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            },
            new ApplicationUser
            {
                UserName = "john.doe@example.com",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            },
            new ApplicationUser
            {
                UserName = "jane.smith@example.com",
                Email = "jane.smith@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            },
            new ApplicationUser
            {
                UserName = "biohacker@example.com",
                Email = "biohacker@example.com",
                FirstName = "Bio",
                LastName = "Hacker",
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            }
        };

        foreach (var user in users)
        {
            var result = await userManager.CreateAsync(user, "Password123!");
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create user {Email}: {Errors}", user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        return users;
    }

    private static async Task<List<Intervention>> SeedInterventionsAsync(ElevatorDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding interventions...");

        var compounds = new List<Compound>
        {
            new Compound
            {
                Name = "Modafinil",
                Description = "A wakefulness-promoting agent used to treat narcolepsy and other sleep disorders. Popular in biohacking for cognitive enhancement.",
                Duration = "12-15 hours",
                DoseRange = "100-200mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Compound
            {
                Name = "L-Theanine",
                Description = "An amino acid found in tea leaves that promotes relaxation without drowsiness. Often combined with caffeine for focus.",
                Duration = "4-6 hours",
                DoseRange = "100-400mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Compound
            {
                Name = "Noopept",
                Description = "A synthetic nootropic compound that may enhance cognitive function and memory formation.",
                Duration = "2-4 hours",
                DoseRange = "10-30mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Compound
            {
                Name = "Alpha-GPC",
                Description = "A choline compound that supports cognitive function and may enhance memory and learning.",
                Duration = "4-6 hours",
                DoseRange = "300-600mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Compound
            {
                Name = "Phenylpiracetam",
                Description = "A racetam nootropic with stimulant properties that may enhance cognitive performance and physical endurance.",
                Duration = "4-6 hours",
                DoseRange = "100-200mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var plants = new List<Plant>
        {
            new Plant
            {
                Name = "Bacopa Monnieri",
                Description = "An adaptogenic herb traditionally used in Ayurvedic medicine for cognitive enhancement and stress reduction.",
                ScientificName = "Bacopa monnieri",
                CommonNames = "Brahmi, Water Hyssop",
                TraditionalUses = "Memory enhancement, anxiety reduction, cognitive support",
                Duration = "6-8 hours",
                DoseRange = "300-600mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Plant
            {
                Name = "Lion's Mane Mushroom",
                Description = "A medicinal mushroom known for its potential neuroprotective and cognitive-enhancing properties.",
                ScientificName = "Hericium erinaceus",
                CommonNames = "Lion's Mane, Bearded Tooth Mushroom",
                TraditionalUses = "Cognitive support, nerve health, digestive health",
                Duration = "Variable",
                DoseRange = "500-1000mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Plant
            {
                Name = "Rhodiola Rosea",
                Description = "An adaptogenic herb that may help reduce fatigue and improve mental performance under stress.",
                ScientificName = "Rhodiola rosea",
                CommonNames = "Golden Root, Arctic Root",
                TraditionalUses = "Stress adaptation, fatigue reduction, mood support",
                Duration = "4-6 hours",
                DoseRange = "200-400mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Plant
            {
                Name = "Ginkgo Biloba",
                Description = "A tree extract traditionally used to support cognitive function and circulation.",
                ScientificName = "Ginkgo biloba",
                CommonNames = "Maidenhair Tree",
                TraditionalUses = "Memory support, circulation, cognitive function",
                Duration = "4-6 hours",
                DoseRange = "120-240mg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Compounds.AddRange(compounds);
        context.Plants.AddRange(plants);
        await context.SaveChangesAsync();

        // Set up plant constituents relationships
        var lTheanine = compounds.First(c => c.Name == "L-Theanine");
        var alphaGpc = compounds.First(c => c.Name == "Alpha-GPC");
        
        var lionsMane = plants.First(p => p.Name == "Lion's Mane Mushroom");
        var bacopa = plants.First(p => p.Name == "Bacopa Monnieri");

        // Add some constituent relationships (simplified for demo)
        lionsMane.Constituents = new List<Compound> { alphaGpc };
        bacopa.Constituents = new List<Compound> { lTheanine };

        await context.SaveChangesAsync();

        var allInterventions = new List<Intervention>();
        allInterventions.AddRange(compounds);
        allInterventions.AddRange(plants);

        return allInterventions;
    }

    private static async Task<List<Protocol>> SeedProtocolsAsync(ElevatorDbContext context, List<ApplicationUser> users, List<Intervention> interventions, ILogger logger)
    {
        logger.LogInformation("Seeding protocols...");

        var protocols = new List<Protocol>
        {
            new Protocol
            {
                Name = "Morning Focus Stack",
                Description = "A combination of nootropics for enhanced morning focus and productivity.",
                UserId = users[1].Id, // John Doe
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Protocol
            {
                Name = "Stress Management Protocol",
                Description = "Adaptogenic herbs and compounds for managing daily stress and anxiety.",
                UserId = users[2].Id, // Jane Smith
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Protocol
            {
                Name = "Cognitive Enhancement Stack",
                Description = "Advanced nootropic combination for memory and learning enhancement.",
                UserId = users[3].Id, // Bio Hacker
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Protocol
            {
                Name = "Personal Experimentation",
                Description = "My personal protocol for testing various compounds.",
                UserId = users[1].Id, // John Doe
                IsPublic = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Protocols.AddRange(protocols);
        await context.SaveChangesAsync();

        // Add protocol interventions
        var protocolInterventions = new List<ProtocolIntervention>
        {
            // Morning Focus Stack
            new ProtocolIntervention
            {
                ProtocolId = protocols[0].Id,
                InterventionId = interventions.First(i => i.Name == "Modafinil").Id,
                Dosage = "100mg",
                Schedule = "Once daily in the morning",
                Notes = "Take on empty stomach for best absorption"
            },
            new ProtocolIntervention
            {
                ProtocolId = protocols[0].Id,
                InterventionId = interventions.First(i => i.Name == "L-Theanine").Id,
                Dosage = "200mg",
                Schedule = "With morning coffee",
                Notes = "Helps smooth out caffeine jitters"
            },
            
            // Stress Management Protocol
            new ProtocolIntervention
            {
                ProtocolId = protocols[1].Id,
                InterventionId = interventions.First(i => i.Name == "Rhodiola Rosea").Id,
                Dosage = "300mg",
                Schedule = "Twice daily with meals",
                Notes = "Take consistently for best adaptogenic effects"
            },
            new ProtocolIntervention
            {
                ProtocolId = protocols[1].Id,
                InterventionId = interventions.First(i => i.Name == "Bacopa Monnieri").Id,
                Dosage = "500mg",
                Schedule = "Once daily with dinner",
                Notes = "May take 4-6 weeks to see full effects"
            },
            
            // Cognitive Enhancement Stack
            new ProtocolIntervention
            {
                ProtocolId = protocols[2].Id,
                InterventionId = interventions.First(i => i.Name == "Noopept").Id,
                Dosage = "20mg",
                Schedule = "Twice daily",
                Notes = "Cycle 5 days on, 2 days off"
            },
            new ProtocolIntervention
            {
                ProtocolId = protocols[2].Id,
                InterventionId = interventions.First(i => i.Name == "Alpha-GPC").Id,
                Dosage = "400mg",
                Schedule = "Once daily with Noopept",
                Notes = "Provides choline for enhanced effects"
            },
            new ProtocolIntervention
            {
                ProtocolId = protocols[2].Id,
                InterventionId = interventions.First(i => i.Name == "Lion's Mane Mushroom").Id,
                Dosage = "750mg",
                Schedule = "Daily with breakfast",
                Notes = "Long-term neuroprotective benefits"
            }
        };

        context.ProtocolInterventions.AddRange(protocolInterventions);
        await context.SaveChangesAsync();

        return protocols;
    }

    private static async Task<List<Models.Discussion.Discussion>> SeedDiscussionsAsync(ElevatorDbContext context, List<ApplicationUser> users, List<Intervention> interventions, List<Protocol> protocols, ILogger logger)
    {
        logger.LogInformation("Seeding discussions...");

        var discussions = new List<Models.Discussion.Discussion>
        {
            new Models.Discussion.Discussion
            {
                Title = "Modafinil Dosage and Timing",
                Content = "What's everyone's experience with Modafinil dosing? I've been taking 200mg but wondering if 100mg might be sufficient for daily use.",
                UserId = users[1].Id,
                InterventionId = interventions.First(i => i.Name == "Modafinil").Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.Discussion.Discussion
            {
                Title = "L-Theanine and Caffeine Synergy",
                Content = "The combination of L-Theanine with caffeine has been a game-changer for my focus. Anyone else notice significant improvements?",
                UserId = users[2].Id,
                InterventionId = interventions.First(i => i.Name == "L-Theanine").Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.Discussion.Discussion
            {
                Title = "Lion's Mane Long-term Effects",
                Content = "I've been taking Lion's Mane for 6 months now. Here are my observations on cognitive improvements and neurogenesis effects.",
                UserId = users[3].Id,
                InterventionId = interventions.First(i => i.Name == "Lion's Mane Mushroom").Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.Discussion.Discussion
            {
                Title = "Morning Focus Stack Results",
                Content = "After 3 months on this protocol, I've seen significant improvements in morning productivity and sustained focus throughout the day.",
                UserId = users[1].Id,
                ProtocolId = protocols.First(p => p.Name == "Morning Focus Stack").Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.Discussion.Discussion
            {
                Title = "Stress Management Protocol Feedback",
                Content = "This adaptogenic combination has really helped with my daily stress levels. The Rhodiola seems to be the key component.",
                UserId = users[0].Id,
                ProtocolId = protocols.First(p => p.Name == "Stress Management Protocol").Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Discussions.AddRange(discussions);
        await context.SaveChangesAsync();

        return discussions;
    }

    private static async Task SeedRatingsAsync(ElevatorDbContext context, List<ApplicationUser> users, List<Intervention> interventions, List<Protocol> protocols, ILogger logger)
    {
        logger.LogInformation("Seeding ratings...");

        var ratings = new List<Rating>
        {
            // Intervention ratings
            new Rating { UserId = users[1].Id, InterventionId = interventions.First(i => i.Name == "Modafinil").Id, Value = 4.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[2].Id, InterventionId = interventions.First(i => i.Name == "Modafinil").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[3].Id, InterventionId = interventions.First(i => i.Name == "Modafinil").Id, Value = 5.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            new Rating { UserId = users[1].Id, InterventionId = interventions.First(i => i.Name == "L-Theanine").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[2].Id, InterventionId = interventions.First(i => i.Name == "L-Theanine").Id, Value = 4.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[0].Id, InterventionId = interventions.First(i => i.Name == "L-Theanine").Id, Value = 3.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            new Rating { UserId = users[2].Id, InterventionId = interventions.First(i => i.Name == "Lion's Mane Mushroom").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[3].Id, InterventionId = interventions.First(i => i.Name == "Lion's Mane Mushroom").Id, Value = 4.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            new Rating { UserId = users[1].Id, InterventionId = interventions.First(i => i.Name == "Bacopa Monnieri").Id, Value = 3.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[2].Id, InterventionId = interventions.First(i => i.Name == "Bacopa Monnieri").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // Protocol ratings
            new Rating { UserId = users[0].Id, ProtocolId = protocols.First(p => p.Name == "Morning Focus Stack").Id, Value = 4.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[2].Id, ProtocolId = protocols.First(p => p.Name == "Morning Focus Stack").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[3].Id, ProtocolId = protocols.First(p => p.Name == "Morning Focus Stack").Id, Value = 5.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            new Rating { UserId = users[0].Id, ProtocolId = protocols.First(p => p.Name == "Stress Management Protocol").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[1].Id, ProtocolId = protocols.First(p => p.Name == "Stress Management Protocol").Id, Value = 3.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            new Rating { UserId = users[1].Id, ProtocolId = protocols.First(p => p.Name == "Cognitive Enhancement Stack").Id, Value = 4.5m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Rating { UserId = users[2].Id, ProtocolId = protocols.First(p => p.Name == "Cognitive Enhancement Stack").Id, Value = 4.0m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        context.Ratings.AddRange(ratings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCommentsAndVotesAsync(ElevatorDbContext context, List<ApplicationUser> users, List<Models.Discussion.Discussion> discussions, ILogger logger)
    {
        logger.LogInformation("Seeding comments and votes...");

        var comments = new List<Comment>
        {
            new Comment
            {
                Content = "I've found 100mg to be perfect for daily use. 200mg can be too stimulating for regular consumption.",
                UserId = users[2].Id,
                DiscussionId = discussions[0].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Comment
            {
                Content = "Agreed! I also cycle it - 3 days on, 1 day off to prevent tolerance.",
                UserId = users[3].Id,
                DiscussionId = discussions[0].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Comment
            {
                Content = "The synergy is real! I use a 2:1 ratio - 200mg L-Theanine with 100mg caffeine.",
                UserId = users[0].Id,
                DiscussionId = discussions[1].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Comment
            {
                Content = "Great results! I've been considering trying this stack. Any side effects?",
                UserId = users[2].Id,
                DiscussionId = discussions[3].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Comments.AddRange(comments);
        await context.SaveChangesAsync();

        // Add some nested comments
        var nestedComments = new List<Comment>
        {
            new Comment
            {
                Content = "That's a good cycling strategy. Do you notice any withdrawal effects on off days?",
                UserId = users[0].Id,
                DiscussionId = discussions[0].Id,
                ParentCommentId = comments[1].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Comment
            {
                Content = "No significant side effects for me. Just make sure to take it early in the day to avoid sleep issues.",
                UserId = users[1].Id,
                DiscussionId = discussions[3].Id,
                ParentCommentId = comments[3].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Comments.AddRange(nestedComments);
        await context.SaveChangesAsync();

        // Add votes for discussions and comments
        var votes = new List<Vote>
        {
            // Discussion votes
            new Vote { UserId = users[0].Id, DiscussionId = discussions[0].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[2].Id, DiscussionId = discussions[0].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[3].Id, DiscussionId = discussions[1].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[1].Id, DiscussionId = discussions[2].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            
            // Comment votes
            new Vote { UserId = users[1].Id, CommentId = comments[0].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[0].Id, CommentId = comments[0].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[2].Id, CommentId = comments[1].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[1].Id, CommentId = comments[2].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow },
            new Vote { UserId = users[3].Id, CommentId = comments[2].Id, IsUpvote = true, CreatedAt = DateTime.UtcNow }
        };

        context.Votes.AddRange(votes);
        await context.SaveChangesAsync();
    }
}