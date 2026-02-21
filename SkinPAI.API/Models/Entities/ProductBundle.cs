using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class ProductBundle
{
    [Key]
    public Guid BundleId { get; set; } = Guid.NewGuid();

    public Guid BrandId { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal BundlePrice { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal OriginalPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Savings { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public string? Benefits { get; set; } // JSON array

    public bool IsCustomized { get; set; } = false;

    public string? ForSkinTypes { get; set; } // JSON array

    public string? ForSkinConcerns { get; set; } // JSON array

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BrandId))]
    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<ProductBundleItem> BundleItems { get; set; } = new List<ProductBundleItem>();
}
