using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace Elevator.Web;

public static class TestSeeding
{
    public static async Task<bool> TestDatabaseConnectionAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ElevatorDbContext>();
            
            // Test database connection
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                Console.WriteLine("Cannot connect to database");
                return false;
            }
            
            Console.WriteLine("Database connection successful");
            
            // Check if tables exist and have data
            var userCount = await context.Users.CountAsync();
            var compoundCount = await context.Compounds.CountAsync();
            var plantCount = await context.Plants.CountAsync();
            var protocolCount = await context.Protocols.CountAsync();
            var discussionCount = await context.Discussions.CountAsync();
            var ratingCount = await context.Ratings.CountAsync();
            
            Console.WriteLine($"Database statistics:");
            Console.WriteLine($"- Users: {userCount}");
            Console.WriteLine($"- Compounds: {compoundCount}");
            Console.WriteLine($"- Plants: {plantCount}");
            Console.WriteLine($"- Protocols: {protocolCount}");
            Console.WriteLine($"- Discussions: {discussionCount}");
            Console.WriteLine($"- Ratings: {ratingCount}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database test failed: {ex.Message}");
            return false;
        }
    }
}