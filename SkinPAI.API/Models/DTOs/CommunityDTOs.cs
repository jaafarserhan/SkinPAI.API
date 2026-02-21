namespace SkinPAI.API.Models.DTOs;

// ==================== Community DTOs ====================
public record CommunityPostDto(
    Guid PostId,
    Guid UserId,
    string? UserName,
    string? UserProfileImage,
    bool IsVerified,
    Guid? StationId,
    string? StationName,
    string PostType,
    string? Title,
    string? Content,
    string? ThumbnailUrl,
    string[]? MediaUrls,
    string[]? Tags,
    string[]? Hashtags,
    int? ReadTimeMinutes,
    int ViewCount,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    string Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    bool IsLikedByCurrentUser
);

public record CreatePostRequest(
    string Content,
    string PostType = "Post",
    string? Title = null,
    string[]? MediaBase64 = null,
    string[]? Tags = null,
    string[]? Hashtags = null
);

// ==================== Creator Station DTOs ====================
public record CreatorStationDto(
    Guid StationId,
    Guid UserId,
    string StationName,
    string StationSlug,
    string? Bio,
    string? Description,
    string? Location,
    string? BannerImageUrl,
    string? ProfileImageUrl,
    string? InstagramUrl,
    string? YoutubeUrl,
    string? TikTokUrl,
    string? TwitterUrl,
    string? WebsiteUrl,
    string? Email,
    string[]? Specialties,
    string[]? Certifications,
    string? Experience,
    string? ContentFrequency,
    string Theme,
    int FollowersCount,
    int TotalPosts,
    long TotalViews,
    int TotalLikes,
    bool IsPublished,
    DateTime CreatedAt,
    bool IsFollowedByCurrentUser
);

public record InfluencerBadgeDto(
    string Id,
    string Name,
    string Icon,
    string Description,
    DateTime EarnedDate
);

// ==================== Brand Campaign DTOs ====================
public record BrandCampaignDto(
    Guid CampaignId,
    BrandDto Brand,
    string Title,
    string? Description,
    string? CampaignImageUrl,
    string? CampaignVideoUrl,
    DateTime StartDate,
    DateTime EndDate,
    int ParticipantCount,
    int TotalEngagement,
    string? Hashtag,
    string? Prize,
    string[]? Requirements,
    bool IsActive,
    bool Featured,
    bool IsParticipating
);
