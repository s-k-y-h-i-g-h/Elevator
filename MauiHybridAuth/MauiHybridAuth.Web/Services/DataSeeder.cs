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
            
            // Seed all intervention types
            var compounds = await SeedCompoundsAsync();
            var dietaries = await SeedDietariesAsync();
            var behaviorals = await SeedBehavioralsAsync();
            var augmentations = await SeedAugmentationsAsync();
            var physiologicals = await SeedPhysiologicalsAsync();
            var wearables = await SeedWearablesAsync();
            
            // Combine all interventions for category assignment and ratings
            var allInterventions = compounds.Cast<Intervention>()
                .Concat(dietaries.Cast<Intervention>())
                .Concat(behaviorals.Cast<Intervention>())
                .Concat(augmentations.Cast<Intervention>())
                .Concat(physiologicals.Cast<Intervention>())
                .Concat(wearables.Cast<Intervention>())
                .ToList();
            
            // Assign categories to all interventions
            await AssignCategoriesToInterventionsAsync(allInterventions, categories);
            
            // Seed intervention ratings for all interventions
            await SeedInterventionRatingsAsync(users, allInterventions);

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
            new { Name = "Health", Subcategories = new[] { "Sleep Quality", "Digestion", "Immune System", "Emotional Regulation" } },
            new { Name = "Longevity", Subcategories = new[] { "Telomeres", "NAD+", "Mitochondria", "Inflammation" } },
            new { Name = "Disease", Subcategories = new[] { "Schizophrenia", "Depression", "Cancer", "Cardiovascular Disease (CVD)" } },
            new { Name = "Neuroenhancement", Subcategories = new[] { "Cognitive", "Social", "Mood", "Motivation" } },
            new { Name = "Body Enhancement", Subcategories = new[] { "Performance", "Muscle", "Diagnostic", "Sensory" } }
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
        var random = new Random();
        var compounds = new List<Compound>
        {
            new Compound { Name = "Aspirin", Description = "Common pain reliever and anti-inflammatory medication" },
            new Compound { Name = "Piracetam", Description = "Nootropic drug that improves memory and cognitive function" },
            new Compound { Name = "L-Carnosine", Description = "Antioxidant that protects against oxidative stress" },
            new Compound { Name = "Lisinopril", Description = "ACE inhibitor used to treat high blood pressure" },
            new Compound { Name = "Metformin", Description = "Oral diabetes medicine that helps control blood sugar" },
            new Compound { Name = "Atorvastatin", Description = "Statin medication used to lower cholesterol" },
            new Compound { Name = "Omeprazole", Description = "Proton pump inhibitor for acid reflux" },
            new Compound { Name = "Amlodipine", Description = "Calcium channel blocker for blood pressure" },
            new Compound { Name = "Losartan", Description = "Angiotensin II receptor blocker" },
            new Compound { Name = "Sertraline", Description = "Selective serotonin reuptake inhibitor (SSRI)" }
        };

        // Get all available classification tags
        var allTags = Enum.GetValues<ClassificationTag>().ToList();

        foreach (var compound in compounds)
        {
            var existingCompound = await _context.Compounds
                .FirstOrDefaultAsync(c => c.Name == compound.Name);
            
            if (existingCompound == null)
            {
                // Generate random dose range in "X-XXXmg" format
                var minDose = random.Next(5, 51); // 5-50mg
                var maxDose = random.Next(minDose + 10, minDose + 101); // 10-100mg more than min
                compound.DoseRange = $"{minDose}-{maxDose}mg";
                
                // Generate random duration between 60-240 minutes
                compound.DurationInMinutes = random.Next(60, 241);
                
                // Assign 0-3 random classification tags
                var numTags = random.Next(0, 4); // 0, 1, 2, or 3 tags
                var selectedTags = allTags.OrderBy(x => random.Next()).Take(numTags).ToList();
                compound.ClassificationTags = selectedTags;
                
                _context.Compounds.Add(compound);
                _logger.LogInformation("Added compound: {Name} with dose range {DoseRange}, duration {Duration} minutes, and tags: {Tags}", 
                    compound.Name, compound.DoseRange, compound.DurationInMinutes, 
                    selectedTags.Any() ? string.Join(", ", selectedTags) : "None");
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Compounds.ToListAsync();
    }

    private async Task<List<Dietary>> SeedDietariesAsync()
    {
        var random = new Random();
        var dietaries = new List<Dietary>
        {
            new Dietary { Name = "Mediterranean Diet", Description = "Heart-healthy diet rich in fruits, vegetables, whole grains, and healthy fats" },
            new Dietary { Name = "Ketogenic Diet", Description = "High-fat, low-carbohydrate diet that induces ketosis" },
            new Dietary { Name = "Intermittent Fasting", Description = "Cycling between periods of eating and fasting" },
            new Dietary { Name = "Plant-Based Diet", Description = "Diet focused on foods derived from plants" },
            new Dietary { Name = "Anti-Inflammatory Diet", Description = "Diet designed to reduce chronic inflammation" },
            new Dietary { Name = "DASH Diet", Description = "Dietary Approaches to Stop Hypertension" },
            new Dietary { Name = "Paleo Diet", Description = "Diet based on foods presumed to be available to Paleolithic humans" },
            new Dietary { Name = "Low FODMAP Diet", Description = "Diet low in fermentable oligosaccharides, disaccharides, monosaccharides, and polyols" }
        };

        foreach (var dietary in dietaries)
        {
            var existingDietary = await _context.Set<Dietary>()
                .FirstOrDefaultAsync(d => d.Name == dietary.Name);
            
            if (existingDietary == null)
            {
                _context.Set<Dietary>().Add(dietary);
                _logger.LogInformation("Added dietary intervention: {Name}", dietary.Name);
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Set<Dietary>().ToListAsync();
    }

    private async Task<List<Behavioral>> SeedBehavioralsAsync()
    {
        var random = new Random();
        var behaviorals = new List<Behavioral>
        {
            new Behavioral { Name = "Cognitive Behavioral Therapy", Description = "Psychotherapy that focuses on changing unhelpful cognitive distortions and behaviors" },
            new Behavioral { Name = "Mindfulness Meditation", Description = "Practice of focusing attention on the present moment" },
            new Behavioral { Name = "Exercise Therapy", Description = "Physical activity prescribed to improve health and fitness" },
            new Behavioral { Name = "Sleep Hygiene", Description = "Practices and habits that promote good sleep quality" },
            new Behavioral { Name = "Stress Management", Description = "Techniques and strategies to manage stress levels" },
            new Behavioral { Name = "Social Support Groups", Description = "Groups providing emotional and practical support" },
            new Behavioral { Name = "Biofeedback Training", Description = "Technique to control bodily processes through monitoring" },
            new Behavioral { Name = "Progressive Muscle Relaxation", Description = "Technique for reducing anxiety and stress through muscle relaxation" }
        };

        foreach (var behavioral in behaviorals)
        {
            var existingBehavioral = await _context.Set<Behavioral>()
                .FirstOrDefaultAsync(b => b.Name == behavioral.Name);
            
            if (existingBehavioral == null)
            {
                _context.Set<Behavioral>().Add(behavioral);
                _logger.LogInformation("Added behavioral intervention: {Name}", behavioral.Name);
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Set<Behavioral>().ToListAsync();
    }

    private async Task<List<Augmentation>> SeedAugmentationsAsync()
    {
        var random = new Random();
        var augmentations = new List<Augmentation>
        {
            new Augmentation { Name = "Transcranial Magnetic Stimulation", Description = "Non-invasive brain stimulation using magnetic fields" },
            new Augmentation { Name = "Deep Brain Stimulation", Description = "Surgical treatment involving implanted electrodes" },
            new Augmentation { Name = "Vagus Nerve Stimulation", Description = "Electrical stimulation of the vagus nerve" },
            new Augmentation { Name = "Cochlear Implant", Description = "Electronic device that provides hearing to deaf individuals" },
            new Augmentation { Name = "Prosthetic Limb", Description = "Artificial device that replaces a missing body part" },
            new Augmentation { Name = "Retinal Implant", Description = "Device that restores vision to blind individuals" },
            new Augmentation { Name = "Spinal Cord Stimulation", Description = "Electrical stimulation to treat chronic pain" },
            new Augmentation { Name = "Cardiac Pacemaker", Description = "Device that regulates heart rhythm" }
        };

        foreach (var augmentation in augmentations)
        {
            var existingAugmentation = await _context.Set<Augmentation>()
                .FirstOrDefaultAsync(a => a.Name == augmentation.Name);
            
            if (existingAugmentation == null)
            {
                _context.Set<Augmentation>().Add(augmentation);
                _logger.LogInformation("Added augmentation intervention: {Name}", augmentation.Name);
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Set<Augmentation>().ToListAsync();
    }

    private async Task<List<Physiological>> SeedPhysiologicalsAsync()
    {
        var random = new Random();
        var physiologicals = new List<Physiological>
        {
            new Physiological { Name = "Acupuncture", Description = "Traditional Chinese medicine technique using thin needles" },
            new Physiological { Name = "Massage Therapy", Description = "Manipulation of soft tissues to promote healing and relaxation" },
            new Physiological { Name = "Chiropractic Adjustment", Description = "Manual manipulation of the spine to improve alignment" },
            new Physiological { Name = "Osteopathic Manipulation", Description = "Hands-on treatment to improve mobility and function" },
            new Physiological { Name = "Physical Therapy", Description = "Treatment to restore movement and function" },
            new Physiological { Name = "Occupational Therapy", Description = "Therapy to improve daily living skills" },
            new Physiological { Name = "Respiratory Therapy", Description = "Treatment for breathing disorders" },
            new Physiological { Name = "Hydrotherapy", Description = "Use of water for pain relief and treatment" }
        };

        foreach (var physiological in physiologicals)
        {
            var existingPhysiological = await _context.Set<Physiological>()
                .FirstOrDefaultAsync(p => p.Name == physiological.Name);
            
            if (existingPhysiological == null)
            {
                _context.Set<Physiological>().Add(physiological);
                _logger.LogInformation("Added physiological intervention: {Name}", physiological.Name);
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Set<Physiological>().ToListAsync();
    }

    private async Task<List<Wearable>> SeedWearablesAsync()
    {
        var random = new Random();
        var wearables = new List<Wearable>
        {
            new Wearable { Name = "Fitness Tracker", Description = "Device that monitors physical activity and health metrics" },
            new Wearable { Name = "Smart Watch", Description = "Wearable computer with health monitoring capabilities" },
            new Wearable { Name = "Continuous Glucose Monitor", Description = "Device that tracks blood sugar levels continuously" },
            new Wearable { Name = "Heart Rate Monitor", Description = "Device that tracks heart rate and cardiovascular metrics" },
            new Wearable { Name = "Sleep Tracker", Description = "Device that monitors sleep patterns and quality" },
            new Wearable { Name = "Blood Pressure Monitor", Description = "Wearable device for continuous blood pressure monitoring" },
            new Wearable { Name = "ECG Monitor", Description = "Portable device for electrocardiogram monitoring" },
            new Wearable { Name = "Oxygen Saturation Monitor", Description = "Device that measures blood oxygen levels" }
        };

        foreach (var wearable in wearables)
        {
            var existingWearable = await _context.Set<Wearable>()
                .FirstOrDefaultAsync(w => w.Name == wearable.Name);
            
            if (existingWearable == null)
            {
                _context.Set<Wearable>().Add(wearable);
                _logger.LogInformation("Added wearable intervention: {Name}", wearable.Name);
            }
        }

        await _context.SaveChangesAsync();
        return await _context.Set<Wearable>().ToListAsync();
    }

    private async Task AssignCategoriesToInterventionsAsync(List<Intervention> interventions, List<Category> categories)
    {
        var random = new Random();
        var interventionCategories = new List<InterventionCategory>();

        // Get all subcategories (categories with parent)
        var subcategories = categories.Where(c => c.ParentCategoryId.HasValue).ToList();

        foreach (var intervention in interventions)
        {
            // Assign 1-3 random subcategories to each intervention
            var numCategories = random.Next(1, 4);
            var selectedCategories = subcategories.OrderBy(x => random.Next()).Take(numCategories).ToList();

            foreach (var category in selectedCategories)
            {
                // Check if assignment already exists
                var existingAssignment = await _context.InterventionCategories
                    .FirstOrDefaultAsync(ic => ic.InterventionId == intervention.Id && ic.CategoryId == category.Id);

                if (existingAssignment == null)
                {
                    var interventionCategory = new InterventionCategory
                    {
                        InterventionId = intervention.Id,
                        CategoryId = category.Id
                    };

                    interventionCategories.Add(interventionCategory);
                    _logger.LogInformation("Assigned category {Category} to intervention {Intervention}", 
                        category.Name, intervention.Name);
                }
            }
        }

        if (interventionCategories.Any())
        {
            _context.InterventionCategories.AddRange(interventionCategories);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedInterventionRatingsAsync(List<ApplicationUser> users, List<Intervention> interventions)
    {
        var random = new Random();
        var ratings = new List<InterventionRating>();

        // Create ratings for each intervention by different users
        foreach (var intervention in interventions)
        {
            // Each intervention gets ratings from 2-4 random users
            var numRatings = random.Next(2, 5);
            var selectedUsers = users.OrderBy(x => random.Next()).Take(numRatings).ToList();

            foreach (var user in selectedUsers)
            {
                // Check if rating already exists
                var existingRating = await _context.InterventionRatings
                    .FirstOrDefaultAsync(ir => ir.InterventionId == intervention.Id && ir.ApplicationUserId == user.Id);

                if (existingRating == null)
                {
                    var rating = new InterventionRating
                    {
                        InterventionId = intervention.Id,
                        ApplicationUserId = user.Id,
                        Rating = random.Next(1, 6) // Random rating from 1-5
                    };

                    ratings.Add(rating);
                    _logger.LogInformation("Added rating {Rating} for intervention {Intervention} by user {User}", 
                        rating.Rating, intervention.Name, user.UserName);
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