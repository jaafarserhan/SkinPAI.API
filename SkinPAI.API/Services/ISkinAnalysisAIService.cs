namespace SkinPAI.API.Services;

/// <summary>
/// Interface for AI-powered skin analysis service
/// </summary>
public interface ISkinAnalysisAIService
{
    /// <summary>
    /// Analyze a skin image using AI and return detailed skin metrics
    /// </summary>
    /// <param name="imageBase64">Base64 encoded image data</param>
    /// <param name="imageUrl">Optional URL of the stored image</param>
    /// <param name="actualAge">User's actual age for comparison</param>
    /// <returns>AI analysis results with skin metrics</returns>
    Task<SkinAIAnalysisResult> AnalyzeSkinAsync(string imageBase64, string? imageUrl = null, int? actualAge = null);
}

/// <summary>
/// Result from AI skin analysis
/// </summary>
public class SkinAIAnalysisResult
{
    // Skin Type
    public string SkinType { get; set; } = "normal";
    
    // Health Metrics (0-100)
    public decimal Hydration { get; set; }
    public decimal Moisture { get; set; }
    public decimal Oiliness { get; set; }
    public decimal Evenness { get; set; }
    public decimal Texture { get; set; }
    public decimal Clarity { get; set; }
    public decimal Firmness { get; set; }
    public decimal Elasticity { get; set; }
    public decimal PoreSize { get; set; }
    public decimal Smoothness { get; set; }
    public decimal Radiance { get; set; }
    
    // Concerns (0-100 severity)
    public decimal AcneSeverity { get; set; }
    public decimal WrinklesSeverity { get; set; }
    public decimal DarkSpotsSeverity { get; set; }
    public decimal RednessLevel { get; set; }
    public decimal DarkCircles { get; set; }
    public decimal UVDamage { get; set; }
    
    // AI Generated Content
    public string[] TopConcerns { get; set; } = Array.Empty<string>();
    public string[] RecommendedIngredients { get; set; } = Array.Empty<string>();
    public string[] IngredientsToAvoid { get; set; } = Array.Empty<string>();
    public string AIAnalysisText { get; set; } = string.Empty;
    public string RawAIResponse { get; set; } = string.Empty;
    
    // Metadata
    public int ConfidenceScore { get; set; }
    public bool FaceDetected { get; set; }
    public int EstimatedSkinAge { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
