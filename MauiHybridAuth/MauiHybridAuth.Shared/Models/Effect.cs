namespace MauiHybridAuth.Shared.Models
{
    public class Effect
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Name { get; set; } = string.Empty;
    }
} 