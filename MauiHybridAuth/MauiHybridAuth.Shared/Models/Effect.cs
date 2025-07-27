namespace MauiHybridAuth.Shared.Models
{
    public class Effect
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Name { get; set; } = string.Empty;
        
        // Many-to-many relationship with Interventions
        public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    }
} 