using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class SkinAnalysisResult
{
    [Key]
    public Guid AnalysisId { get; set; } = Guid.NewGuid();

    public Guid ScanId { get; set; }

    [MaxLength(50)]
    public string? SkinType { get; set; } // Oily, Dry, Combination, Normal, Sensitive

    // Skin Health Metrics (0-100)
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Hydration { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Moisture { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Oiliness { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Evenness { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Texture { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Clarity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Firmness { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Elasticity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? PoreSize { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Smoothness { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Radiance { get; set; }

    // Skin Concerns (0-100 severity)
    [Column(TypeName = "decimal(5,2)")]
    public decimal? AcneSeverity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? WrinklesSeverity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DarkSpotsSeverity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? RednessLevel { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DarkCircles { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? UVDamage { get; set; }

    // AI Analysis
    public string? TopConcerns { get; set; } // JSON array

    public string? RecommendedIngredients { get; set; } // JSON array

    public string? IngredientsToAvoid { get; set; } // JSON array

    public string? RoutineRecommendations { get; set; } // JSON

    public string? AIAnalysisText { get; set; }

    public string? RawAIResponse { get; set; } // Full JSON response from AI

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ConfidenceScore { get; set; }

    public bool FaceDetected { get; set; } = false;

    public int? FaceLandmarksCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ScanId))]
    public virtual SkinScan SkinScan { get; set; } = null!;
}
