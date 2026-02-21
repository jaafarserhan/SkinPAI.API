using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class Product
{
    [Key]
    public Guid ProductId { get; set; } = Guid.NewGuid();

    public Guid BrandId { get; set; }

    public Guid CategoryId { get; set; }

    public Guid? DistributorId { get; set; }

    [Required, MaxLength(300)]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ProductImageUrl { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OriginalPrice { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [MaxLength(500)]
    public string? ShopUrl { get; set; }

    [MaxLength(500)]
    public string? AffiliateUrl { get; set; }

    // Specific affiliate store links
    [MaxLength(500)]
    public string? AmazonAffiliateUrl { get; set; }

    [MaxLength(500)]
    public string? SephoraAffiliateUrl { get; set; }

    [MaxLength(500)]
    public string? UltaAffiliateUrl { get; set; }

    [MaxLength(50)]
    public string? Volume { get; set; } // e.g., "50ml", "1oz"

    // Product Details (JSON arrays)
    public string? KeyIngredients { get; set; }
    public string? SkinTypes { get; set; } // JSON array
    public string? SkinConcerns { get; set; } // JSON array

    // Ratings & Reviews
    [Column(TypeName = "decimal(3,2)")]
    public decimal? AverageRating { get; set; }

    public int TotalReviews { get; set; } = 0;

    // Inventory
    public bool InStock { get; set; } = true;
    public int? StockQuantity { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsRecommended { get; set; } = false;

    public int? DiscountPercent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BrandId))]
    public virtual Brand Brand { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public virtual ProductCategory Category { get; set; } = null!;

    [ForeignKey(nameof(DistributorId))]
    public virtual Distributor? Distributor { get; set; }

    public virtual ICollection<ProductRecommendation> ProductRecommendations { get; set; } = new List<ProductRecommendation>();
    public virtual ICollection<UserProductFavorite> UserFavorites { get; set; } = new List<UserProductFavorite>();
    public virtual ICollection<ProductBundleItem> BundleItems { get; set; } = new List<ProductBundleItem>();
}
