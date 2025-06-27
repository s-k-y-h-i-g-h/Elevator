using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class Category
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;
    
    // Self-referencing relationship for subcategories
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    
    // Navigation properties
    public ICollection<Category> Subcategories { get; set; } = new List<Category>();
    public ICollection<InterventionCategory> InterventionCategories { get; set; } = new List<InterventionCategory>();
} 