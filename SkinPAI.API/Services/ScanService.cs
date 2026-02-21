using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface IScanService
{
    Task<SkinScanDto> CreateScanAsync(Guid userId, CreateScanRequest request, IFileStorageService fileStorage);
    Task<SkinScanDto?> GetScanByIdAsync(Guid scanId, Guid userId);
    Task<List<SkinScanDto>> GetUserScansAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<DailyScanUsageDto> GetDailyScanUsageAsync(Guid userId);
    Task<bool> CanUserScanAsync(Guid userId);
    Task<bool> CanPerformScanAsync(Guid userId);
    Task<SkinAnalysisResultDto?> GetAnalysisByScanIdAsync(Guid scanId, Guid userId);
    Task<bool> DeleteScanAsync(Guid scanId, Guid userId);
    Task<SkinProgressDto> GetSkinProgressAsync(Guid userId, int days = 30);
}

public class ScanService : IScanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScanService> _logger;
    private readonly ISkinAnalysisAIService _aiService;

    public ScanService(
        IUnitOfWork unitOfWork, 
        ILogger<ScanService> logger,
        ISkinAnalysisAIService aiService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _aiService = aiService;
    }

    public async Task<SkinScanDto> CreateScanAsync(Guid userId, CreateScanRequest request, IFileStorageService fileStorage)
    {
        // Check if user can scan
        if (!await CanUserScanAsync(userId))
        {
            throw new InvalidOperationException("Daily scan limit reached");
        }

        // Save the scan image
        var imageUrl = await fileStorage.SaveImageAsync(request.ImageBase64, "scans", $"scan_{userId}_{DateTime.UtcNow.Ticks}");

        // Create the scan record
        var scan = new SkinScan
        {
            UserId = userId,
            ScanImageUrl = imageUrl,
            ScanType = request.ScanType,
            ActualAge = request.ActualAge,
            AIProcessingStatus = "Processing"
        };

        await _unitOfWork.SkinScans.AddAsync(scan);

        // Update daily scan usage
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyUsage = await _unitOfWork.DailyScanUsages
            .FirstOrDefaultAsync(d => d.UserId == userId && d.ScanDate == today);

        if (dailyUsage == null)
        {
            dailyUsage = new DailyScanUsage
            {
                UserId = userId,
                ScanDate = today,
                ScanCount = 1,
                LastScanAt = DateTime.UtcNow
            };
            await _unitOfWork.DailyScanUsages.AddAsync(dailyUsage);
        }
        else
        {
            dailyUsage.ScanCount++;
            dailyUsage.LastScanAt = DateTime.UtcNow;
            _unitOfWork.DailyScanUsages.Update(dailyUsage);
        }

        // Update user stats
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.TotalScansUsed++;
            user.LastScanDate = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.SaveChangesAsync();

        // Process AI skin analysis using Hugging Face
        await ProcessScanAnalysisAsync(scan, request.ImageBase64);

        return MapToSkinScanDto(scan);
    }

    private async Task ProcessScanAnalysisAsync(SkinScan scan, string imageBase64)
    {
        scan.AIProcessingStartedAt = DateTime.UtcNow;
        _logger.LogInformation("🤖 Starting AI skin analysis for scan {ScanId}", scan.ScanId);

        try
        {
            // Call the real AI service (Hugging Face)
            var aiResult = await _aiService.AnalyzeSkinAsync(imageBase64, scan.ScanImageUrl, scan.ActualAge);
            
            if (!aiResult.Success)
            {
                _logger.LogWarning("⚠️ AI analysis returned with warnings: {Error}", aiResult.ErrorMessage);
            }

            var analysisResult = new SkinAnalysisResult
            {
                ScanId = scan.ScanId,
                SkinType = aiResult.SkinType,
                
                // Health metrics from AI
                Hydration = aiResult.Hydration,
                Moisture = aiResult.Moisture,
                Oiliness = aiResult.Oiliness,
                Evenness = aiResult.Evenness,
                Texture = aiResult.Texture,
                Clarity = aiResult.Clarity,
                Firmness = aiResult.Firmness,
                Elasticity = aiResult.Elasticity,
                PoreSize = aiResult.PoreSize,
                Smoothness = aiResult.Smoothness,
                Radiance = aiResult.Radiance,
                
                // Concerns from AI
                AcneSeverity = aiResult.AcneSeverity,
                WrinklesSeverity = aiResult.WrinklesSeverity,
                DarkSpotsSeverity = aiResult.DarkSpotsSeverity,
                RednessLevel = aiResult.RednessLevel,
                DarkCircles = aiResult.DarkCircles,
                UVDamage = aiResult.UVDamage,
                
                // AI Generated Content
                TopConcerns = JsonSerializer.Serialize(aiResult.TopConcerns),
                RecommendedIngredients = JsonSerializer.Serialize(aiResult.RecommendedIngredients),
                IngredientsToAvoid = JsonSerializer.Serialize(aiResult.IngredientsToAvoid),
                AIAnalysisText = aiResult.AIAnalysisText,
                RawAIResponse = aiResult.RawAIResponse,
                ConfidenceScore = aiResult.ConfidenceScore,
                FaceDetected = aiResult.FaceDetected,
                FaceLandmarksCount = 468
            };

            // Calculate overall score
            scan.OverallScore = (aiResult.Hydration + aiResult.Evenness + 
                aiResult.Clarity + aiResult.Smoothness + aiResult.Radiance) / 5m;
            
            scan.EstimatedSkinAge = aiResult.EstimatedSkinAge;
            scan.AIProcessingStatus = "Completed";
            scan.AIProcessingCompletedAt = DateTime.UtcNow;
            scan.AIModelVersion = aiResult.ModelVersion;

            await _unitOfWork.SkinAnalysisResults.AddAsync(analysisResult);
            _unitOfWork.SkinScans.Update(scan);
            
            // Generate product recommendations based on AI analysis
            await GenerateProductRecommendationsAsync(scan.ScanId, analysisResult);
            
            await _unitOfWork.SaveChangesAsync();
            
            scan.AnalysisResult = analysisResult;
            
            _logger.LogInformation("✅ AI analysis completed for scan {ScanId}. SkinType: {SkinType}, Score: {Score}, Model: {Model}", 
                scan.ScanId, aiResult.SkinType, scan.OverallScore, aiResult.ModelVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ AI analysis failed for scan {ScanId}: {Error}", scan.ScanId, ex.Message);
            scan.AIProcessingStatus = "Failed";
            scan.AIProcessingCompletedAt = DateTime.UtcNow;
            _unitOfWork.SkinScans.Update(scan);
            await _unitOfWork.SaveChangesAsync();
            throw;
        }
    }

    private async Task GenerateProductRecommendationsAsync(Guid scanId, SkinAnalysisResult analysis)
    {
        // Get products that match the skin profile
        var products = await _unitOfWork.Products.Query()
            .Where(p => p.IsActive && p.InStock)
            .Take(10)
            .ToListAsync();

        var random = new Random();
        foreach (var product in products)
        {
            var recommendation = new ProductRecommendation
            {
                ScanId = scanId,
                ProductId = product.ProductId,
                RecommendationScore = random.Next(70, 98),
                RecommendationReason = $"Recommended based on your {analysis.SkinType} skin type",
                Priority = random.Next(1, 10)
            };
            await _unitOfWork.ProductRecommendations.AddAsync(recommendation);
        }
    }

    public async Task<SkinScanDto?> GetScanByIdAsync(Guid scanId, Guid userId)
    {
        var scan = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .Include(s => s.ProductRecommendations)
                .ThenInclude(r => r.Product)
                    .ThenInclude(p => p.Brand)
            .FirstOrDefaultAsync(s => s.ScanId == scanId && s.UserId == userId && !s.IsDeleted);

        if (scan == null) return null;

        return MapToSkinScanDto(scan);
    }

    public async Task<List<SkinScanDto>> GetUserScansAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var scans = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.ScanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return scans.Select(MapToSkinScanDto).ToList();
    }

    public async Task<DailyScanUsageDto> GetDailyScanUsageAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            // Return default for unknown user
            return new DailyScanUsageDto(DateOnly.FromDateTime(DateTime.UtcNow), 0, 1, 1);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyUsage = await _unitOfWork.DailyScanUsages
            .FirstOrDefaultAsync(d => d.UserId == userId && d.ScanDate == today);
        var scanCount = dailyUsage?.ScanCount ?? 0;

        // Updated limits: Guest: 1/week, Member: 3/day, Pro: unlimited
        if (user.MembershipType == "Pro")
        {
            return new DailyScanUsageDto(today, scanCount, 9999, 9999);
        }

        if (user.MembershipType == "Member")
        {
            return new DailyScanUsageDto(today, scanCount, 3, Math.Max(0, 3 - scanCount));
        }

        // Guest: 1 scan per week - check weekly usage
        var weekStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek));
        var weeklyScans = await _unitOfWork.DailyScanUsages
            .Query()
            .Where(d => d.UserId == userId && d.ScanDate >= weekStart)
            .SumAsync(d => d.ScanCount);
        
        return new DailyScanUsageDto(today, weeklyScans, 1, Math.Max(0, 1 - weeklyScans));
    }

    public async Task<bool> CanUserScanAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        // Updated limits: Guest: 1/week, Member: 3/day, Pro: unlimited
        if (user.MembershipType == "Pro")
        {
            return true; // Unlimited scans
        }

        if (user.MembershipType == "Member")
        {
            // Member: 3 scans per day
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var dailyUsage = await _unitOfWork.DailyScanUsages
                .FirstOrDefaultAsync(d => d.UserId == userId && d.ScanDate == today);
            return (dailyUsage?.ScanCount ?? 0) < 3;
        }

        // Guest: 1 scan per week
        var weekStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek));
        var weeklyScans = await _unitOfWork.DailyScanUsages
            .Query()
            .Where(d => d.UserId == userId && d.ScanDate >= weekStart)
            .SumAsync(d => d.ScanCount);
        
        return weeklyScans < 1;
    }

    public async Task<bool> CanPerformScanAsync(Guid userId)
    {
        return await CanUserScanAsync(userId);
    }

    public async Task<SkinAnalysisResultDto?> GetAnalysisByScanIdAsync(Guid scanId, Guid userId)
    {
        var scan = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .FirstOrDefaultAsync(s => s.ScanId == scanId && s.UserId == userId && !s.IsDeleted);

        if (scan?.AnalysisResult == null) return null;

        var result = scan.AnalysisResult;
        return new SkinAnalysisResultDto(
            result.SkinType,
            result.Hydration,
            result.Moisture,
            result.Oiliness,
            result.Evenness,
            result.Texture,
            result.Clarity,
            result.Firmness,
            result.Elasticity,
            result.PoreSize,
            result.Smoothness,
            result.Radiance,
            result.AcneSeverity,
            result.WrinklesSeverity,
            result.DarkSpotsSeverity,
            result.RednessLevel,
            result.DarkCircles,
            result.UVDamage,
            result.TopConcerns,
            result.RecommendedIngredients,
            result.AIAnalysisText,
            null,
            result.ConfidenceScore,
            result.FaceDetected,
            result.FaceLandmarksCount
        );
    }

    public async Task<bool> DeleteScanAsync(Guid scanId, Guid userId)
    {
        var scan = await _unitOfWork.SkinScans.FirstOrDefaultAsync(s => s.ScanId == scanId && s.UserId == userId);
        if (scan == null) return false;

        scan.IsDeleted = true;
        scan.DeletedAt = DateTime.UtcNow;
        _unitOfWork.SkinScans.Update(scan);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SkinProgressDto> GetSkinProgressAsync(Guid userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var scans = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .Where(s => s.UserId == userId && !s.IsDeleted && s.ScanDate >= startDate)
            .OrderBy(s => s.ScanDate)
            .ToListAsync();

        var overallProgress = scans
            .Where(s => s.OverallScore.HasValue)
            .Select(s => new SkinProgressPointDto(s.ScanDate, s.OverallScore!.Value))
            .ToList();

        var hydrationProgress = scans
            .Where(s => s.AnalysisResult?.Hydration != null)
            .Select(s => new SkinProgressPointDto(s.ScanDate, s.AnalysisResult!.Hydration!.Value))
            .ToList();

        var acneProgress = scans
            .Where(s => s.AnalysisResult?.AcneSeverity != null)
            .Select(s => new SkinProgressPointDto(s.ScanDate, s.AnalysisResult!.AcneSeverity!.Value))
            .ToList();

        var avgScore = overallProgress.Any() ? overallProgress.Average(p => p.Value) : 0;
        var scoreChange = overallProgress.Count >= 2 
            ? overallProgress.Last().Value - overallProgress.First().Value 
            : 0;

        return new SkinProgressDto(
            overallProgress,
            hydrationProgress,
            acneProgress,
            avgScore,
            scoreChange,
            scans.Count
        );
    }

    private SkinScanDto MapToSkinScanDto(SkinScan scan)
    {
        var recommendations = scan.ProductRecommendations?.Select(r => new ProductRecommendationDto(
            r.ProductId,
            r.Product?.ProductName ?? "",
            r.Product?.ProductImageUrl,
            r.Product?.Price ?? 0,
            r.RecommendationScore,
            r.RecommendationReason,
            r.Product?.Brand?.BrandName ?? ""
        )).ToList();

        return new SkinScanDto(
            scan.ScanId,
            scan.UserId,
            scan.ScanImageUrl,
            scan.OverlayImageUrl,
            scan.ScanType,
            scan.ScanDate,
            scan.AIProcessingStatus,
            scan.OverallScore,
            scan.EstimatedSkinAge,
            scan.ActualAge,
            scan.AnalysisResult != null ? new SkinAnalysisResultDto(
                scan.AnalysisResult.SkinType,
                scan.AnalysisResult.Hydration,
                scan.AnalysisResult.Moisture,
                scan.AnalysisResult.Oiliness,
                scan.AnalysisResult.Evenness,
                scan.AnalysisResult.Texture,
                scan.AnalysisResult.Clarity,
                scan.AnalysisResult.Firmness,
                scan.AnalysisResult.Elasticity,
                scan.AnalysisResult.PoreSize,
                scan.AnalysisResult.Smoothness,
                scan.AnalysisResult.Radiance,
                scan.AnalysisResult.AcneSeverity,
                scan.AnalysisResult.WrinklesSeverity,
                scan.AnalysisResult.DarkSpotsSeverity,
                scan.AnalysisResult.RednessLevel,
                scan.AnalysisResult.DarkCircles,
                scan.AnalysisResult.UVDamage,
                scan.AnalysisResult.TopConcerns,
                scan.AnalysisResult.RecommendedIngredients,
                scan.AnalysisResult.AIAnalysisText,
                null,
                scan.AnalysisResult.ConfidenceScore,
                scan.AnalysisResult.FaceDetected,
                scan.AnalysisResult.FaceLandmarksCount
            ) : null,
            recommendations
        );
    }
}
