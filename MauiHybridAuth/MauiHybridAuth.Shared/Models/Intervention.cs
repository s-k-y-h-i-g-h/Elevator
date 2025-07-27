namespace MauiHybridAuth.Shared.Models
{
    public abstract class Intervention
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        
        // Many-to-many relationship with Categories
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        
        // Collection of mechanisms of action
        public ICollection<MechanismOfAction> MechanismsOfAction { get; set; } = new List<MechanismOfAction>();
        
        // Collection of effects
        public ICollection<Effect> Effects { get; set; } = new List<Effect>();
    }
} 