using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<UserDto> UpdateProfileImageAsync(Guid userId, string imageBase64, IFileStorageService fileStorage);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<SkinProfileDto?> GetSkinProfileAsync(Guid userId);
    Task<SkinProfileDto> UpdateSkinProfileAsync(Guid userId, UpdateSkinProfileRequest request);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<MemberDashboardDto> GetMemberDashboardAsync(Guid userId);
    Task<ProDashboardDto> GetProDashboardAsync(Guid userId);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.SkinProfile)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;
        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;
        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth;
        if (request.Gender != null)
            user.Gender = request.Gender;
        if (request.Bio != null)
            user.Bio = request.Bio;

        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserDto(user);
    }

    public async Task<UserDto> UpdateProfileImageAsync(Guid userId, string imageBase64, IFileStorageService fileStorage)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Save the image
        var imageUrl = await fileStorage.SaveImageAsync(imageBase64, "profile-images", $"{userId}_{DateTime.UtcNow.Ticks}");
        user.ProfileImageUrl = imageUrl;
        user.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserDto(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SkinProfileDto?> GetSkinProfileAsync(Guid userId)
    {
        var profile = await _unitOfWork.SkinProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return null;

        return new SkinProfileDto(
            profile.SkinType,
            profile.SkinConcerns,
            profile.CurrentRoutine,
            profile.SunExposure,
            profile.Lifestyle,
            profile.QuestionnaireCompleted
        );
    }

    public async Task<SkinProfileDto> UpdateSkinProfileAsync(Guid userId, UpdateSkinProfileRequest request)
    {
        var profile = await _unitOfWork.SkinProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new SkinProfile { UserId = userId };
            await _unitOfWork.SkinProfiles.AddAsync(profile);
        }

        if (request.SkinType != null)
            profile.SkinType = request.SkinType;
        if (request.SkinConcerns != null)
            profile.SkinConcerns = request.SkinConcerns;
        if (request.CurrentRoutine != null)
            profile.CurrentRoutine = request.CurrentRoutine;
        if (request.SunExposure != null)
            profile.SunExposure = request.SunExposure;
        if (request.Lifestyle != null)
            profile.Lifestyle = request.Lifestyle;

        profile.QuestionnaireCompleted = true;
        profile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.SkinProfiles.Update(profile);

        // Also update the User entity's QuestionnaireCompleted flag
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null && !user.QuestionnaireCompleted)
        {
            user.QuestionnaireCompleted = true;
            user.QuestionnaireCompletedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.SaveChangesAsync();

        return new SkinProfileDto(
            profile.SkinType,
            profile.SkinConcerns,
            profile.CurrentRoutine,
            profile.SunExposure,
            profile.Lifestyle,
            profile.QuestionnaireCompleted
        );
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.Email = $"deleted_{user.UserId}@deleted.skinpai";
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<MemberDashboardDto> GetMemberDashboardAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.SkinProfile)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyUsage = await _unitOfWork.DailyScanUsages
            .FirstOrDefaultAsync(d => d.UserId == userId && d.ScanDate == today);

        var maxScans = user.MembershipType switch
        {
            "Pro" => 9999,
            "Member" => 5,
            _ => 1
        };

        var recentScans = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.ScanDate)
            .Take(5)
            .ToListAsync();

        var routines = await _unitOfWork.UserRoutines.Query()
            .Include(r => r.RoutineSteps)
            .Where(r => r.UserId == userId && r.IsActive)
            .ToListAsync();

        var achievements = await _unitOfWork.UserAchievements.Query()
            .Include(a => a.Achievement)
            .Where(a => a.UserId == userId && a.IsEarned)
            .OrderByDescending(a => a.EarnedDate)
            .Take(5)
            .ToListAsync();

        var unreadNotifications = await _unitOfWork.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return new MemberDashboardDto(
            MapToUserDto(user),
            new DailyScanUsageDto(today, dailyUsage?.ScanCount ?? 0, maxScans, maxScans - (dailyUsage?.ScanCount ?? 0)),
            recentScans.FirstOrDefault() != null ? MapToSkinScanDto(recentScans.First()) : null,
            recentScans.Select(MapToSkinScanDto).ToList(),
            routines.Select(MapToRoutineDto).ToList(),
            achievements.Select(MapToUserAchievementDto).ToList(),
            unreadNotifications
        );
    }

    public async Task<ProDashboardDto> GetProDashboardAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.SkinProfile)
            .Include(u => u.CreatorStation)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyUsage = await _unitOfWork.DailyScanUsages
            .FirstOrDefaultAsync(d => d.UserId == userId && d.ScanDate == today);

        var latestScan = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.ScanDate)
            .FirstOrDefaultAsync();

        var recentPosts = user.CreatorStation != null
            ? await _unitOfWork.CommunityPosts.Query()
                .Where(p => p.StationId == user.CreatorStation.StationId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync()
            : new List<CommunityPost>();

        var unreadMessages = await _unitOfWork.ChatMessages
            .CountAsync(m => m.ReceiverUserId == userId && !m.IsRead);

        var unreadNotifications = await _unitOfWork.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return new ProDashboardDto(
            MapToUserDto(user),
            user.CreatorStation != null ? MapToCreatorStationDto(user.CreatorStation, false) : null,
            new DailyScanUsageDto(today, dailyUsage?.ScanCount ?? 0, 9999, 9999 - (dailyUsage?.ScanCount ?? 0)),
            latestScan != null ? MapToSkinScanDto(latestScan) : null,
            user.CreatorStation?.FollowersCount ?? 0,
            user.CreatorStation?.TotalPosts ?? 0,
            user.CreatorStation?.TotalViews ?? 0,
            user.CreatorStation?.TotalLikes ?? 0,
            recentPosts.Select(p => MapToCommunityPostDto(p, false)).ToList(),
            unreadMessages,
            unreadNotifications
        );
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.ProfileImageUrl,
            user.MembershipType,
            user.MembershipStatus,
            user.MembershipStartDate,
            user.MembershipEndDate,
            user.WalletBalance,
            user.TotalScansUsed,
            user.IsVerified,
            user.IsCreator,
            user.SkinProfile?.QuestionnaireCompleted ?? false,
            user.SkinProfile != null ? new SkinProfileDto(
                user.SkinProfile.SkinType,
                user.SkinProfile.SkinConcerns,
                user.SkinProfile.CurrentRoutine,
                user.SkinProfile.SunExposure,
                user.SkinProfile.Lifestyle,
                user.SkinProfile.QuestionnaireCompleted
            ) : null,
            user.CreatedAt
        );
    }

    private SkinScanDto MapToSkinScanDto(SkinScan scan)
    {
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
            scan.AnalysisResult != null ? MapToSkinAnalysisResultDto(scan.AnalysisResult) : null,
            null
        );
    }

    private SkinAnalysisResultDto MapToSkinAnalysisResultDto(SkinAnalysisResult result)
    {
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

    private RoutineDto MapToRoutineDto(UserRoutine routine)
    {
        return new RoutineDto(
            routine.RoutineId,
            routine.RoutineName,
            routine.RoutineType,
            routine.IsActive,
            routine.RoutineSteps.OrderBy(s => s.StepOrder).Select(s => new RoutineStepDto(
                s.StepId,
                s.StepOrder,
                s.StepName,
                s.Instructions,
                s.DurationMinutes,
                s.IsCompleted,
                null
            )).ToList(),
            routine.CreatedAt
        );
    }

    private UserAchievementDto MapToUserAchievementDto(UserAchievement ua)
    {
        return new UserAchievementDto(
            ua.Achievement.AchievementId,
            ua.Achievement.AchievementCode ?? "",
            ua.Achievement.AchievementName ?? ua.Achievement.Title ?? "",
            ua.Achievement.Description,
            ua.Achievement.IconUrl ?? ua.Achievement.Icon,
            ua.Achievement.Category,
            ua.Achievement.Rarity,
            ua.Achievement.PointsValue,
            ua.UnlockedAt,
            ua.Progress
        );
    }

    private CreatorStationDto MapToCreatorStationDto(CreatorStation station, bool isFollowing)
    {
        return new CreatorStationDto(
            station.StationId,
            station.UserId,
            station.StationName,
            station.StationSlug,
            station.Bio,
            station.Description,
            station.Location,
            station.BannerImageUrl,
            station.ProfileImageUrl,
            station.InstagramUrl,
            station.YoutubeUrl,
            station.TikTokUrl,
            station.TwitterUrl,
            station.WebsiteUrl,
            station.Email,
            station.Specialties?.Split(','),
            station.Certifications?.Split(','),
            station.Experience,
            station.ContentFrequency,
            station.Theme,
            station.FollowersCount,
            station.TotalPosts,
            station.TotalViews,
            station.TotalLikes,
            station.IsPublished,
            station.CreatedAt,
            isFollowing
        );
    }

    private CommunityPostDto MapToCommunityPostDto(CommunityPost post, bool isLiked)
    {
        return new CommunityPostDto(
            post.PostId,
            post.UserId,
            post.User?.FullName,
            post.User?.ProfileImageUrl,
            post.User?.IsVerified ?? false,
            post.StationId,
            post.Station?.StationName,
            post.PostType,
            post.Title,
            post.Content,
            post.ThumbnailUrl,
            post.MediaUrls?.Split(','),
            post.Tags?.Split(','),
            post.Hashtags?.Split(','),
            post.ReadTimeMinutes,
            post.ViewCount,
            post.LikeCount,
            post.CommentCount,
            post.ShareCount,
            post.Status,
            post.PublishedAt,
            post.CreatedAt,
            isLiked
        );
    }
}
