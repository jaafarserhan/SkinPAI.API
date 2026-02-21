using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IFileStorageService fileStorageService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetUserId();
        _logger.LogDebug("👤 USER GET: Fetching current user profile | UserId: {UserId}", userId);
        
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("⚠️ USER NOT FOUND: User profile not found | UserId: {UserId}", userId);
            return NotFound();
        }
        
        _logger.LogInformation("👤 USER GET: Profile retrieved | UserId: {UserId} | Email: {Email} | MembershipType: {MembershipType}", 
            userId, user.Email, user.MembershipType);
        return Ok(user);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        _logger.LogInformation("✏️ USER UPDATE: Profile update requested | UserId: {UserId}", userId);
        
        try
        {
            var user = await _userService.UpdateUserAsync(userId, request);
            _logger.LogInformation("✅ USER UPDATED: Profile updated | UserId: {UserId} | Fields: {Fields}", 
                userId, GetUpdatedFields(request));
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("⚠️ USER UPDATE FAILED: User not found | UserId: {UserId}", userId);
            return NotFound();
        }
    }

    /// <summary>
    /// Upload profile image
    /// </summary>
    [HttpPost("me/profile-image")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> UploadProfileImage([FromBody] UploadImageRequest request)
    {
        var userId = GetUserId();
        _logger.LogInformation("📷 PROFILE IMAGE: Upload requested | UserId: {UserId} | ImageSize: {Size} bytes", 
            userId, request.Base64Image?.Length ?? 0);
        
        var imageUrl = await _fileStorageService.SaveImageAsync(request.Base64Image, "profiles", $"user_{userId}");
        
        await _userService.UpdateUserAsync(userId, new UpdateUserRequest(ProfileImageUrl: imageUrl));
        
        _logger.LogInformation("✅ PROFILE IMAGE: Uploaded successfully | UserId: {UserId} | ImageUrl: {ImageUrl}", 
            userId, imageUrl);
        return Ok(new { imageUrl });
    }

    /// <summary>
    /// Get user's skin profile
    /// </summary>
    [HttpGet("me/skin-profile")]
    [ProducesResponseType(typeof(SkinProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SkinProfileDto>> GetSkinProfile()
    {
        var userId = GetUserId();
        _logger.LogDebug("🧬 SKIN PROFILE GET: Fetching skin profile | UserId: {UserId}", userId);
        
        var profile = await _userService.GetSkinProfileAsync(userId);
        if (profile == null)
        {
            _logger.LogWarning("⚠️ SKIN PROFILE NOT FOUND: Profile not found | UserId: {UserId}", userId);
            return NotFound();
        }
        
        _logger.LogInformation("🧬 SKIN PROFILE GET: Retrieved | UserId: {UserId} | SkinType: {SkinType} | QuestionnaireDone: {Done}", 
            userId, profile.SkinType, profile.QuestionnaireCompleted);
        return Ok(profile);
    }

    /// <summary>
    /// Update or create skin profile
    /// </summary>
    [HttpPut("me/skin-profile")]
    [ProducesResponseType(typeof(SkinProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SkinProfileDto>> UpdateSkinProfile([FromBody] UpdateSkinProfileRequest request)
    {
        var userId = GetUserId();
        _logger.LogInformation("✏️ SKIN PROFILE UPDATE: Update requested | UserId: {UserId}", userId);
        
        var profile = await _userService.UpdateSkinProfileAsync(userId, request);
        _logger.LogInformation("✅ SKIN PROFILE UPDATED: Profile updated | UserId: {UserId} | SkinType: {SkinType}", 
            userId, profile.SkinType);
        return Ok(profile);
    }

    /// <summary>
    /// Get member dashboard data
    /// </summary>
    [HttpGet("me/dashboard")]
    [ProducesResponseType(typeof(MemberDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MemberDashboardDto>> GetMemberDashboard()
    {
        var userId = GetUserId();
        _logger.LogDebug("📊 DASHBOARD GET: Fetching member dashboard | UserId: {UserId}", userId);
        
        var dashboard = await _userService.GetMemberDashboardAsync(userId);
        _logger.LogInformation("📊 DASHBOARD GET: Retrieved | UserId: {UserId} | UnreadNotifications: {UnreadNotifications}", 
            userId, dashboard.UnreadNotifications);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get pro/creator dashboard data
    /// </summary>
    [HttpGet("me/pro-dashboard")]
    [ProducesResponseType(typeof(ProDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProDashboardDto>> GetProDashboard()
    {
        var userId = GetUserId();
        _logger.LogDebug("🌟 PRO DASHBOARD GET: Fetching pro dashboard | UserId: {UserId}", userId);
        
        var dashboard = await _userService.GetProDashboardAsync(userId);
        _logger.LogInformation("🌟 PRO DASHBOARD GET: Retrieved | UserId: {UserId} | TotalFollowers: {Followers} | TotalPosts: {Posts}", 
            userId, dashboard.TotalFollowers, dashboard.TotalPosts);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get user by ID (public profile)
    /// </summary>
    [HttpGet("{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetUser(Guid userId)
    {
        _logger.LogDebug("👤 USER GET PUBLIC: Fetching public profile | TargetUserId: {UserId}", userId);
        
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("⚠️ USER NOT FOUND: Public profile not found | TargetUserId: {UserId}", userId);
            return NotFound();
        }
        return Ok(user);
    }

    /// <summary>
    /// Delete account (soft delete)
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        _logger.LogWarning("🗑️ ACCOUNT DELETE: Delete requested | UserId: {UserId}", userId);
        
        await _userService.DeleteUserAsync(userId);
        _logger.LogInformation("✅ ACCOUNT DELETED: Account soft deleted | UserId: {UserId}", userId);
        return Ok(new { message = "Account deleted" });
    }

    private static string GetUpdatedFields(UpdateUserRequest request)
    {
        var fields = new List<string>();
        if (request.FirstName != null) fields.Add("FirstName");
        if (request.LastName != null) fields.Add("LastName");
        if (request.PhoneNumber != null) fields.Add("PhoneNumber");
        if (request.ProfileImageUrl != null) fields.Add("ProfileImage");
        return string.Join(", ", fields);
    }
}

public record UploadImageRequest(string Base64Image);
