using MauiHybridAuth.Shared.Models;
using MauiHybridAuth.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Services;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding...");

            // Seed users
            var users = await SeedUsersAsync();
            
            // Seed compounds
            var compounds = await SeedCompoundsAsync();
            
            // Seed intervention ratings
            await SeedInterventionRatingsAsync(users, compounds);

            _logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }

    private async Task<List<ApplicationUser>> SeedUsersAsync()
    {
        var users = new List<ApplicationUser>();
        var testUsers = new[]
        {
            new { Email = "john.doe@test.com", UserName = "john.doe", Name = "John Doe" },
            new { Email = "jane.smith@test.com", UserName = "jane.smith", Name = "Jane Smith" },
            new { Email = "bob.wilson@test.com", UserName = "bob.wilson", Name = "Bob Wilson" },
            new { Email = "alice.johnson@test.com", UserName = "alice.johnson", Name = "Alice Johnson" },
            new { Email = "charlie.brown@test.com", UserName = "charlie.brown", Name = "Charlie Brown" }
        };

        foreach (var userInfo in testUsers)
        {
            var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userInfo.UserName,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0
                };

                var result = await _userManager.CreateAsync(user, "Test123!");
                if (result.Succeeded)
                {
                    users.Add(user);
                    _logger.LogInformation("Created test user: {Email}", userInfo.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to create user {Email}: {Errors}", 
                        userInfo.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                users.Add(existingUser);
            }
        }

        return users;
    }

    private async Task<List<Compound>> SeedCompoundsAsync()
    {
        var compounds = new List<Compound>
        {
            new Compound { Name = "Aspirin", Description = "Common pain reliever and anti-inflammatory medication" },
            new Compound { Name = "Ibuprofen", Description = "Nonsteroidal anti-inflammatory drug (NSAID)" },
            new Compound { Name = "Acetaminophen", Description = "Pain reliever and fever reducer" },
            new Compound { Name = "Lisinopril", Description = "ACE inhibitor used to treat high blood pressure" },
            new Compound { Name = "Metformin", Description = "Oral diabetes medicine that helps control blood sugar" },
            new Compound { Name = "Atorvastatin", Description = "Statin medication used to lower cholesterol" },
            new Compound { Name = "Omeprazole", Description = "Proton pump inhibitor for acid reflux" },
            new Compound { Name = "Amlodipine", Description = "Calcium channel blocker for blood pressure" },
            new Compound { Name = "Losartan", Description = "Angiotensin II receptor blocker" },
            new Compound { Name = "Sertraline", Description = "Selective serotonin reuptake inhibitor (SSRI)" }
        };

        foreach (var compound in compounds)
        {
            var existingCompound = await _context.Compounds
                .FirstOrDefaultAsync(c => c.Name == compound.Name);
            
            if (existingCompound == null)
            {
                _context.Compounds.Add(compound);
                _logger.LogInformation("Added compound: {Name}", compound.Name);
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Compounds.ToListAsync();
    }

    private async Task SeedInterventionRatingsAsync(List<ApplicationUser> users, List<Compound> compounds)
    {
        var random = new Random();
        var ratings = new List<InterventionRating>();

        // Create ratings for each compound by different users
        foreach (var compound in compounds)
        {
            // Each compound gets ratings from 2-4 random users
            var numRatings = random.Next(2, 5);
            var selectedUsers = users.OrderBy(x => random.Next()).Take(numRatings).ToList();

            foreach (var user in selectedUsers)
            {
                // Check if rating already exists
                var existingRating = await _context.InterventionRatings
                    .FirstOrDefaultAsync(ir => ir.InterventionId == compound.Id && ir.ApplicationUserId == user.Id);

                if (existingRating == null)
                {
                    var rating = new InterventionRating
                    {
                        InterventionId = compound.Id,
                        ApplicationUserId = user.Id,
                        Rating = random.Next(1, 6) // Random rating from 1-5
                    };

                    ratings.Add(rating);
                    _logger.LogInformation("Added rating {Rating} for compound {Compound} by user {User}", 
                        rating.Rating, compound.Name, user.UserName);
                }
            }
        }

        if (ratings.Any())
        {
            _context.InterventionRatings.AddRange(ratings);
            await _context.SaveChangesAsync();
        }
    }
} 