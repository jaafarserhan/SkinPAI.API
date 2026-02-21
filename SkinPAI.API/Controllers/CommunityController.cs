using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CommunityController> _logger;

    public CommunityController(ICommunityService communityService, IFileStorageService fileStorageService, ILogger<CommunityController> logger)
    {
        _communityService = communityService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    private Guid RequireUserId() => GetUserId() ?? throw new UnauthorizedAccessException();

    private string GetMembershipType()
    {
        return User.FindFirstValue("MembershipType") ?? "Guest";
    }

    private bool IsProUser()
    {
        return GetMembershipType() == "Pro";
    }

    // ==================== Posts ====================

    /// <summary>
    /// Get community feed
    /// </summary>
    [HttpGet("feed")]
    [ProducesResponseType(typeof(PaginatedResponse<CommunityPostDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<CommunityPostDto>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        _logger.LogDebug("📰 FEED GET: Fetching community feed | UserId: {UserId} | Page: {Page} | PageSize: {PageSize}", 
            userId ?? Guid.Empty, page, pageSize);
        
        var feed = await _communityService.GetFeedAsync(userId, page, pageSize);
        _logger.LogInformation("📰 FEED GET: Retrieved {Count} posts | Page: {Page}", feed.Items.Count, page);
        return Ok(feed);
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("posts/{postId}")]
    [ProducesResponseType(typeof(CommunityPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommunityPostDto>> GetPost(Guid postId)
    {
        _logger.LogDebug("📄 POST GET: Fetching post | PostId: {PostId}", postId);
        
        var post = await _communityService.GetPostByIdAsync(postId, GetUserId());
        if (post == null)
        {
            _logger.LogWarning("⚠️ POST NOT FOUND: Post does not exist | PostId: {PostId}", postId);
            return NotFound();
        }
        return Ok(post);
    }

    /// <summary>
    /// Create a new post (Pro users only)
    /// </summary>
    [Authorize]
    [HttpPost("posts")]
    [ProducesResponseType(typeof(CommunityPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CommunityPostDto>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = RequireUserId();
        _logger.LogInformation("✍️ POST CREATE: New post requested | UserId: {UserId} | MembershipType: {MembershipType}", 
            userId, GetMembershipType());
        
        if (!IsProUser())
        {
            _logger.LogWarning("⚠️ POST CREATE BLOCKED: Non-Pro user attempted to create post | UserId: {UserId}", userId);
            return StatusCode(403, new { message = "Only Pro members can create posts. Upgrade to Pro to share content with the community." });
        }
        
        var post = await _communityService.CreatePostAsync(userId, request, _fileStorageService);
        _logger.LogInformation("✅ POST CREATED: Post created successfully | UserId: {UserId} | PostId: {PostId}", 
            userId, post.PostId);
        return Ok(post);
    }

    /// <summary>
    /// Update a post
    /// </summary>
    [Authorize]
    [HttpPut("posts/{postId}")]
    [ProducesResponseType(typeof(CommunityPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommunityPostDto>> UpdatePost(Guid postId, [FromBody] UpdatePostRequest request)
    {
        var userId = RequireUserId();
        _logger.LogInformation("✏️ POST UPDATE: Update requested | UserId: {UserId} | PostId: {PostId}", userId, postId);
        
        try
        {
            var post = await _communityService.UpdatePostAsync(postId, userId, request);
            _logger.LogInformation("✅ POST UPDATED: Post updated | UserId: {UserId} | PostId: {PostId}", userId, postId);
            return Ok(post);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("⚠️ POST UPDATE FAILED: Post not found | UserId: {UserId} | PostId: {PostId}", userId, postId);
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    [Authorize]
    [HttpDelete("posts/{postId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePost(Guid postId)
    {
        var userId = RequireUserId();
        _logger.LogInformation("🗑️ POST DELETE: Delete requested | UserId: {UserId} | PostId: {PostId}", userId, postId);
        
        var deleted = await _communityService.DeletePostAsync(postId, userId);
        if (!deleted)
        {
            _logger.LogWarning("⚠️ POST DELETE FAILED: Post not found | UserId: {UserId} | PostId: {PostId}", userId, postId);
            return NotFound();
        }
        
        _logger.LogInformation("✅ POST DELETED: Post removed | UserId: {UserId} | PostId: {PostId}", userId, postId);
        return Ok(new { message = "Post deleted" });
    }

    /// <summary>
    /// Like a post
    /// </summary>
    [Authorize]
    [HttpPost("posts/{postId}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> LikePost(Guid postId)
    {
        var userId = RequireUserId();
        var liked = await _communityService.LikePostAsync(postId, userId);
        _logger.LogDebug("❤️ POST LIKE: Post {Action} | UserId: {UserId} | PostId: {PostId}", 
            liked ? "liked" : "already liked", userId, postId);
        return Ok(new { success = liked, message = liked ? "Post liked" : "Already liked" });
    }

    /// <summary>
    /// Unlike a post
    /// </summary>
    [Authorize]
    [HttpDelete("posts/{postId}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UnlikePost(Guid postId)
    {
        var userId = RequireUserId();
        var unliked = await _communityService.UnlikePostAsync(postId, userId);
        _logger.LogDebug("💔 POST UNLIKE: Post {Action} | UserId: {UserId} | PostId: {PostId}", 
            unliked ? "unliked" : "was not liked", userId, postId);
        return Ok(new { success = unliked, message = unliked ? "Post unliked" : "Not liked" });
    }

    // ==================== Comments ====================

    /// <summary>
    /// Get comments for a post
    /// </summary>
    [HttpGet("posts/{postId}/comments")]
    [ProducesResponseType(typeof(List<PostCommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PostCommentDto>>> GetComments(Guid postId)
    {
        var comments = await _communityService.GetCommentsAsync(postId);
        return Ok(comments);
    }

    /// <summary>
    /// Add a comment to a post
    /// </summary>
    [Authorize]
    [HttpPost("posts/{postId}/comments")]
    [ProducesResponseType(typeof(PostCommentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostCommentDto>> AddComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        var comment = await _communityService.AddCommentAsync(postId, RequireUserId(), request);
        return Ok(comment);
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [Authorize]
    [HttpDelete("comments/{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteComment(Guid commentId)
    {
        var deleted = await _communityService.DeleteCommentAsync(commentId, RequireUserId());
        if (!deleted) return NotFound();
        return Ok(new { message = "Comment deleted" });
    }

    // ==================== Stations ====================

    /// <summary>
    /// Get creator station by ID
    /// </summary>
    [HttpGet("stations/{stationId}")]
    [ProducesResponseType(typeof(CreatorStationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatorStationDto>> GetStation(Guid stationId)
    {
        _logger.LogDebug("🎨 STATION GET: Fetching station | StationId: {StationId}", stationId);
        
        var station = await _communityService.GetStationByIdAsync(stationId, GetUserId());
        if (station == null)
        {
            _logger.LogWarning("⚠️ STATION NOT FOUND: Station does not exist | StationId: {StationId}", stationId);
            return NotFound();
        }
        return Ok(station);
    }

    /// <summary>
    /// Get creator station by slug
    /// </summary>
    [HttpGet("stations/slug/{slug}")]
    [ProducesResponseType(typeof(CreatorStationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatorStationDto>> GetStationBySlug(string slug)
    {
        _logger.LogDebug("🎨 STATION GET BY SLUG: Fetching station | Slug: {Slug}", slug);
        
        var station = await _communityService.GetStationBySlugAsync(slug, GetUserId());
        if (station == null)
        {
            _logger.LogWarning("⚠️ STATION NOT FOUND: Station does not exist | Slug: {Slug}", slug);
            return NotFound();
        }
        return Ok(station);
    }

    /// <summary>
    /// Create a creator station
    /// </summary>
    [Authorize]
    [HttpPost("stations")]
    [ProducesResponseType(typeof(CreatorStationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatorStationDto>> CreateStation([FromBody] CreateStationRequest request)
    {
        var userId = RequireUserId();
        _logger.LogInformation("🎨 STATION CREATE: New station requested | UserId: {UserId} | StationName: {StationName}", 
            userId, request.StationName);
        
        try
        {
            var station = await _communityService.CreateStationAsync(userId, request);
            _logger.LogInformation("✅ STATION CREATED: Station created successfully | UserId: {UserId} | StationId: {StationId} | Name: {Name}", 
                userId, station.StationId, station.StationName);
            return Ok(station);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ STATION CREATE FAILED: Creation failed | UserId: {UserId} | Reason: {Reason}", 
                userId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update creator station
    /// </summary>
    [Authorize]
    [HttpPut("stations/{stationId}")]
    [ProducesResponseType(typeof(CreatorStationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatorStationDto>> UpdateStation(Guid stationId, [FromBody] UpdateStationRequest request)
    {
        var userId = RequireUserId();
        _logger.LogInformation("✏️ STATION UPDATE: Update requested | UserId: {UserId} | StationId: {StationId}", userId, stationId);
        
        try
        {
            var station = await _communityService.UpdateStationAsync(stationId, userId, request, _fileStorageService);
            _logger.LogInformation("✅ STATION UPDATED: Station updated | UserId: {UserId} | StationId: {StationId}", userId, stationId);
            return Ok(station);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("⚠️ STATION UPDATE FAILED: Station not found | UserId: {UserId} | StationId: {StationId}", userId, stationId);
            return NotFound();
        }
    }

    /// <summary>
    /// Follow a creator station
    /// </summary>
    [Authorize]
    [HttpPost("stations/{stationId}/follow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> FollowStation(Guid stationId)
    {
        var userId = RequireUserId();
        var followed = await _communityService.FollowStationAsync(stationId, userId);
        _logger.LogDebug("➕ STATION FOLLOW: Station {Action} | UserId: {UserId} | StationId: {StationId}", 
            followed ? "followed" : "already following", userId, stationId);
        return Ok(new { success = followed, message = followed ? "Station followed" : "Already following" });
    }

    /// <summary>
    /// Unfollow a creator station
    /// </summary>
    [Authorize]
    [HttpDelete("stations/{stationId}/follow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UnfollowStation(Guid stationId)
    {
        var userId = RequireUserId();
        var unfollowed = await _communityService.UnfollowStationAsync(stationId, userId);
        _logger.LogDebug("➖ STATION UNFOLLOW: Station {Action} | UserId: {UserId} | StationId: {StationId}", 
            unfollowed ? "unfollowed" : "was not following", userId, stationId);
        return Ok(new { success = unfollowed, message = unfollowed ? "Station unfollowed" : "Not following" });
    }

    // ==================== Influencers ====================

    /// <summary>
    /// Get top influencers
    /// </summary>
    [HttpGet("influencers/top")]
    [ProducesResponseType(typeof(List<InfluencerProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InfluencerProfileDto>>> GetTopInfluencers([FromQuery] int limit = 10)
    {
        var influencers = await _communityService.GetTopInfluencersAsync(limit);
        return Ok(influencers);
    }

    // ==================== Campaigns ====================

    /// <summary>
    /// Get active brand campaigns
    /// </summary>
    [HttpGet("campaigns")]
    [ProducesResponseType(typeof(List<BrandCampaignDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BrandCampaignDto>>> GetCampaigns()
    {
        var campaigns = await _communityService.GetActiveCampaignsAsync(GetUserId());
        return Ok(campaigns);
    }

    /// <summary>
    /// Get campaign by ID
    /// </summary>
    [HttpGet("campaigns/{campaignId}")]
    [ProducesResponseType(typeof(BrandCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandCampaignDto>> GetCampaign(Guid campaignId)
    {
        var campaign = await _communityService.GetCampaignByIdAsync(campaignId, GetUserId());
        if (campaign == null) return NotFound();
        return Ok(campaign);
    }

    /// <summary>
    /// Join a campaign
    /// </summary>
    [Authorize]
    [HttpPost("campaigns/{campaignId}/join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> JoinCampaign(Guid campaignId)
    {
        var joined = await _communityService.JoinCampaignAsync(campaignId, RequireUserId());
        return Ok(new { success = joined, message = joined ? "Joined campaign" : "Already participating" });
    }

    // ==================== Stats ====================

    /// <summary>
    /// Get community statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CommunityStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommunityStatsDto>> GetStats()
    {
        var stats = await _communityService.GetCommunityStatsAsync();
        return Ok(stats);
    }
}
