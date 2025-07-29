using MauiHybridAuth.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedAllSync(ApplicationDbContext context)
        {
            // Check if categories already exist
            if (context.Categories.Any())
                return;

            context.Categories.AddRange(GenerateTestCategories());
            context.SaveChanges();
        }

        public static async Task SeedAllAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if categories already exist
            if (await context.Categories.AnyAsync(cancellationToken))
                return;

            context.Categories.AddRange(GenerateTestCategories());
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