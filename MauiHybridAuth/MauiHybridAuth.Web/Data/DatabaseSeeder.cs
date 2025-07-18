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
                new { Name = "Caffeine", Duration = "4-6 hours", DoseRange = "100-200mg" },
                new { Name = "L-Theanine", Duration = "2-3 hours", DoseRange = "100-200mg" },
                new { Name = "Creatine", Duration = "N/A (builds up)", DoseRange = "3-5g daily" },
                new { Name = "Magnesium", Duration = "6-8 hours", DoseRange = "200-400mg" },
                new { Name = "Vitamin D", Duration = "24-48 hours", DoseRange = "1000-2000 IU" },
                new { Name = "Omega-3", Duration = "6-12 hours", DoseRange = "500-1000mg" },
                new { Name = "Vitamin B12", Duration = "24-72 hours", DoseRange = "100-500mcg" },
                new { Name = "Zinc", Duration = "8-12 hours", DoseRange = "8-15mg" },
                new { Name = "Iron", Duration = "6-8 hours", DoseRange = "15-25mg" },
                new { Name = "Melatonin", Duration = "4-6 hours", DoseRange = "0.5-1mg" },
                new { Name = "Ashwagandha", Duration = "6-8 hours", DoseRange = "300-500mg" },
                new { Name = "Rhodiola", Duration = "4-6 hours", DoseRange = "200-400mg" },
                new { Name = "Ginkgo Biloba", Duration = "4-6 hours", DoseRange = "120-240mg" },
                new { Name = "Turmeric", Duration = "6-8 hours", DoseRange = "500-1000mg" },
                new { Name = "CoQ10", Duration = "12-24 hours", DoseRange = "100-200mg" },
                new { Name = "Alpha-GPC", Duration = "4-6 hours", DoseRange = "300-600mg" },
                new { Name = "Lion's Mane", Duration = "6-8 hours", DoseRange = "500-1000mg" },
                new { Name = "Bacopa Monnieri", Duration = "6-8 hours", DoseRange = "300-600mg" },
                new { Name = "Modafinil", Duration = "8-12 hours", DoseRange = "100-200mg" },
                new { Name = "Phenylpiracetam", Duration = "4-6 hours", DoseRange = "100-200mg" },
                new { Name = "NAD+", Duration = "2-4 hours", DoseRange = "250-500mg" },
                new { Name = "Resveratrol", Duration = "6-8 hours", DoseRange = "150-500mg" },
                new { Name = "Quercetin", Duration = "12-24 hours", DoseRange = "500-1000mg" },
                new { Name = "Curcumin", Duration = "6-8 hours", DoseRange = "500-1000mg" },
                new { Name = "Green Tea Extract", Duration = "4-6 hours", DoseRange = "300-500mg" },
                new { Name = "Spirulina", Duration = "4-6 hours", DoseRange = "1-3g" },
                new { Name = "Chlorella", Duration = "4-6 hours", DoseRange = "2-3g" },
                new { Name = "Berberine", Duration = "6-8 hours", DoseRange = "500-1500mg" },
                new { Name = "Alpha-Lipoic Acid", Duration = "3-4 hours", DoseRange = "300-600mg" },
                new { Name = "PQQ", Duration = "8-12 hours", DoseRange = "10-20mg" },
                new { Name = "Nicotinamide Riboside", Duration = "6-8 hours", DoseRange = "300-1000mg" },
                new { Name = "Spermidine", Duration = "12-24 hours", DoseRange = "1-10mg" },
                new { Name = "Fisetin", Duration = "8-12 hours", DoseRange = "100-500mg" },
                new { Name = "Pterostilbene", Duration = "6-8 hours", DoseRange = "50-250mg" },
                new { Name = "Sulforaphane", Duration = "4-6 hours", DoseRange = "10-100mg" }
            };

            for (int i = 0; i < count; i++)
            {
                var baseCompound = compoundData[i % compoundData.Length];

                var compound = new Compound
                {
                    Name = baseCompound.Name,
                    Duration = baseCompound.Duration,
                    DoseRange = baseCompound.DoseRange
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
            var hair = new Category { Name = "Hair", Parent = health };
            var greyHairPrevention = new Category { Name = "Grey Hair Prevention", Parent = hair };
            var hairLossPrevention = new Category { Name = "Hair Loss Prevention", Parent = hair };

            categories.AddRange(new[] { vitamins, minerals, essentialNutrients, hair, greyHairPrevention, hairLossPrevention });

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