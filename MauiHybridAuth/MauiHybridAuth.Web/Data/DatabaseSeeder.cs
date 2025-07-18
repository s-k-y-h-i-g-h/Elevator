using MauiHybridAuth.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MauiHybridAuth.Web.Data
{
    public static class DatabaseSeeder
    {
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

        public static async Task SeedCompoundsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // Check if compounds already exist
            if (await context.Compounds.AnyAsync(cancellationToken))
                return;

            var compounds = GenerateTestCompounds(100);
            context.Compounds.AddRange(compounds);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
} 