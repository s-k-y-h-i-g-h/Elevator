namespace MauiHybridAuth.Shared.Models
{
    public class MechanismOfAction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Target { get; set; } = string.Empty; // e.g., "Dopamine D2 receptor", "VGCC L-type", "SERT"
        
        public string Action { get; set; } = string.Empty; // e.g., "Antagonist", "Blocker", "Inhibitor"
        
        // Many-to-many relationship with Interventions
        public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    }
} 