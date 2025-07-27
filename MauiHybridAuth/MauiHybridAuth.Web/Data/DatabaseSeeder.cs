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

            // Process each intervention type separately to avoid MARS issues
            await SeedCompoundCategoriesAsync(context, cancellationToken);
            await SeedPlantCategoriesAsync(context, cancellationToken);
            await SeedFormulationCategoriesAsync(context, cancellationToken);
        }

        private static async Task SeedCompoundCategoriesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            var compounds = await context.Compounds.ToListAsync(cancellationToken);
            var categories = await context.Categories.ToListAsync(cancellationToken);

            if (!compounds.Any() || !categories.Any())
                return;

            // Find specific categories for assignment
            var categoryLookup = categories.ToLookup(c => c.Name, c => c);

            // Get category references
            var GetCategory = (string name) => categoryLookup[name].FirstOrDefault();

            // Comprehensive category assignments for compounds
            var compoundAssignments = new List<(string Name, string[] CategoryNames)>
            {
                // Racetams - primarily memory and learning enhancers
                ("Piracetam", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                ("Aniracetam", new[] { "Neuroenhancement", "Memory", "Creativity", "Mood", "Anxiety" }),
                ("Oxiracetam", new[] { "Neuroenhancement", "Memory", "Focus", "Concentration", "Reasoning" }),
                ("Pramiracetam", new[] { "Neuroenhancement", "Memory", "Focus", "Concentration" }),
                ("Phenylpiracetam", new[] { "Neuroenhancement", "Memory", "Focus", "Motivation", "Physical Enhancement" }),
                ("Nefiracetam", new[] { "Neuroenhancement", "Memory", "Neuroprotection", "Depression" }),
                ("Coluracetam", new[] { "Neuroenhancement", "Memory", "Perception", "Depression" }),
                
                // Ampakines - cognitive enhancers
                ("Sunifiram", new[] { "Neuroenhancement", "Memory", "Focus" }),
                ("Unifiram", new[] { "Neuroenhancement", "Memory", "Focus" }),
                
                // Noopept and related peptides
                ("Noopept", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                ("PRL-8-53", new[] { "Neuroenhancement", "Memory" }),
                ("NSI-189", new[] { "Neuroenhancement", "Depression", "Mood", "Neuroprotection" }),
                
                // Eugeroics - wakefulness promoters
                ("Modafinil", new[] { "Neuroenhancement", "Focus", "Motivation", "Concentration" }),
                ("Armodafinil", new[] { "Neuroenhancement", "Focus", "Motivation", "Concentration" }),
                ("Adrafinil", new[] { "Neuroenhancement", "Focus", "Motivation" }),
                ("Fluorenol", new[] { "Neuroenhancement", "Focus" }),
                
                // Cholinergics - acetylcholine system enhancers
                ("Alpha-GPC", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                ("CDP-Choline", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                ("Centrophenoxine", new[] { "Neuroenhancement", "Memory", "Neuroprotection" }),
                ("Choline Bitartrate", new[] { "Neuroenhancement", "Memory" }),
                ("Huperzine A", new[] { "Neuroenhancement", "Memory", "Neuroprotection" }),
                ("Citicoline", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                
                // Stimulants
                ("Caffeine", new[] { "Neuroenhancement", "Focus", "Concentration", "Motivation" }),
                ("L-Theanine", new[] { "Neuroenhancement", "Focus", "Stress Resistance", "Anxiety" }),
                ("Theacrine", new[] { "Neuroenhancement", "Focus", "Motivation", "Mood" }),
                ("Dynamine", new[] { "Neuroenhancement", "Focus", "Motivation" }),
                
                // Dopaminergics
                ("L-DOPA", new[] { "Neuroenhancement", "Motivation", "Mood", "Motor" }),
                ("L-Tyrosine", new[] { "Neuroenhancement", "Focus", "Motivation", "Stress Resistance" }),
                ("N-Acetyl L-Tyrosine", new[] { "Neuroenhancement", "Focus", "Motivation", "Stress Resistance" }),
                ("Phenylethylamine", new[] { "Neuroenhancement", "Mood", "Motivation" }),
                
                // GABAergics
                ("Phenibut", new[] { "Neuroenhancement", "Anxiety", "Social", "Confidence", "Mood" }),
                ("Picamilon", new[] { "Neuroenhancement", "Anxiety", "Focus", "Mood" }),
                ("Taurine", new[] { "Neuroenhancement", "Anxiety", "Neuroprotection" }),
                
                // Serotonergics
                ("5-HTP", new[] { "Neuroenhancement", "Mood", "Happiness", "Sleep" }),
                ("L-Tryptophan", new[] { "Neuroenhancement", "Mood", "Happiness", "Sleep" }),
                
                // Metabolics and mitochondrial enhancers
                ("Creatine", new[] { "Physical Enhancement", "Strength", "Muscle Gain", "Performance" }),
                ("PQQ", new[] { "Neuroenhancement", "Neuroprotection", "Longevity" }),
                ("Nicotinamide Riboside", new[] { "Neuroenhancement", "Longevity" }),
                ("Acetyl-L-Carnitine", new[] { "Neuroenhancement", "Memory", "Neuroprotection", "Longevity" }),
                
                // Essential nutrients
                ("Vitamin D", new[] { "Essential", "Mood", "Neuroprotection" }),
                ("Vitamin B12", new[] { "Essential", "Neuroenhancement", "Memory" }),
                ("Magnesium", new[] { "Essential", "Anxiety", "Sleep", "Neuroprotection" }),
                ("Zinc", new[] { "Essential", "Neuroenhancement", "Neuroprotection" }),
                ("Iron", new[] { "Essential", "Neuroenhancement" }),
                
                // Others
                ("Vinpocetine", new[] { "Neuroenhancement", "Memory", "Neuroprotection" }),
                ("Sulbutiamine", new[] { "Neuroenhancement", "Memory", "Motivation", "Mood" }),
                ("Idebenone", new[] { "Neuroenhancement", "Neuroprotection", "Longevity" }),
                ("Phosphatidylserine", new[] { "Neuroenhancement", "Memory", "Neuroprotection" }),
                
                // Longevity compounds with nootropic effects
                ("Resveratrol", new[] { "Longevity" }),
                ("Quercetin", new[] { "Longevity" }),
                ("Curcumin", new[] { "Longevity" }),
                ("Omega-3", new[] { "Essential", "Neuroenhancement", "Neuroprotection", "Mood" }),
                ("Melatonin", new[] { "Sleep", "Neuroprotection", "Longevity" }),
                ("CoQ10", new[] { "Longevity" })
            };

            // Apply category assignments to compounds
            foreach (var (name, categoryNames) in compoundAssignments)
            {
                var compound = compounds.FirstOrDefault(c => c.Name == name);
                if (compound != null)
                {
                    compound.Categories.Clear();
                    foreach (var categoryName in categoryNames)
                    {
                        var category = GetCategory(categoryName);
                        if (category != null)
                        {
                            compound.Categories.Add(category);
                        }
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedPlantCategoriesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            var plants = await context.Plants.ToListAsync(cancellationToken);
            var categories = await context.Categories.ToListAsync(cancellationToken);

            if (!plants.Any() || !categories.Any())
                return;

            var categoryLookup = categories.ToLookup(c => c.Name, c => c);
            var GetCategory = (string name) => categoryLookup[name].FirstOrDefault();

            // Category assignments for plants
            var plantAssignments = new List<(string Name, string[] CategoryNames)>
            {
                ("Ginkgo Biloba", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                ("Bacopa Monnieri", new[] { "Neuroenhancement", "Memory", "Anxiety", "Neuroprotection" }),
                ("Lion's Mane Mushroom", new[] { "Neuroenhancement", "Memory", "Neuroprotection", "Longevity" }),
                ("Rhodiola Rosea", new[] { "Neuroenhancement", "Stress Resistance", "Mood", "Motivation", "Physical Enhancement" }),
                ("Ashwagandha", new[] { "Neuroenhancement", "Stress Resistance", "Anxiety", "Mood", "Physical Enhancement" }),
                ("Panax Ginseng", new[] { "Neuroenhancement", "Memory", "Motivation", "Physical Enhancement", "Neuroprotection" }),
                ("Centella Asiatica", new[] { "Neuroenhancement", "Memory", "Neuroprotection", "Longevity" }),
                ("Mucuna Pruriens", new[] { "Neuroenhancement", "Mood", "Motivation", "Motor" }),
                ("Green Tea", new[] { "Neuroenhancement", "Focus", "Neuroprotection", "Longevity" }),
                ("Curcuma Longa", new[] { "Longevity", "Neuroprotection" }),
                ("Yerba Mate", new[] { "Neuroenhancement", "Focus", "Motivation" }),
                ("Kanna", new[] { "Neuroenhancement", "Mood", "Social", "Anxiety" }),
                ("Camellia Sinensis", new[] { "Neuroenhancement", "Focus", "Neuroprotection" }),
                ("Polygala Tenuifolia", new[] { "Neuroenhancement", "Memory", "Mood" }),
                ("Schisandra Chinensis", new[] { "Neuroenhancement", "Neuroprotection", "Stress Resistance", "Longevity" }),
                ("Eleutherococcus Senticosus", new[] { "Neuroenhancement", "Stress Resistance", "Physical Enhancement" }),
                ("Convolvulus Pluricaulis", new[] { "Neuroenhancement", "Memory", "Neuroprotection" }),
                ("Mandukaparni", new[] { "Neuroenhancement", "Memory", "Neuroprotection" })
            };

            // Apply category assignments to plants
            foreach (var (name, categoryNames) in plantAssignments)
            {
                var plant = plants.FirstOrDefault(p => p.Name == name);
                if (plant != null)
                {
                    plant.Categories.Clear();
                    foreach (var categoryName in categoryNames)
                    {
                        var category = GetCategory(categoryName);
                        if (category != null)
                        {
                            plant.Categories.Add(category);
                        }
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedFormulationCategoriesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            var formulations = await context.Formulations.ToListAsync(cancellationToken);
            var categories = await context.Categories.ToListAsync(cancellationToken);

            if (!formulations.Any() || !categories.Any())
                return;

            var categoryLookup = categories.ToLookup(c => c.Name, c => c);
            var GetCategory = (string name) => categoryLookup[name].FirstOrDefault();

            // Category assignments for formulations
            var formulationAssignments = new List<(string Name, string[] CategoryNames)>
            {
                ("Nootropic Stack Alpha", new[] { "Neuroenhancement", "Focus", "Memory", "Concentration" }),
                ("Cognitive Enhancement Complex", new[] { "Neuroenhancement", "Memory", "Focus", "Executive Function", "Neuroprotection" }),
                ("Brain Boost Formula", new[] { "Neuroenhancement", "Focus", "Memory", "Motivation" }),
                ("Focus & Memory Blend", new[] { "Neuroenhancement", "Focus", "Memory", "Concentration" }),
                ("Racetam Complex", new[] { "Neuroenhancement", "Memory", "Focus", "Neuroprotection" }),
                ("Choline Matrix", new[] { "Neuroenhancement", "Memory", "Neuroprotection" }),
                ("Adaptogen Cognitive Blend", new[] { "Neuroenhancement", "Stress Resistance", "Neuroprotection", "Longevity" }),
                ("Liposomal Curcumin Complex", new[] { "Longevity" })
            };

            // Apply category assignments to formulations
            foreach (var (name, categoryNames) in formulationAssignments)
            {
                var formulation = formulations.FirstOrDefault(f => f.Name == name);
                if (formulation != null)
                {
                    formulation.Categories.Clear();
                    foreach (var categoryName in categoryNames)
                    {
                        var category = GetCategory(categoryName);
                        if (category != null)
                        {
                            formulation.Categories.Add(category);
                        }
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