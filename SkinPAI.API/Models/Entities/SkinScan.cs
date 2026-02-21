using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class SkinScan
{
    [Key]
    public Guid ScanId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(500)]
    public string ScanImageUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? OverlayImageUrl { get; set; }

    [Required, MaxLength(50)]
    public string ScanType { get; set; } = "Face"; // Face, Forehead, Cheeks, etc.

    public DateTime ScanDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string AIProcessingStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Failed

    public DateTime? AIProcessingStartedAt { get; set; }

    public DateTime? AIProcessingCompletedAt { get; set; }

    [MaxLength(50)]
    public string? AIModelVersion { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? OverallScore { get; set; } // 0-100

    public int? EstimatedSkinAge { get; set; }

    public int? ActualAge { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public virtual SkinAnalysisResult? AnalysisResult { get; set; }

    public virtual ICollection<ProductRecommendation> ProductRecommendations { get; set; } = new List<ProductRecommendation>();
}
