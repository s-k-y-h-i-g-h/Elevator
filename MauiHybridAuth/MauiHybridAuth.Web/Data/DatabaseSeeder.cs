using MauiHybridAuth.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedAllSync(ApplicationDbContext context)
        {
            SeedAllAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        public static async Task SeedAllAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if categories already exist
            if (await context.Categories.AnyAsync(cancellationToken))
                return;

            context.Categories.AddRange(GenerateCategories());
            await context.SaveChangesAsync(cancellationToken);
        }

        private static List<Category> GenerateCategories()
        {
            var categoryData = new Dictionary<string, object>
            {
                ["Health"] = new Dictionary<string, object>
                {
                    ["Essential"] = new Dictionary<string, object>(),
                    ["Hair"] = new Dictionary<string, object>
                    {
                        ["Grey Hair Prevention"] = new Dictionary<string, object>(),
                        ["Hair Loss Prevention"] = new Dictionary<string, object>()
                    },
                    ["Skin"] = new Dictionary<string, object>(),
                    ["Sex"] = new Dictionary<string, object>(),
                    ["Dental"] = new Dictionary<string, object>(),
                    ["Sleep"] = new Dictionary<string, object>
                    {
                        ["Slow Wave Sleep"] = new Dictionary<string, object>(),
                        ["Rapid Eye Movement"] = new Dictionary<string, object>(),
                        ["Latency"] = new Dictionary<string, object>(),
                        ["Duration"] = new Dictionary<string, object>()
                    },
                    ["Joint"] = new Dictionary<string, object>(),
                    ["Digestion"] = new Dictionary<string, object>(),
                    ["Bone"] = new Dictionary<string, object>(),
                    ["Diagnostic"] = new Dictionary<string, object>()
                },

                ["Longevity"] = new Dictionary<string, object>(),

                ["Neuroenhancement"] = new Dictionary<string, object>
                {
                    ["Focus"] = new Dictionary<string, object>(),
                    ["Memory"] = new Dictionary<string, object>(),
                    ["Creativity"] = new Dictionary<string, object>(),
                    ["Neuroprotection"] = new Dictionary<string, object>(),
                    ["Concentration"] = new Dictionary<string, object>(),
                    ["Executive Function"] = new Dictionary<string, object>(),
                    ["Perception"] = new Dictionary<string, object>(),
                    ["Language"] = new Dictionary<string, object>(),
                    ["Reasoning"] = new Dictionary<string, object>(),
                    ["Processing Speed"] = new Dictionary<string, object>(),
                    ["Stress Resistance"] = new Dictionary<string, object>(),
                    ["Happiness"] = new Dictionary<string, object>(),
                    ["Cognitive Flexibility"] = new Dictionary<string, object>(),
                    ["Mood"] = new Dictionary<string, object>(),
                    ["Motivation"] = new Dictionary<string, object>(),
                    ["Self-Esteem"] = new Dictionary<string, object>(),
                    ["Confidence"] = new Dictionary<string, object>(),
                    ["Social"] = new Dictionary<string, object>(),
                    ["Motor"] = new Dictionary<string, object>()
                },

                ["Physical Enhancement"] = new Dictionary<string, object>
                {
                    ["Endurance"] = new Dictionary<string, object>(),
                    ["Strength"] = new Dictionary<string, object>(),
                    ["Recovery"] = new Dictionary<string, object>(),
                    ["Fat Loss"] = new Dictionary<string, object>(),
                    ["Performance"] = new Dictionary<string, object>(),
                    ["Muscle Gain"] = new Dictionary<string, object>(),
                    ["Aesthetic"] = new Dictionary<string, object>(),
                    ["Sensory"] = new Dictionary<string, object>(),
                    ["Functional"] = new Dictionary<string, object>(),
                    ["Diagnostic"] = new Dictionary<string, object>()
                },

                ["Disease"] = new Dictionary<string, object>
                {
                    ["Anxiety"] = new Dictionary<string, object>(),
                    ["Depression"] = new Dictionary<string, object>(),
                    ["Stress"] = new Dictionary<string, object>(),
                    ["Insomnia"] = new Dictionary<string, object>(),
                    ["Inflammation"] = new Dictionary<string, object>(),
                    ["Addiction"] = new Dictionary<string, object>(),
                    ["Schizophrenia"] = new Dictionary<string, object>(),
                    ["Obesity"] = new Dictionary<string, object>(),
                    ["Borderline Personality Disorder"] = new Dictionary<string, object>(),
                    ["Autism"] = new Dictionary<string, object>(),
                    ["Herpes Simplex"] = new Dictionary<string, object>(),
                    ["Cardiovascular Disease"] = new Dictionary<string, object>(),
                    ["Cancer"] = new Dictionary<string, object>(),
                    ["Stroke"] = new Dictionary<string, object>(),
                    ["Diabetes"] = new Dictionary<string, object>()
                }
            };

            var categories = new List<Category>();
            
            // Create all categories from the structured data
            CreateCategoriesFromHierarchy(categoryData, null, categories);
            
            return categories;
        }

        private static void CreateCategoriesFromHierarchy(
            Dictionary<string, object> hierarchy, 
            Category? parent, 
            List<Category> categories)
        {
            foreach (var kvp in hierarchy)
            {
                var category = new Category 
                { 
                    Name = kvp.Key, 
                    Parent = parent 
                };
                
                categories.Add(category);

                // If the value is a dictionary, it contains subcategories
                if (kvp.Value is Dictionary<string, object> subcategories)
                {
                    CreateCategoriesFromHierarchy(subcategories, category, categories);
                }
            }
        }
    }
} 