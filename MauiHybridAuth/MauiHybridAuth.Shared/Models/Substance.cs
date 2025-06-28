namespace MauiHybridAuth.Shared.Models;

public abstract class Substance : Intervention
{
    public int DurationInMinutes { get; set; } = default!;
    public string DoseRange { get; set; } = default!;
    
    // Classification tags - allows multiple tags from the predefined set
    public ICollection<ClassificationTag> ClassificationTags { get; set; } = new List<ClassificationTag>();
}