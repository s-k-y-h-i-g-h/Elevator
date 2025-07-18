namespace MauiHybridAuth.Shared.Models
{
    public abstract class Intervention
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }
} 