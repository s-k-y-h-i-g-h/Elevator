using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MauiHybridAuth.Web.Data;

namespace MauiHybridAuth.Web.Services;

public static class DataSeederCommands
{
    public static async Task SeedDataCommand(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await dataSeeder.SeedDataAsync();
    }

    public static async Task ClearDataCommand(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

        logger.LogInformation("Clearing all test data...");

        // Clear intervention ratings
        var ratings = await dbContext.InterventionRatings.ToListAsync();
        dbContext.InterventionRatings.RemoveRange(ratings);

        // Clear compounds
        var compounds = await dbContext.Compounds.ToListAsync();
        dbContext.Compounds.RemoveRange(compounds);

        // Clear test users
        var testEmails = new[]
        {
            "john.doe@test.com",
            "jane.smith@test.com", 
            "bob.wilson@test.com",
            "alice.johnson@test.com",
            "charlie.brown@test.com"
        };

        foreach (var email in testEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await userManager.DeleteAsync(user);
                logger.LogInformation("Deleted test user: {Email}", email);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("All test data cleared successfully.");
    }

    public static async Task ShowDataCommand(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

        var compounds = await dbContext.Compounds
            .Include(c => c.InterventionRatings)
            .ToListAsync();

        var users = await dbContext.Set<ApplicationUser>().ToListAsync();

        logger.LogInformation("=== Current Data Summary ===");
        logger.LogInformation("Users: {UserCount}", users.Count);
        logger.LogInformation("Compounds: {CompoundCount}", compounds.Count);
        logger.LogInformation("Total Ratings: {RatingCount}", compounds.Sum(c => c.InterventionRatings.Count));

        foreach (var compound in compounds)
        {
            logger.LogInformation("Compound: {Name} - {RatingCount} ratings, Avg: {AverageRating:F1}", 
                compound.Name, 
                compound.InterventionRatings.Count, 
                compound.CalculateAverageRating());
        }
    }
} 