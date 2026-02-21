using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get user notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var notifications = await _notificationService.GetNotificationsAsync(GetUserId(), unreadOnly);
        return Ok(notifications);
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(GetUserId());
        return Ok(new { count });
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkAsRead(Guid notificationId)
    {
        var success = await _notificationService.MarkAsReadAsync(notificationId, GetUserId());
        if (!success) return NotFound();
        return Ok(new { message = "Notification marked as read" });
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var count = await _notificationService.MarkAllAsReadAsync(GetUserId());
        return Ok(new { markedCount = count });
    }

    /// <summary>
    /// Delete notification
    /// </summary>
    [HttpDelete("{notificationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteNotification(Guid notificationId)
    {
        var success = await _notificationService.DeleteNotificationAsync(notificationId, GetUserId());
        if (!success) return NotFound();
        return Ok(new { message = "Notification deleted" });
    }

    // ==================== Achievements ====================

    /// <summary>
    /// Get all achievements
    /// </summary>
    [AllowAnonymous]
    [HttpGet("achievements")]
    [ProducesResponseType(typeof(List<AchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AchievementDto>>> GetAllAchievements()
    {
        var achievements = await _notificationService.GetAllAchievementsAsync();
        return Ok(achievements);
    }

    /// <summary>
    /// Get user's unlocked achievements
    /// </summary>
    [HttpGet("achievements/me")]
    [ProducesResponseType(typeof(List<UserAchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserAchievementDto>>> GetMyAchievements()
    {
        var achievements = await _notificationService.GetUserAchievementsAsync(GetUserId());
        return Ok(achievements);
    }

    /// <summary>
    /// Manually check and unlock achievements (e.g., after app actions)
    /// </summary>
    [HttpPost("achievements/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CheckAchievements()
    {
        await _notificationService.CheckAndUnlockAchievementsAsync(GetUserId());
        return Ok(new { message = "Achievement check completed" });
    }
}
