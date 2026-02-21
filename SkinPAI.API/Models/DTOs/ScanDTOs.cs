namespace SkinPAI.API.Models.DTOs;

// ==================== Scan DTOs ====================
public record SkinScanDto(
    Guid ScanId,
    Guid UserId,
    string ScanImageUrl,
    string? OverlayImageUrl,
    string ScanType,
    DateTime ScanDate,
    string AIProcessingStatus,
    decimal? OverallScore,
    int? EstimatedSkinAge,
    int? ActualAge,
    SkinAnalysisResultDto? AnalysisResult,
    List<ProductRecommendationDto>? ProductRecommendations
);

public record CreateScanRequest(
    string ImageBase64,
    string ScanType = "Face",
    int? ActualAge = null
);

public record SkinAnalysisResultDto(
    string? SkinType,
    // Health Metrics
    decimal? Hydration,
    decimal? Moisture,
    decimal? Oiliness,
    decimal? Evenness,
    decimal? Texture,
    decimal? Clarity,
    decimal? Firmness,
    decimal? Elasticity,
    decimal? PoreSize,
    decimal? Smoothness,
    decimal? Radiance,
    // Concerns
    decimal? AcneSeverity,
    decimal? WrinklesSeverity,
    decimal? DarkSpotsSeverity,
    decimal? RednessLevel,
    decimal? DarkCircles,
    decimal? UVDamage,
    // Analysis
    string? TopConcerns,
    string? RecommendedIngredients,
    string? AIAnalysisText,
    string[]? Recommendations,
    decimal? ConfidenceScore,
    bool FaceDetected,
    int? FaceLandmarksCount
);

public record DailyScanUsageDto(
    DateOnly ScanDate,
    int ScanCount,
    int MaxScans,
    int RemainingScans
);
