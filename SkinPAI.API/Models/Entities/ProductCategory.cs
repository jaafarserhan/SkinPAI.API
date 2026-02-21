using System.ComponentModel.DataAnnotations;

namespace SkinPAI.API.Models.Entities;

public class ProductCategory
{
    [Key]
    public Guid CategoryId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? CategoryIcon { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
