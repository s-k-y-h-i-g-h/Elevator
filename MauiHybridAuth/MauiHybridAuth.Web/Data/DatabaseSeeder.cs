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
            SeedInterventionCategories(context);
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

        public static void SeedInterventionCategories(ApplicationDbContext context)
        {
            // Check if relationships already exist
            if (context.Set<Dictionary<string, object>>("InterventionCategory").Any())
                return;

            var compounds = context.Compounds.ToList();
            var categories = context.Categories.ToList();

            if (!compounds.Any() || !categories.Any())
                return;

            // Find specific categories for assignment
            var vitamins = categories.FirstOrDefault(c => c.Name == "Vitamins");
            var minerals = categories.FirstOrDefault(c => c.Name == "Minerals");
            var neuroenhancement = categories.FirstOrDefault(c => c.Name == "Neuroenhancement");
            var focus = categories.FirstOrDefault(c => c.Name == "Focus");
            var longevity = categories.FirstOrDefault(c => c.Name == "Longevity");
            var mitochondrialDysfunction = categories.FirstOrDefault(c => c.Name == "Mitochondrial Dysfunction");
            var chronicInflammation = categories.FirstOrDefault(c => c.Name == "Chronic Inflammation");
            var physicalEnhancement = categories.FirstOrDefault(c => c.Name == "Physical Enhancement");
            var strength = categories.FirstOrDefault(c => c.Name == "Strength");

            // Assign categories to specific compounds
            var categoryAssignments = new List<(string CompoundName, Category[] Categories)>
            {
                ("Caffeine", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("L-Theanine", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("Creatine", new[] { physicalEnhancement, strength }.OfType<Category>().ToArray()),
                ("Magnesium", new[] { minerals }.OfType<Category>().ToArray()),
                ("Vitamin D", new[] { vitamins }.OfType<Category>().ToArray()),
                ("Omega-3", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Vitamin B12", new[] { vitamins }.OfType<Category>().ToArray()),
                ("Zinc", new[] { minerals }.OfType<Category>().ToArray()),
                ("Iron", new[] { minerals }.OfType<Category>().ToArray()),
                ("Melatonin", new[] { longevity }.OfType<Category>().ToArray()),
                ("Ashwagandha", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Rhodiola", new[] { neuroenhancement, longevity }.OfType<Category>().ToArray()),
                ("Ginkgo Biloba", new[] { neuroenhancement, longevity }.OfType<Category>().ToArray()),
                ("Turmeric", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("CoQ10", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Alpha-GPC", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("Lion's Mane", new[] { neuroenhancement, longevity }.OfType<Category>().ToArray()),
                ("Bacopa Monnieri", new[] { neuroenhancement }.OfType<Category>().ToArray()),
                ("Modafinil", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("Phenylpiracetam", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("NAD+", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Resveratrol", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Quercetin", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Curcumin", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("PQQ", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Nicotinamide Riboside", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Spermidine", new[] { longevity }.OfType<Category>().ToArray()),
                ("Fisetin", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Pterostilbene", new[] { longevity }.OfType<Category>().ToArray()),
                ("Sulforaphane", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray())
            };

            // Apply category assignments
            foreach (var (compoundName, assignedCategories) in categoryAssignments)
            {
                var compound = compounds.FirstOrDefault(c => c.Name == compoundName);
                if (compound != null && assignedCategories.Any())
                {
                    compound.Categories.Clear();
                    foreach (var category in assignedCategories)
                    {
                        compound.Categories.Add(category);
                    }
                }
            }

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
            await SeedInterventionCategoriesAsync(context, cancellationToken);
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

        public static async Task SeedInterventionCategoriesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if relationships already exist
            if (await context.Set<Dictionary<string, object>>("InterventionCategory").AnyAsync(cancellationToken))
                return;

            var compounds = await context.Compounds.ToListAsync(cancellationToken);
            var categories = await context.Categories.ToListAsync(cancellationToken);

            if (!compounds.Any() || !categories.Any())
                return;

            // Find specific categories for assignment
            var vitamins = categories.FirstOrDefault(c => c.Name == "Vitamins");
            var minerals = categories.FirstOrDefault(c => c.Name == "Minerals");
            var neuroenhancement = categories.FirstOrDefault(c => c.Name == "Neuroenhancement");
            var focus = categories.FirstOrDefault(c => c.Name == "Focus");
            var longevity = categories.FirstOrDefault(c => c.Name == "Longevity");
            var mitochondrialDysfunction = categories.FirstOrDefault(c => c.Name == "Mitochondrial Dysfunction");
            var chronicInflammation = categories.FirstOrDefault(c => c.Name == "Chronic Inflammation");
            var physicalEnhancement = categories.FirstOrDefault(c => c.Name == "Physical Enhancement");
            var strength = categories.FirstOrDefault(c => c.Name == "Strength");

            // Assign categories to specific compounds
            var categoryAssignments = new List<(string CompoundName, Category[] Categories)>
            {
                ("Caffeine", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("L-Theanine", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("Creatine", new[] { physicalEnhancement, strength }.OfType<Category>().ToArray()),
                ("Magnesium", new[] { minerals }.OfType<Category>().ToArray()),
                ("Vitamin D", new[] { vitamins }.OfType<Category>().ToArray()),
                ("Omega-3", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Vitamin B12", new[] { vitamins }.OfType<Category>().ToArray()),
                ("Zinc", new[] { minerals }.OfType<Category>().ToArray()),
                ("Iron", new[] { minerals }.OfType<Category>().ToArray()),
                ("Melatonin", new[] { longevity }.OfType<Category>().ToArray()),
                ("Ashwagandha", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Rhodiola", new[] { neuroenhancement, longevity }.OfType<Category>().ToArray()),
                ("Ginkgo Biloba", new[] { neuroenhancement, longevity }.OfType<Category>().ToArray()),
                ("Turmeric", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("CoQ10", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Alpha-GPC", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("Lion's Mane", new[] { neuroenhancement, longevity }.OfType<Category>().ToArray()),
                ("Bacopa Monnieri", new[] { neuroenhancement }.OfType<Category>().ToArray()),
                ("Modafinil", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("Phenylpiracetam", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                ("NAD+", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Resveratrol", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Quercetin", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Curcumin", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("PQQ", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Nicotinamide Riboside", new[] { longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Spermidine", new[] { longevity }.OfType<Category>().ToArray()),
                ("Fisetin", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray()),
                ("Pterostilbene", new[] { longevity }.OfType<Category>().ToArray()),
                ("Sulforaphane", new[] { longevity, chronicInflammation }.OfType<Category>().ToArray())
            };

            // Apply category assignments
            foreach (var (compoundName, assignedCategories) in categoryAssignments)
            {
                var compound = compounds.FirstOrDefault(c => c.Name == compoundName);
                if (compound != null && assignedCategories.Any())
                {
                    compound.Categories.Clear();
                    foreach (var category in assignedCategories)
                    {
                        compound.Categories.Add(category);
                    }
                }
            }

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
            var essential = new Category { Name = "Essential", Parent = health };
            var hair = new Category { Name = "Hair", Parent = health };
            var greyHairPrevention = new Category { Name = "Grey Hair Prevention", Parent = hair };
            var hairLossPrevention = new Category { Name = "Hair Loss Prevention", Parent = hair };
            var skin = new Category { Name = "Skin", Parent = health };
            var sex = new Category { Name = "Sex", Parent = health };
            var dental = new Category { Name = "Dental", Parent = health };
            var sleep = new Category { Name = "Sleep", Parent = health };
            var slowWaveSleep = new Category { Name = "Slow Wave Sleep", Parent = sleep };
            var rapidEyeMovement = new Category { Name = "Rapid Eye Movement", Parent = sleep };
            var latency = new Category { Name = "Latency", Parent = sleep };
            var duration = new Category { Name = "Duration", Parent = sleep };
            var joint = new Category { Name = "Joint", Parent = health };
            var digestion = new Category { Name = "Digestion", Parent = health };
            var bone = new Category { Name = "Bone", Parent = health };
            var diagnosticHealth = new Category { Name = "Diagnostic", Parent = health };

            categories.AddRange(new[] { essential, hair, greyHairPrevention, hairLossPrevention, skin, sex, dental, sleep, slowWaveSleep, rapidEyeMovement, latency, duration, joint, digestion, bone, diagnosticHealth });

            var genomicInstability = new Category { Name = "Genomic Instability", Parent = longevity };
            var telomereAttrition = new Category { Name = "Telomere Attrition", Parent = longevity };
            var epigeneticAlterations = new Category { Name = "Epigenetic Alterations", Parent = longevity };
            var lossOfProteostasis = new Category { Name = "Loss of Proteostasis", Parent = longevity };
            var disabledMacroautophagy = new Category { Name = "Disabled Macroautophagy", Parent = longevity };
            var deregulatedNutrientSensing = new Category { Name = "Deregulated Nutrient-Sensing", Parent = longevity };
            var mitochondrialDysfunction = new Category { Name = "Mitochondrial Dysfunction", Parent = longevity };
            var cellularSenescence = new Category { Name = "Cellular Senescence", Parent = longevity };
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
            var concentration = new Category { Name = "Concentration", Parent = neuroenhancement };
            var executiveFunction = new Category { Name = "Executive Function", Parent = neuroenhancement };
            var perception = new Category { Name = "Perception", Parent = neuroenhancement };
            var language = new Category { Name = "Language", Parent = neuroenhancement };
            var reasoning = new Category { Name = "Reasoning", Parent = neuroenhancement };
            var processingSpeed = new Category { Name = "Processing Speed", Parent = neuroenhancement };
            var stressResistance = new Category { Name = "Stress Resistance", Parent = neuroenhancement };
            var happiness = new Category { Name = "Happiness", Parent = neuroenhancement };
            var cognitiveFlexibility = new Category { Name = "Cognitive Flexibility", Parent = neuroenhancement };
            var mood = new Category { Name = "Mood", Parent = neuroenhancement };
            var motivation = new Category { Name = "Motivation", Parent = neuroenhancement };
            var selfEsteem = new Category { Name = "Self-Esteem", Parent = neuroenhancement };
            var confidence = new Category { Name = "Confidence", Parent = neuroenhancement };
            var social = new Category { Name = "Social", Parent = neuroenhancement };
            var motor = new Category { Name = "Motor", Parent = neuroenhancement };

            categories.AddRange(new[] { focus, memory, creativity, neuroprotection, concentration, executiveFunction, perception, language, reasoning, processingSpeed, stressResistance, happiness, cognitiveFlexibility, mood, motivation, selfEsteem, confidence, social, motor });

            // Physical Enhancement subcategories
            var endurance = new Category { Name = "Endurance", Parent = physicalEnhancement };
            var strength = new Category { Name = "Strength", Parent = physicalEnhancement };
            var recovery = new Category { Name = "Recovery", Parent = physicalEnhancement };
            var fatLoss = new Category { Name = "Fat Loss", Parent = physicalEnhancement };
            var performance = new Category { Name = "Performance", Parent = physicalEnhancement };
            var muscleGain = new Category { Name = "Muscle Gain", Parent = physicalEnhancement };
            var aesthetic = new Category { Name = "Aesthetic", Parent = physicalEnhancement };
            var sensory = new Category { Name = "Sensory", Parent = physicalEnhancement };
            var functional = new Category { Name = "Functional", Parent = physicalEnhancement };
            var diagnosticPhysical = new Category { Name = "Diagnostic", Parent = physicalEnhancement };

            categories.AddRange(new[] { endurance, strength, recovery, fatLoss, performance, muscleGain, aesthetic, sensory, functional, diagnosticPhysical });

            // Disease subcategories
            var anxiety = new Category { Name = "Anxiety", Parent = disease };
            var depression = new Category { Name = "Depression", Parent = disease };
            var stress = new Category { Name = "Stress", Parent = disease };
            var insomnia = new Category { Name = "Insomnia", Parent = disease };
            var inflammation = new Category { Name = "Inflammation", Parent = disease };
            var addiction = new Category { Name = "Addiction", Parent = disease };
            var schizophrenia = new Category { Name = "Schizophrenia", Parent = disease };
            var obesity = new Category { Name = "Obesity", Parent = disease };
            var borderlinePersonalityDisorder = new Category { Name = "Borderline Personality Disorder", Parent = disease };
            var autism = new Category { Name = "Autism", Parent = disease };
            var herpesSimplex = new Category { Name = "Herpes Simplex", Parent = disease };
            var cardiovascularDisease = new Category { Name = "Cardiovascular Disease", Parent = disease };
            var cancer = new Category { Name = "Cancer", Parent = disease };
            var stroke = new Category { Name = "Stroke", Parent = disease };
            var diabetes = new Category { Name = "Diabetes", Parent = disease };

            categories.AddRange(new[] { anxiety, depression, stress, insomnia, inflammation, addiction, schizophrenia, obesity, borderlinePersonalityDisorder, autism, herpesSimplex, cardiovascularDisease, cancer, stroke, diabetes });

            return categories;
        }
    }
} 