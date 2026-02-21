using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<NotificationDto> CreateNotificationAsync(Guid userId, string type, string title, string message, string? actionUrl = null, Guid? relatedEntityId = null);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
    
    // Achievements
    Task<List<AchievementDto>> GetAllAchievementsAsync();
    Task<List<UserAchievementDto>> GetUserAchievementsAsync(Guid userId);
    Task<UserAchievementDto> UnlockAchievementAsync(Guid userId, string achievementCode);
    Task CheckAndUnlockAchievementsAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(100) // Limit to prevent memory issues
            .ToListAsync();

        return notifications.Select(n => new NotificationDto(
            n.NotificationId,
            n.NotificationType,
            n.Title,
            n.Body,
            n.IsRead,
            n.ActionUrl,
            n.IconUrl,
            n.RelatedEntityId,
            n.CreatedAt,
            n.ReadAt
        )).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Notifications.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<NotificationDto> CreateNotificationAsync(Guid userId, string type, string title, string message, string? actionUrl = null, Guid? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            NotificationType = type,
            Title = title,
            Body = message,
            ActionUrl = actionUrl,
            RelatedEntityId = relatedEntityId
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return new NotificationDto(
            notification.NotificationId,
            notification.NotificationType,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.ActionUrl,
            notification.IconUrl,
            notification.RelatedEntityId,
            notification.CreatedAt,
            notification.ReadAt
        );
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification == null) return false;

        _unitOfWork.Notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<AchievementDto>> GetAllAchievementsAsync()
    {
        var achievements = await _unitOfWork.Achievements.Query()
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Category)
            .ToListAsync();

        return achievements.Select(a => new AchievementDto(
            a.AchievementId,
            a.AchievementCode,
            a.AchievementName,
            a.Description,
            a.IconUrl,
            a.Category,
            a.Rarity,
            a.PointsValue,
            a.UnlockCriteria
        )).ToList();
    }

    public async Task<List<UserAchievementDto>> GetUserAchievementsAsync(Guid userId)
    {
        var userAchievements = await _unitOfWork.UserAchievements.Query()
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.UnlockedAt)
            .ToListAsync();

        return userAchievements.Select(ua => new UserAchievementDto(
            ua.Achievement.AchievementId,
            ua.Achievement.AchievementCode,
            ua.Achievement.AchievementName,
            ua.Achievement.Description,
            ua.Achievement.IconUrl,
            ua.Achievement.Category,
            ua.Achievement.Rarity,
            ua.Achievement.PointsValue,
            ua.UnlockedAt,
            ua.Progress
        )).ToList();
    }

    public async Task<UserAchievementDto> UnlockAchievementAsync(Guid userId, string achievementCode)
    {
        var achievement = await _unitOfWork.Achievements.FirstOrDefaultAsync(a => a.AchievementCode == achievementCode);
        if (achievement == null)
            throw new KeyNotFoundException($"Achievement '{achievementCode}' not found");

        // Check if already unlocked
        var existing = await _unitOfWork.UserAchievements.FirstOrDefaultAsync(
            ua => ua.UserId == userId && ua.AchievementId == achievement.AchievementId);
        
        if (existing != null)
        {
            return new UserAchievementDto(
                achievement.AchievementId,
                achievement.AchievementCode,
                achievement.AchievementName,
                achievement.Description,
                achievement.IconUrl,
                achievement.Category,
                achievement.Rarity,
                achievement.PointsValue,
                existing.UnlockedAt,
                existing.Progress
            );
        }

        var userAchievement = new UserAchievement
        {
            UserId = userId,
            AchievementId = achievement.AchievementId,
            Progress = 100
        };

        await _unitOfWork.UserAchievements.AddAsync(userAchievement);

        // Increment achievement unlocked count
        achievement.TotalUnlocked++;
        _unitOfWork.Achievements.Update(achievement);

        // Create notification
        await CreateNotificationAsync(
            userId,
            "achievement",
            "Achievement Unlocked!",
            $"You earned the '{achievement.AchievementName}' achievement!",
            null,
            achievement.AchievementId
        );

        await _unitOfWork.SaveChangesAsync();

        return new UserAchievementDto(
            achievement.AchievementId,
            achievement.AchievementCode,
            achievement.AchievementName,
            achievement.Description,
            achievement.IconUrl,
            achievement.Category,
            achievement.Rarity,
            achievement.PointsValue,
            userAchievement.UnlockedAt,
            userAchievement.Progress
        );
    }

    public async Task CheckAndUnlockAchievementsAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return;

        var unlockedAchievementCodes = (await _unitOfWork.UserAchievements.Query()
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.Achievement.AchievementCode)
            .ToListAsync()).ToHashSet();

        // Check First Scan achievement
        if (!unlockedAchievementCodes.Contains("FIRST_SCAN"))
        {
            var hasScans = await _unitOfWork.SkinScans.AnyAsync(s => s.UserId == userId);
            if (hasScans)
            {
                await UnlockAchievementAsync(userId, "FIRST_SCAN");
            }
        }

        // Check Scan Streak achievements
        if (!unlockedAchievementCodes.Contains("SCAN_STREAK_7"))
        {
            // Check for 7-day streak
            var recentScans = await _unitOfWork.SkinScans.Query()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.ScanDate)
                .Take(7)
                .Select(s => s.ScanDate.Date)
                .Distinct()
                .ToListAsync();

            if (recentScans.Count >= 7)
            {
                await UnlockAchievementAsync(userId, "SCAN_STREAK_7");
            }
        }

        // Check Community achievements
        if (!unlockedAchievementCodes.Contains("FIRST_POST"))
        {
            var hasPosts = await _unitOfWork.CommunityPosts.AnyAsync(p => p.UserId == userId);
            if (hasPosts)
            {
                await UnlockAchievementAsync(userId, "FIRST_POST");
            }
        }

        // Check Routine achievements
        if (!unlockedAchievementCodes.Contains("ROUTINE_MASTER"))
        {
            var completionsCount = await _unitOfWork.RoutineCompletions.CountAsync(c => c.UserId == userId);
            if (completionsCount >= 30)
            {
                await UnlockAchievementAsync(userId, "ROUTINE_MASTER");
            }
        }
    }
}
