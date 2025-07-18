using MauiHybridAuth.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedAll(ApplicationDbContext context)
        {
            SeedCategories(context);
            SeedCompounds(context);
        }

        public static void SeedCategories(ApplicationDbContext context)
        {
            // Check if categories already exist
            if (context.Categories.Any())
                return;

            var categories = GenerateTestCategories();
            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        public static void SeedCompounds(ApplicationDbContext context)
        {
            // Check if compounds already exist
            if (context.Compounds.Any())
                return;

            var compounds = GenerateTestCompounds(100);
            context.Compounds.AddRange(compounds);
            context.SaveChanges();
        }

        private static List<Compound> GenerateTestCompounds(int count)
        {
            var compounds = new List<Compound>();
            var random = new Random();

            // Sample compound data with realistic names, durations, and dose ranges
            var compoundData = new[]
            {
                new { Name = "Caffeine", DurationOptions = new[] { "4-6 hours", "3-5 hours", "5-7 hours" }, DoseRanges = new[] { "50-100mg", "100-200mg", "200-400mg" } },
                new { Name = "L-Theanine", DurationOptions = new[] { "2-3 hours", "3-4 hours", "1-3 hours" }, DoseRanges = new[] { "100-200mg", "200-400mg", "50-100mg" } },
                new { Name = "Creatine", DurationOptions = new[] { "N/A (builds up)", "Ongoing", "Cumulative" }, DoseRanges = new[] { "3-5g daily", "5-10g daily", "2-3g daily" } },
                new { Name = "Magnesium", DurationOptions = new[] { "6-8 hours", "4-6 hours", "8-12 hours" }, DoseRanges = new[] { "200-400mg", "400-600mg", "100-200mg" } },
                new { Name = "Vitamin D", DurationOptions = new[] { "24-48 hours", "12-24 hours", "2-3 days" }, DoseRanges = new[] { "1000-2000 IU", "2000-4000 IU", "4000-6000 IU" } },
                new { Name = "Omega-3", DurationOptions = new[] { "6-12 hours", "12-24 hours", "4-8 hours" }, DoseRanges = new[] { "500-1000mg", "1000-2000mg", "300-500mg" } },
                new { Name = "Vitamin B12", DurationOptions = new[] { "24-72 hours", "12-24 hours", "3-5 days" }, DoseRanges = new[] { "100-500mcg", "500-1000mcg", "1000-2500mcg" } },
                new { Name = "Zinc", DurationOptions = new[] { "8-12 hours", "6-8 hours", "12-16 hours" }, DoseRanges = new[] { "8-15mg", "15-30mg", "5-10mg" } },
                new { Name = "Iron", DurationOptions = new[] { "6-8 hours", "8-12 hours", "4-6 hours" }, DoseRanges = new[] { "15-25mg", "25-50mg", "8-15mg" } },
                new { Name = "Melatonin", DurationOptions = new[] { "4-6 hours", "6-8 hours", "3-5 hours" }, DoseRanges = new[] { "0.5-1mg", "1-3mg", "3-5mg" } },
                new { Name = "Ashwagandha", DurationOptions = new[] { "6-8 hours", "8-12 hours", "4-6 hours" }, DoseRanges = new[] { "300-500mg", "500-1000mg", "600-1200mg" } },
                new { Name = "Rhodiola", DurationOptions = new[] { "4-6 hours", "6-8 hours", "3-5 hours" }, DoseRanges = new[] { "200-400mg", "400-600mg", "100-200mg" } },
                new { Name = "Ginkgo Biloba", DurationOptions = new[] { "4-6 hours", "6-8 hours", "2-4 hours" }, DoseRanges = new[] { "120-240mg", "240-480mg", "60-120mg" } },
                new { Name = "Turmeric", DurationOptions = new[] { "6-8 hours", "8-12 hours", "4-6 hours" }, DoseRanges = new[] { "500-1000mg", "1000-1500mg", "300-500mg" } },
                new { Name = "CoQ10", DurationOptions = new[] { "12-24 hours", "8-12 hours", "6-10 hours" }, DoseRanges = new[] { "100-200mg", "200-400mg", "50-100mg" } },
                new { Name = "Alpha-GPC", DurationOptions = new[] { "4-6 hours", "6-8 hours", "3-5 hours" }, DoseRanges = new[] { "300-600mg", "600-1200mg", "150-300mg" } },
                new { Name = "Lion's Mane", DurationOptions = new[] { "6-8 hours", "8-12 hours", "4-6 hours" }, DoseRanges = new[] { "500-1000mg", "1000-3000mg", "300-500mg" } },
                new { Name = "Bacopa Monnieri", DurationOptions = new[] { "6-8 hours", "8-12 hours", "4-6 hours" }, DoseRanges = new[] { "300-600mg", "600-900mg", "150-300mg" } },
                new { Name = "Modafinil", DurationOptions = new[] { "8-12 hours", "10-15 hours", "6-10 hours" }, DoseRanges = new[] { "100-200mg", "200-400mg", "50-100mg" } },
                new { Name = "Phenylpiracetam", DurationOptions = new[] { "4-6 hours", "6-8 hours", "3-5 hours" }, DoseRanges = new[] { "100-200mg", "200-400mg", "50-100mg" } }
            };

            for (int i = 0; i < count; i++)
            {
                var baseCompound = compoundData[random.Next(compoundData.Length)];
                
                // Add some variation to names for duplicates
                var name = baseCompound.Name;
                if (compounds.Any(c => c.Name == name))
                {
                    var suffixes = new[] { "Extended Release", "Immediate Release", "Micronized", "Chelated", "Liposomal", "Buffered" };
                    name = $"{baseCompound.Name} ({suffixes[random.Next(suffixes.Length)]})";
                }

                var compound = new Compound
                {
                    Name = name,
                    Duration = baseCompound.DurationOptions[random.Next(baseCompound.DurationOptions.Length)],
                    DoseRange = baseCompound.DoseRanges[random.Next(baseCompound.DoseRanges.Length)]
                };

                compounds.Add(compound);
            }

            return compounds;
        }

        public static async Task SeedAllAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            await SeedCategoriesAsync(context, cancellationToken);
            await SeedCompoundsAsync(context, cancellationToken);
        }

        public static async Task SeedCategoriesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if categories already exist
            if (await context.Categories.AnyAsync(cancellationToken))
                return;

            var categories = GenerateTestCategories();
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync(cancellationToken);
        }

        public static async Task SeedCompoundsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if compounds already exist
            if (await context.Compounds.AnyAsync(cancellationToken))
                return;

            var compounds = GenerateTestCompounds(100);
            context.Compounds.AddRange(compounds);
            await context.SaveChangesAsync(cancellationToken);
        }

        private static List<Category> GenerateTestCategories()
        {
            var categories = new List<Category>();

            // Root categories
            var health = new Category { Name = "Health" };
            var longevity = new Category { Name = "Longevity" };
            var neuroenhancement = new Category { Name = "Neuroenhancement" };
            var physicalEnhancement = new Category { Name = "Physical Enhancement" };
            var disease = new Category { Name = "Disease" };

            categories.AddRange(new[] { health, longevity, neuroenhancement, physicalEnhancement, disease });

            // Health subcategories
            var vitamins = new Category { Name = "Vitamins", Parent = health };
            var minerals = new Category { Name = "Minerals", Parent = health };
            var essentialNutrients = new Category { Name = "Essential Nutrients", Parent = health };
            var generalWellness = new Category { Name = "General Wellness", Parent = health };

            categories.AddRange(new[] { vitamins, minerals, essentialNutrients, generalWellness });

            // Longevity subcategories - The 12 Hallmarks of Aging (López-Otín et al., 2023)
            // Primary Hallmarks
            var genomicInstability = new Category { Name = "Genomic Instability", Parent = longevity };
            var telomereAttrition = new Category { Name = "Telomere Attrition", Parent = longevity };
            var epigeneticAlterations = new Category { Name = "Epigenetic Alterations", Parent = longevity };
            var lossOfProteostasis = new Category { Name = "Loss of Proteostasis", Parent = longevity };
            var disabledMacroautophagy = new Category { Name = "Disabled Macroautophagy", Parent = longevity };
            
            // Antagonistic Hallmarks
            var deregulatedNutrientSensing = new Category { Name = "Deregulated Nutrient-Sensing", Parent = longevity };
            var mitochondrialDysfunction = new Category { Name = "Mitochondrial Dysfunction", Parent = longevity };
            var cellularSenescence = new Category { Name = "Cellular Senescence", Parent = longevity };
            
            // Integrative Hallmarks
            var stemCellExhaustion = new Category { Name = "Stem Cell Exhaustion", Parent = longevity };
            var alteredIntercellularCommunication = new Category { Name = "Altered Intercellular Communication", Parent = longevity };
            var chronicInflammation = new Category { Name = "Chronic Inflammation", Parent = longevity };
            var dysbiosis = new Category { Name = "Dysbiosis", Parent = longevity };

            categories.AddRange(new[] { genomicInstability, telomereAttrition, epigeneticAlterations, lossOfProteostasis, disabledMacroautophagy, 
                                      deregulatedNutrientSensing, mitochondrialDysfunction, cellularSenescence, 
                                      stemCellExhaustion, alteredIntercellularCommunication, chronicInflammation, dysbiosis });

            // Neuroenhancement subcategories
            var focus = new Category { Name = "Focus", Parent = neuroenhancement };
            var memory = new Category { Name = "Memory", Parent = neuroenhancement };
            var creativity = new Category { Name = "Creativity", Parent = neuroenhancement };
            var neuroprotection = new Category { Name = "Neuroprotection", Parent = neuroenhancement };

            categories.AddRange(new[] { focus, memory, creativity, neuroprotection });

            // Physical Enhancement subcategories
            var endurance = new Category { Name = "Endurance", Parent = physicalEnhancement };
            var strength = new Category { Name = "Strength", Parent = physicalEnhancement };
            var recovery = new Category { Name = "Recovery", Parent = physicalEnhancement };
            var fatLoss = new Category { Name = "Fat Loss", Parent = physicalEnhancement };
            var performance = new Category { Name = "Performance", Parent = physicalEnhancement };

            categories.AddRange(new[] { endurance, strength, recovery, fatLoss, performance });

            // Disease subcategories
            var anxiety = new Category { Name = "Anxiety", Parent = disease };
            var depression = new Category { Name = "Depression", Parent = disease };
            var stress = new Category { Name = "Stress", Parent = disease };
            var insomnia = new Category { Name = "Insomnia", Parent = disease };
            var inflammation = new Category { Name = "Inflammation", Parent = disease };
            var immuneDisorders = new Category { Name = "Immune Disorders", Parent = disease };

            categories.AddRange(new[] { anxiety, depression, stress, insomnia, inflammation, immuneDisorders });

            return categories;
        }
    }
} 