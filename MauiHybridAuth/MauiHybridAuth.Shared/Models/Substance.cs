namespace MauiHybridAuth.Shared.Models
{
    public abstract class Substance : Intervention
    {
        public string Duration { get; set; } = string.Empty;
        public string DoseRange { get; set; } = string.Empty;
        public List<ClassificationTag> ClassificationTags { get; set; } = new List<ClassificationTag>();
    }
} 