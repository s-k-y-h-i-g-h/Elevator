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
            
            // Seed categories and subcategories
            var categories = await SeedCategoriesAsync();
            
            // Seed compounds
            var compounds = await SeedCompoundsAsync();
            
            // Assign categories to compounds
            await AssignCategoriesToCompoundsAsync(compounds, categories);
            
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

    private async Task<List<Category>> SeedCategoriesAsync()
    {
        var categories = new List<Category>();
        var random = new Random();

        // Create main categories
        var mainCategories = new[]
        {
            new { Name = "Pain Management", Subcategories = new[] { "Analgesics", "Anti-inflammatories", "Muscle Relaxants", "Topical Pain Relief" } },
            new { Name = "Cardiovascular", Subcategories = new[] { "Blood Pressure", "Cholesterol", "Heart Disease", "Blood Thinners" } },
            new { Name = "Diabetes", Subcategories = new[] { "Blood Sugar Control", "Insulin", "Oral Medications", "Monitoring" } },
            new { Name = "Mental Health", Subcategories = new[] { "Antidepressants", "Anxiety", "Mood Stabilizers", "Sleep Aids" } },
            new { Name = "Gastrointestinal", Subcategories = new[] { "Acid Reflux", "Ulcers", "Digestive Health", "Anti-nausea" } }
        };

        foreach (var mainCat in mainCategories)
        {
            var existingMainCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == mainCat.Name && c.ParentCategoryId == null);
            
            Category mainCategory;
            if (existingMainCategory == null)
            {
                mainCategory = new Category { Name = mainCat.Name };
                _context.Categories.Add(mainCategory);
                await _context.SaveChangesAsync(); // Save to get the ID
                _logger.LogInformation("Created main category: {Name}", mainCat.Name);
            }
            else
            {
                mainCategory = existingMainCategory;
            }

            categories.Add(mainCategory);

            // Create subcategories
            foreach (var subCatName in mainCat.Subcategories)
            {
                var existingSubCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == subCatName && c.ParentCategoryId == mainCategory.Id);
                
                if (existingSubCategory == null)
                {
                    var subCategory = new Category 
                    { 
                        Name = subCatName, 
                        ParentCategoryId = mainCategory.Id 
                    };
                    _context.Categories.Add(subCategory);
                    _logger.LogInformation("Created subcategory: {Name} under {Parent}", subCatName, mainCat.Name);
                }
                else
                {
                    categories.Add(existingSubCategory);
                }
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Categories.Include(c => c.Subcategories).ToListAsync();
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

    private async Task AssignCategoriesToCompoundsAsync(List<Compound> compounds, List<Category> categories)
    {
        var random = new Random();
        var interventionCategories = new List<InterventionCategory>();

        // Get all subcategories (categories with parent)
        var subcategories = categories.Where(c => c.ParentCategoryId.HasValue).ToList();

        foreach (var compound in compounds)
        {
            // Assign 1-5 random subcategories to each compound
            var numCategories = random.Next(1, 6);
            var selectedCategories = subcategories.OrderBy(x => random.Next()).Take(numCategories).ToList();

            foreach (var category in selectedCategories)
            {
                // Check if assignment already exists
                var existingAssignment = await _context.InterventionCategories
                    .FirstOrDefaultAsync(ic => ic.InterventionId == compound.Id && ic.CategoryId == category.Id);

                if (existingAssignment == null)
                {
                    var interventionCategory = new InterventionCategory
                    {
                        InterventionId = compound.Id,
                        CategoryId = category.Id
                    };

                    interventionCategories.Add(interventionCategory);
                    _logger.LogInformation("Assigned category {Category} to compound {Compound}", 
                        category.Name, compound.Name);
                }
            }
        }

        if (interventionCategories.Any())
        {
            _context.InterventionCategories.AddRange(interventionCategories);
            await _context.SaveChangesAsync();
        }
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