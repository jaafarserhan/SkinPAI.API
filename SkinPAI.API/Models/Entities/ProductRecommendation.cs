using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class ProductRecommendation
{
    [Key]
    public Guid RecommendationId { get; set; } = Guid.NewGuid();

    public Guid ScanId { get; set; }

    public Guid ProductId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal RecommendationScore { get; set; } // 0-100

    [MaxLength(500)]
    public string? RecommendationReason { get; set; }

    public int Priority { get; set; } = 0; // Higher = more important

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ScanId))]
    public virtual SkinScan SkinScan { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}
