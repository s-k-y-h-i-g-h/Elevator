namespace MauiHybridAuth.Shared.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        
        // Foreign key for parent category
        public Guid? ParentId { get; set; }
        
        // Navigation property for parent category (can be null for root categories)
        public Category? Parent { get; set; }
        
        // Navigation property for subcategories (can be empty)
        public ICollection<Category> Subcategories { get; set; } = new List<Category>();
    }
} 