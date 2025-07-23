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
            SeedPlants(context);
            SeedFormulations(context);
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

            var compounds = GenerateNootropicCompounds();
            context.Compounds.AddRange(compounds);
            context.SaveChanges();
        }

        public static void SeedInterventionCategories(ApplicationDbContext context)
        {
            // Check if relationships already exist
            if (context.Set<Dictionary<string, object>>("InterventionCategory").Any())
                return;

            // Call the async version synchronously to maintain consistency
            SeedInterventionCategoriesAsync(context).GetAwaiter().GetResult();
        }

        private static List<Compound> GenerateNootropicCompounds()
        {
            var compounds = new List<Compound>();

            // Comprehensive nootropic compound data with classification tags
            var compoundData = new[]
            {
                // Racetams
                new { Name = "Piracetam", Duration = "4-6 hours", DoseRange = "1200-4800mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Aniracetam", Duration = "2-4 hours", DoseRange = "400-1500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Oxiracetam", Duration = "6-8 hours", DoseRange = "600-2400mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Pramiracetam", Duration = "4-6 hours", DoseRange = "300-1200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Phenylpiracetam", Duration = "4-6 hours", DoseRange = "100-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Nefiracetam", Duration = "6-8 hours", DoseRange = "150-900mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Coluracetam", Duration = "3-5 hours", DoseRange = "3-20mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Ampakines
                new { Name = "Sunifiram", Duration = "1-3 hours", DoseRange = "4-10mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Unifiram", Duration = "1-3 hours", DoseRange = "2-10mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Noopept and related
                new { Name = "Noopept", Duration = "2-4 hours", DoseRange = "10-40mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "PRL-8-53", Duration = "12-24 hours", DoseRange = "2.5-5mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Eugeroics (Wakefulness Promoters)
                new { Name = "Modafinil", Duration = "8-12 hours", DoseRange = "100-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Medication, ClassificationTag.Drug } },
                new { Name = "Armodafinil", Duration = "12-15 hours", DoseRange = "50-250mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Medication, ClassificationTag.Drug } },
                new { Name = "Adrafinil", Duration = "6-8 hours", DoseRange = "150-300mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Fluorenol", Duration = "4-6 hours", DoseRange = "20-40mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Cholinergics
                new { Name = "Alpha-GPC", Duration = "4-6 hours", DoseRange = "300-600mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "CDP-Choline", Duration = "6-8 hours", DoseRange = "250-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Centrophenoxine", Duration = "4-6 hours", DoseRange = "250-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Choline Bitartrate", Duration = "4-6 hours", DoseRange = "500-2000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Huperzine A", Duration = "8-12 hours", DoseRange = "50-200mcg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Stimulants
                new { Name = "Caffeine", Duration = "4-6 hours", DoseRange = "100-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "L-Theanine", Duration = "2-3 hours", DoseRange = "100-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Theacrine", Duration = "6-8 hours", DoseRange = "50-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Dynamine", Duration = "3-5 hours", DoseRange = "25-100mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Dopaminergics
                new { Name = "L-DOPA", Duration = "1-3 hours", DoseRange = "100-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Medication } },
                new { Name = "L-Tyrosine", Duration = "2-4 hours", DoseRange = "500-2000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "N-Acetyl L-Tyrosine", Duration = "2-4 hours", DoseRange = "350-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Phenylethylamine", Duration = "0.5-1 hours", DoseRange = "100-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // GABAergics
                new { Name = "Phenibut", Duration = "8-24 hours", DoseRange = "250-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Picamilon", Duration = "4-6 hours", DoseRange = "50-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Taurine", Duration = "2-4 hours", DoseRange = "500-2000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Serotonergics
                new { Name = "5-HTP", Duration = "6-8 hours", DoseRange = "50-300mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "L-Tryptophan", Duration = "4-6 hours", DoseRange = "500-1500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Neuropeptides
                new { Name = "Noopept", Duration = "2-4 hours", DoseRange = "10-40mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "NSI-189", Duration = "12-24 hours", DoseRange = "40-80mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Metabolics
                new { Name = "Creatine", Duration = "N/A (builds up)", DoseRange = "3-5g daily", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "PQQ", Duration = "8-12 hours", DoseRange = "10-20mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Nicotinamide Riboside", Duration = "6-8 hours", DoseRange = "300-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Vitamins and Minerals
                new { Name = "Vitamin D", Duration = "24-48 hours", DoseRange = "1000-2000 IU", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Vitamin B12", Duration = "24-72 hours", DoseRange = "100-500mcg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Magnesium", Duration = "6-8 hours", DoseRange = "200-400mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Zinc", Duration = "8-12 hours", DoseRange = "8-15mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Iron", Duration = "6-8 hours", DoseRange = "15-25mg", Tags = new[] { ClassificationTag.Supplement } },
                
                // Others
                new { Name = "Vinpocetine", Duration = "4-6 hours", DoseRange = "5-40mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Sulbutiamine", Duration = "4-6 hours", DoseRange = "200-600mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Idebenone", Duration = "6-8 hours", DoseRange = "45-270mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Citicoline", Duration = "6-8 hours", DoseRange = "250-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Phosphatidylserine", Duration = "6-8 hours", DoseRange = "100-300mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Acetyl-L-Carnitine", Duration = "4-6 hours", DoseRange = "500-2000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                
                // Longevity compounds that also have nootropic effects
                new { Name = "Resveratrol", Duration = "6-8 hours", DoseRange = "150-500mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Quercetin", Duration = "12-24 hours", DoseRange = "500-1000mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Curcumin", Duration = "6-8 hours", DoseRange = "500-1000mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Omega-3", Duration = "6-12 hours", DoseRange = "500-1000mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Melatonin", Duration = "4-6 hours", DoseRange = "0.5-1mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "CoQ10", Duration = "12-24 hours", DoseRange = "100-200mg", Tags = new[] { ClassificationTag.Supplement } }
            };

            foreach (var data in compoundData)
            {
                var compound = new Compound
                {
                    Name = data.Name,
                    Duration = data.Duration,
                    DoseRange = data.DoseRange,
                    ClassificationTags = data.Tags.ToList()
                };

                compounds.Add(compound);
            }

            return compounds;
        }

        public static void SeedPlants(ApplicationDbContext context)
        {
            // Check if plants already exist
            if (context.Plants.Any())
                return;

            var plants = GenerateNootropicPlants();
            context.Plants.AddRange(plants);
            context.SaveChanges();
        }

        public static void SeedFormulations(ApplicationDbContext context)
        {
            // Check if formulations already exist
            if (context.Formulations.Any())
                return;

            var formulations = GenerateNootropicFormulations();
            context.Formulations.AddRange(formulations);
            context.SaveChanges();
        }

        private static List<Plant> GenerateNootropicPlants()
        {
            var plants = new List<Plant>();

            // Nootropic plants with classification tags
            var plantData = new[]
            {
                new { Name = "Ginkgo Biloba", Duration = "4-6 hours", DoseRange = "120-240mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Bacopa Monnieri", Duration = "6-8 hours", DoseRange = "300-600mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Lion's Mane Mushroom", Duration = "6-8 hours", DoseRange = "500-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Rhodiola Rosea", Duration = "4-6 hours", DoseRange = "200-400mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Ashwagandha", Duration = "6-8 hours", DoseRange = "300-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Panax Ginseng", Duration = "4-6 hours", DoseRange = "200-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Centella Asiatica", Duration = "4-6 hours", DoseRange = "300-750mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Mucuna Pruriens", Duration = "4-6 hours", DoseRange = "300-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Green Tea", Duration = "4-6 hours", DoseRange = "300-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Curcuma Longa", Duration = "6-8 hours", DoseRange = "500-1000mg", Tags = new[] { ClassificationTag.Supplement } },
                new { Name = "Yerba Mate", Duration = "4-6 hours", DoseRange = "200-500mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Kanna", Duration = "2-4 hours", DoseRange = "50-200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Camellia Sinensis", Duration = "4-6 hours", DoseRange = "200-400mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Polygala Tenuifolia", Duration = "4-6 hours", DoseRange = "100-300mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Schisandra Chinensis", Duration = "6-8 hours", DoseRange = "300-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Eleutherococcus Senticosus", Duration = "6-8 hours", DoseRange = "300-1200mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Convolvulus Pluricaulis", Duration = "4-6 hours", DoseRange = "500-1000mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Mandukaparni", Duration = "4-6 hours", DoseRange = "300-600mg", Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } }
            };

            foreach (var data in plantData)
            {
                var plant = new Plant
                {
                    Name = data.Name,
                    Duration = data.Duration,
                    DoseRange = data.DoseRange,
                    ClassificationTags = data.Tags.ToList()
                };

                plants.Add(plant);
            }

            return plants;
        }

        private static List<Formulation> GenerateNootropicFormulations()
        {
            var formulations = new List<Formulation>();

            // Common nootropic formulations and stacks
            var formulationData = new[]
            {
                new { Name = "Nootropic Stack Alpha", Duration = "4-6 hours", DoseRange = "1-2 capsules", Liposomal = false, Micronised = false, ExtendedRelease = false, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Cognitive Enhancement Complex", Duration = "6-8 hours", DoseRange = "2-4 capsules", Liposomal = true, Micronised = false, ExtendedRelease = true, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Brain Boost Formula", Duration = "4-6 hours", DoseRange = "1-3 capsules", Liposomal = false, Micronised = true, ExtendedRelease = false, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Focus & Memory Blend", Duration = "6-8 hours", DoseRange = "2-3 capsules", Liposomal = true, Micronised = true, ExtendedRelease = true, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Racetam Complex", Duration = "4-6 hours", DoseRange = "1-2 capsules", Liposomal = false, Micronised = false, ExtendedRelease = false, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Choline Matrix", Duration = "6-8 hours", DoseRange = "1-2 capsules", Liposomal = true, Micronised = false, ExtendedRelease = false, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Adaptogen Cognitive Blend", Duration = "8-12 hours", DoseRange = "2-4 capsules", Liposomal = false, Micronised = false, ExtendedRelease = true, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } },
                new { Name = "Liposomal Curcumin Complex", Duration = "6-8 hours", DoseRange = "1-2 capsules", Liposomal = true, Micronised = true, ExtendedRelease = false, Tags = new[] { ClassificationTag.Nootropic, ClassificationTag.Supplement } }
            };

            foreach (var data in formulationData)
            {
                var formulation = new Formulation
                {
                    Name = data.Name,
                    Duration = data.Duration,
                    DoseRange = data.DoseRange,
                    Liposomal = data.Liposomal,
                    Micronised = data.Micronised,
                    ExtendedRelease = data.ExtendedRelease,
                    ClassificationTags = data.Tags.ToList()
                };

                formulations.Add(formulation);
            }

            return formulations;
        }

        public static async Task SeedAllAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            await SeedCategoriesAsync(context, cancellationToken);
            await SeedCompoundsAsync(context, cancellationToken);
            await SeedPlantsAsync(context, cancellationToken);
            await SeedFormulationsAsync(context, cancellationToken);
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

            var compounds = GenerateNootropicCompounds();
            context.Compounds.AddRange(compounds);
            await context.SaveChangesAsync(cancellationToken);
        }

        public static async Task SeedPlantsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if plants already exist
            if (await context.Plants.AnyAsync(cancellationToken))
                return;

            var plants = GenerateNootropicPlants();
            context.Plants.AddRange(plants);
            await context.SaveChangesAsync(cancellationToken);
        }

        public static async Task SeedFormulationsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if formulations already exist
            if (await context.Formulations.AnyAsync(cancellationToken))
                return;

            var formulations = GenerateNootropicFormulations();
            context.Formulations.AddRange(formulations);
            await context.SaveChangesAsync(cancellationToken);
        }

        public static async Task SeedInterventionCategoriesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if relationships already exist
            if (await context.Set<Dictionary<string, object>>("InterventionCategory").AnyAsync(cancellationToken))
                return;

            var compounds = await context.Compounds.ToListAsync(cancellationToken);
            var plants = await context.Plants.ToListAsync(cancellationToken);
            var formulations = await context.Formulations.ToListAsync(cancellationToken);
            var categories = await context.Categories.ToListAsync(cancellationToken);

            if (!categories.Any())
                return;

            // Find specific categories for assignment
            var essential = categories.FirstOrDefault(c => c.Name == "Essential");
            var neuroenhancement = categories.FirstOrDefault(c => c.Name == "Neuroenhancement");
            var focus = categories.FirstOrDefault(c => c.Name == "Focus");
            var memory = categories.FirstOrDefault(c => c.Name == "Memory");
            var concentration = categories.FirstOrDefault(c => c.Name == "Concentration");
            var creativity = categories.FirstOrDefault(c => c.Name == "Creativity");
            var neuroprotection = categories.FirstOrDefault(c => c.Name == "Neuroprotection");
            var executiveFunction = categories.FirstOrDefault(c => c.Name == "Executive Function");
            var processingSpeed = categories.FirstOrDefault(c => c.Name == "Processing Speed");
            var reasoning = categories.FirstOrDefault(c => c.Name == "Reasoning");
            var mood = categories.FirstOrDefault(c => c.Name == "Mood");
            var motivation = categories.FirstOrDefault(c => c.Name == "Motivation");
            var stressResistance = categories.FirstOrDefault(c => c.Name == "Stress Resistance");
            var cognitiveFlexibility = categories.FirstOrDefault(c => c.Name == "Cognitive Flexibility");
            var confidence = categories.FirstOrDefault(c => c.Name == "Confidence");
            var social = categories.FirstOrDefault(c => c.Name == "Social");
            var happiness = categories.FirstOrDefault(c => c.Name == "Happiness");
            var perception = categories.FirstOrDefault(c => c.Name == "Perception");
            var language = categories.FirstOrDefault(c => c.Name == "Language");
            var motor = categories.FirstOrDefault(c => c.Name == "Motor");
            
            var longevity = categories.FirstOrDefault(c => c.Name == "Longevity");
            var mitochondrialDysfunction = categories.FirstOrDefault(c => c.Name == "Mitochondrial Dysfunction");
            var chronicInflammation = categories.FirstOrDefault(c => c.Name == "Chronic Inflammation");
            var cellularSenescence = categories.FirstOrDefault(c => c.Name == "Cellular Senescence");
            
            var physicalEnhancement = categories.FirstOrDefault(c => c.Name == "Physical Enhancement");
            var strength = categories.FirstOrDefault(c => c.Name == "Strength");
            var endurance = categories.FirstOrDefault(c => c.Name == "Endurance");
            var recovery = categories.FirstOrDefault(c => c.Name == "Recovery");
            var performance = categories.FirstOrDefault(c => c.Name == "Performance");
            var muscleGain = categories.FirstOrDefault(c => c.Name == "Muscle Gain");
            
            var sleep = categories.FirstOrDefault(c => c.Name == "Sleep");
            var anxiety = categories.FirstOrDefault(c => c.Name == "Anxiety");
            var depression = categories.FirstOrDefault(c => c.Name == "Depression");
            var stress = categories.FirstOrDefault(c => c.Name == "Stress");

            // Comprehensive category assignments for compounds
            var compoundAssignments = new List<(string Name, Category[] Categories)>
            {
                // Racetams - primarily memory and learning enhancers
                ("Piracetam", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("Aniracetam", new[] { neuroenhancement, memory, creativity, mood, anxiety }.OfType<Category>().ToArray()),
                ("Oxiracetam", new[] { neuroenhancement, memory, focus, concentration, reasoning }.OfType<Category>().ToArray()),
                ("Pramiracetam", new[] { neuroenhancement, memory, focus, concentration }.OfType<Category>().ToArray()),
                ("Phenylpiracetam", new[] { neuroenhancement, memory, focus, motivation, physicalEnhancement }.OfType<Category>().ToArray()),
                ("Nefiracetam", new[] { neuroenhancement, memory, neuroprotection, depression }.OfType<Category>().ToArray()),
                ("Coluracetam", new[] { neuroenhancement, memory, perception, depression }.OfType<Category>().ToArray()),
                
                // Ampakines - cognitive enhancers
                ("Sunifiram", new[] { neuroenhancement, memory, focus }.OfType<Category>().ToArray()),
                ("Unifiram", new[] { neuroenhancement, memory, focus }.OfType<Category>().ToArray()),
                
                // Noopept and related peptides
                ("Noopept", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("PRL-8-53", new[] { neuroenhancement, memory }.OfType<Category>().ToArray()),
                ("NSI-189", new[] { neuroenhancement, depression, mood, neuroprotection }.OfType<Category>().ToArray()),
                
                // Eugeroics - wakefulness promoters
                ("Modafinil", new[] { neuroenhancement, focus, motivation, concentration }.OfType<Category>().ToArray()),
                ("Armodafinil", new[] { neuroenhancement, focus, motivation, concentration }.OfType<Category>().ToArray()),
                ("Adrafinil", new[] { neuroenhancement, focus, motivation }.OfType<Category>().ToArray()),
                ("Fluorenol", new[] { neuroenhancement, focus }.OfType<Category>().ToArray()),
                
                // Cholinergics - acetylcholine system enhancers
                ("Alpha-GPC", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("CDP-Choline", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("Centrophenoxine", new[] { neuroenhancement, memory, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("Choline Bitartrate", new[] { neuroenhancement, memory }.OfType<Category>().ToArray()),
                ("Huperzine A", new[] { neuroenhancement, memory, neuroprotection }.OfType<Category>().ToArray()),
                ("Citicoline", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                
                // Stimulants
                ("Caffeine", new[] { neuroenhancement, focus, concentration, motivation }.OfType<Category>().ToArray()),
                ("L-Theanine", new[] { neuroenhancement, focus, stressResistance, anxiety }.OfType<Category>().ToArray()),
                ("Theacrine", new[] { neuroenhancement, focus, motivation, mood }.OfType<Category>().ToArray()),
                ("Dynamine", new[] { neuroenhancement, focus, motivation }.OfType<Category>().ToArray()),
                
                // Dopaminergics
                ("L-DOPA", new[] { neuroenhancement, motivation, mood, motor }.OfType<Category>().ToArray()),
                ("L-Tyrosine", new[] { neuroenhancement, focus, motivation, stressResistance }.OfType<Category>().ToArray()),
                ("N-Acetyl L-Tyrosine", new[] { neuroenhancement, focus, motivation, stressResistance }.OfType<Category>().ToArray()),
                ("Phenylethylamine", new[] { neuroenhancement, mood, motivation }.OfType<Category>().ToArray()),
                
                // GABAergics
                ("Phenibut", new[] { neuroenhancement, anxiety, social, confidence, mood }.OfType<Category>().ToArray()),
                ("Picamilon", new[] { neuroenhancement, anxiety, focus, mood }.OfType<Category>().ToArray()),
                ("Taurine", new[] { neuroenhancement, anxiety, neuroprotection }.OfType<Category>().ToArray()),
                
                // Serotonergics
                ("5-HTP", new[] { neuroenhancement, mood, happiness, sleep }.OfType<Category>().ToArray()),
                ("L-Tryptophan", new[] { neuroenhancement, mood, happiness, sleep }.OfType<Category>().ToArray()),
                
                // Metabolics and mitochondrial enhancers
                ("Creatine", new[] { physicalEnhancement, strength, muscleGain, performance }.OfType<Category>().ToArray()),
                ("PQQ", new[] { neuroenhancement, neuroprotection, longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Nicotinamide Riboside", new[] { neuroenhancement, longevity, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                ("Acetyl-L-Carnitine", new[] { neuroenhancement, memory, neuroprotection, mitochondrialDysfunction }.OfType<Category>().ToArray()),
                
                // Essential nutrients
                ("Vitamin D", new[] { essential, mood, neuroprotection }.OfType<Category>().ToArray()),
                ("Vitamin B12", new[] { essential, neuroenhancement, memory }.OfType<Category>().ToArray()),
                ("Magnesium", new[] { essential, anxiety, sleep, neuroprotection }.OfType<Category>().ToArray()),
                ("Zinc", new[] { essential, neuroenhancement, neuroprotection }.OfType<Category>().ToArray()),
                ("Iron", new[] { essential, neuroenhancement }.OfType<Category>().ToArray()),
                
                // Others
                ("Vinpocetine", new[] { neuroenhancement, memory, neuroprotection }.OfType<Category>().ToArray()),
                ("Sulbutiamine", new[] { neuroenhancement, memory, motivation, mood }.OfType<Category>().ToArray()),
                ("Idebenone", new[] { neuroenhancement, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("Phosphatidylserine", new[] { neuroenhancement, memory, neuroprotection }.OfType<Category>().ToArray()),
                
                // Longevity compounds with nootropic effects
                ("Resveratrol", new[] { longevity, neuroprotection, chronicInflammation }.OfType<Category>().ToArray()),
                ("Quercetin", new[] { longevity, neuroprotection, chronicInflammation }.OfType<Category>().ToArray()),
                ("Curcumin", new[] { longevity, neuroprotection, chronicInflammation, mood }.OfType<Category>().ToArray()),
                ("Omega-3", new[] { essential, neuroenhancement, neuroprotection, mood }.OfType<Category>().ToArray()),
                ("Melatonin", new[] { sleep, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("CoQ10", new[] { longevity, mitochondrialDysfunction, neuroprotection }.OfType<Category>().ToArray())
            };

            // Category assignments for plants
            var plantAssignments = new List<(string Name, Category[] Categories)>
            {
                ("Ginkgo Biloba", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("Bacopa Monnieri", new[] { neuroenhancement, memory, anxiety, neuroprotection }.OfType<Category>().ToArray()),
                ("Lion's Mane Mushroom", new[] { neuroenhancement, memory, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("Rhodiola Rosea", new[] { neuroenhancement, stressResistance, mood, motivation, physicalEnhancement }.OfType<Category>().ToArray()),
                ("Ashwagandha", new[] { neuroenhancement, stressResistance, anxiety, mood, physicalEnhancement }.OfType<Category>().ToArray()),
                ("Panax Ginseng", new[] { neuroenhancement, memory, motivation, physicalEnhancement, neuroprotection }.OfType<Category>().ToArray()),
                ("Centella Asiatica", new[] { neuroenhancement, memory, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("Mucuna Pruriens", new[] { neuroenhancement, mood, motivation, motor }.OfType<Category>().ToArray()),
                ("Green Tea", new[] { neuroenhancement, focus, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("Curcuma Longa", new[] { longevity, chronicInflammation, neuroprotection }.OfType<Category>().ToArray()),
                ("Yerba Mate", new[] { neuroenhancement, focus, motivation }.OfType<Category>().ToArray()),
                ("Kanna", new[] { neuroenhancement, mood, social, anxiety }.OfType<Category>().ToArray()),
                ("Camellia Sinensis", new[] { neuroenhancement, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("Polygala Tenuifolia", new[] { neuroenhancement, memory, mood }.OfType<Category>().ToArray()),
                ("Schisandra Chinensis", new[] { neuroenhancement, neuroprotection, stressResistance, longevity }.OfType<Category>().ToArray()),
                ("Eleutherococcus Senticosus", new[] { neuroenhancement, stressResistance, physicalEnhancement }.OfType<Category>().ToArray()),
                ("Convolvulus Pluricaulis", new[] { neuroenhancement, memory, neuroprotection }.OfType<Category>().ToArray()),
                ("Mandukaparni", new[] { neuroenhancement, memory, neuroprotection }.OfType<Category>().ToArray())
            };

            // Category assignments for formulations
            var formulationAssignments = new List<(string Name, Category[] Categories)>
            {
                ("Nootropic Stack Alpha", new[] { neuroenhancement, focus, memory, concentration }.OfType<Category>().ToArray()),
                ("Cognitive Enhancement Complex", new[] { neuroenhancement, memory, focus, executiveFunction, neuroprotection }.OfType<Category>().ToArray()),
                ("Brain Boost Formula", new[] { neuroenhancement, focus, memory, motivation }.OfType<Category>().ToArray()),
                ("Focus & Memory Blend", new[] { neuroenhancement, focus, memory, concentration }.OfType<Category>().ToArray()),
                ("Racetam Complex", new[] { neuroenhancement, memory, focus, neuroprotection }.OfType<Category>().ToArray()),
                ("Choline Matrix", new[] { neuroenhancement, memory, neuroprotection }.OfType<Category>().ToArray()),
                ("Adaptogen Cognitive Blend", new[] { neuroenhancement, stressResistance, neuroprotection, longevity }.OfType<Category>().ToArray()),
                ("Liposomal Curcumin Complex", new[] { longevity, chronicInflammation, neuroprotection }.OfType<Category>().ToArray())
            };

            // Apply category assignments to compounds
            foreach (var (name, assignedCategories) in compoundAssignments)
            {
                var compound = compounds.FirstOrDefault(c => c.Name == name);
                if (compound != null && assignedCategories.Any())
                {
                    compound.Categories.Clear();
                    foreach (var category in assignedCategories)
                    {
                        compound.Categories.Add(category);
                    }
                }
            }

            // Apply category assignments to plants
            foreach (var (name, assignedCategories) in plantAssignments)
            {
                var plant = plants.FirstOrDefault(p => p.Name == name);
                if (plant != null && assignedCategories.Any())
                {
                    plant.Categories.Clear();
                    foreach (var category in assignedCategories)
                    {
                        plant.Categories.Add(category);
                    }
                }
            }

            // Apply category assignments to formulations
            foreach (var (name, assignedCategories) in formulationAssignments)
            {
                var formulation = formulations.FirstOrDefault(f => f.Name == name);
                if (formulation != null && assignedCategories.Any())
                {
                    formulation.Categories.Clear();
                    foreach (var category in assignedCategories)
                    {
                        formulation.Categories.Add(category);
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