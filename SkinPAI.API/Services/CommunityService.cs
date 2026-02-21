using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface ICommunityService
{
    Task<PaginatedResponse<CommunityPostDto>> GetFeedAsync(Guid? currentUserId, int page = 1, int pageSize = 20);
    Task<CommunityPostDto?> GetPostByIdAsync(Guid postId, Guid? currentUserId);
    Task<CommunityPostDto> CreatePostAsync(Guid userId, CreatePostRequest request, IFileStorageService fileStorage);
    Task<CommunityPostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request);
    Task<bool> DeletePostAsync(Guid postId, Guid userId);
    Task<bool> LikePostAsync(Guid postId, Guid userId);
    Task<bool> UnlikePostAsync(Guid postId, Guid userId);
    Task<List<PostCommentDto>> GetCommentsAsync(Guid postId);
    Task<PostCommentDto> AddCommentAsync(Guid postId, Guid userId, CreateCommentRequest request);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
    
    // Creator Stations
    Task<CreatorStationDto?> GetStationByIdAsync(Guid stationId, Guid? currentUserId);
    Task<CreatorStationDto?> GetStationBySlugAsync(string slug, Guid? currentUserId);
    Task<CreatorStationDto> CreateStationAsync(Guid userId, CreateStationRequest request);
    Task<CreatorStationDto> UpdateStationAsync(Guid stationId, Guid userId, UpdateStationRequest request, IFileStorageService? fileStorage = null);
    Task<bool> FollowStationAsync(Guid stationId, Guid userId);
    Task<bool> UnfollowStationAsync(Guid stationId, Guid userId);
    Task<List<InfluencerProfileDto>> GetTopInfluencersAsync(int limit = 10);
    
    // Brand Campaigns
    Task<List<BrandCampaignDto>> GetActiveCampaignsAsync(Guid? currentUserId);
    Task<BrandCampaignDto?> GetCampaignByIdAsync(Guid campaignId, Guid? currentUserId);
    Task<bool> JoinCampaignAsync(Guid campaignId, Guid userId);
    
    // Stats
    Task<CommunityStatsDto> GetCommunityStatsAsync();
}

public class CommunityService : ICommunityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommunityService> _logger;

    public CommunityService(IUnitOfWork unitOfWork, ILogger<CommunityService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResponse<CommunityPostDto>> GetFeedAsync(Guid? currentUserId, int page = 1, int pageSize = 20)
    {
        var query = _unitOfWork.CommunityPosts.Query()
            .Include(p => p.User)
            .Include(p => p.Station)
            .Where(p => p.Status == "Published" && !p.IsFlagged)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get liked posts for current user
        var likedPostIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            likedPostIds = (await _unitOfWork.PostLikes.Query()
                .Where(l => l.UserId == currentUserId.Value && posts.Select(p => p.PostId).Contains(l.PostId))
                .Select(l => l.PostId)
                .ToListAsync()).ToHashSet();
        }

        var postDtos = posts.Select(p => MapToPostDto(p, likedPostIds.Contains(p.PostId))).ToList();

        return new PaginatedResponse<CommunityPostDto>(
            postDtos,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    public async Task<CommunityPostDto?> GetPostByIdAsync(Guid postId, Guid? currentUserId)
    {
        var post = await _unitOfWork.CommunityPosts.Query()
            .Include(p => p.User)
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null) return null;

        // Increment view count
        post.ViewCount++;
        _unitOfWork.CommunityPosts.Update(post);
        await _unitOfWork.SaveChangesAsync();

        var isLiked = currentUserId.HasValue && await _unitOfWork.PostLikes
            .AnyAsync(l => l.PostId == postId && l.UserId == currentUserId.Value);

        return MapToPostDto(post, isLiked);
    }

    public async Task<CommunityPostDto> CreatePostAsync(Guid userId, CreatePostRequest request, IFileStorageService fileStorage)
    {
        var mediaUrls = new List<string>();
        
        // Save media files
        if (request.MediaBase64 != null)
        {
            foreach (var media in request.MediaBase64)
            {
                var url = await fileStorage.SaveImageAsync(media, "posts", $"post_{userId}_{DateTime.UtcNow.Ticks}");
                mediaUrls.Add(url);
            }
        }

        var post = new CommunityPost
        {
            UserId = userId,
            PostType = request.PostType,
            Title = request.Title,
            Content = request.Content,
            MediaUrls = mediaUrls.Any() ? string.Join(",", mediaUrls) : null,
            ThumbnailUrl = mediaUrls.FirstOrDefault(),
            Tags = request.Tags != null ? string.Join(",", request.Tags) : null,
            Hashtags = request.Hashtags != null ? string.Join(",", request.Hashtags) : null,
            Status = "Published",
            PublishedAt = DateTime.UtcNow
        };

        // Get user's station if they have one
        var station = await _unitOfWork.CreatorStations.FirstOrDefaultAsync(s => s.UserId == userId);
        if (station != null)
        {
            post.StationId = station.StationId;
            station.TotalPosts++;
            _unitOfWork.CreatorStations.Update(station);
        }

        await _unitOfWork.CommunityPosts.AddAsync(post);
        await _unitOfWork.SaveChangesAsync();

        // Load the user for the response
        post.User = await _unitOfWork.Users.GetByIdAsync(userId) ?? post.User;

        return MapToPostDto(post, false);
    }

    public async Task<CommunityPostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request)
    {
        var post = await _unitOfWork.CommunityPosts.Query()
            .Include(p => p.User)
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);

        if (post == null)
            throw new KeyNotFoundException("Post not found");

        if (request.Title != null) post.Title = request.Title;
        if (request.Content != null) post.Content = request.Content;
        if (request.Tags != null) post.Tags = string.Join(",", request.Tags);
        if (request.Hashtags != null) post.Hashtags = string.Join(",", request.Hashtags);
        if (request.Status != null) post.Status = request.Status;

        post.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CommunityPosts.Update(post);
        await _unitOfWork.SaveChangesAsync();

        return MapToPostDto(post, false);
    }

    public async Task<bool> DeletePostAsync(Guid postId, Guid userId)
    {
        var post = await _unitOfWork.CommunityPosts.FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);
        if (post == null) return false;

        _unitOfWork.CommunityPosts.Remove(post);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LikePostAsync(Guid postId, Guid userId)
    {
        var exists = await _unitOfWork.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        if (exists) return false;

        await _unitOfWork.PostLikes.AddAsync(new PostLike
        {
            PostId = postId,
            UserId = userId
        });

        var post = await _unitOfWork.CommunityPosts.GetByIdAsync(postId);
        if (post != null)
        {
            post.LikeCount++;
            _unitOfWork.CommunityPosts.Update(post);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlikePostAsync(Guid postId, Guid userId)
    {
        var like = await _unitOfWork.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        if (like == null) return false;

        _unitOfWork.PostLikes.Remove(like);

        var post = await _unitOfWork.CommunityPosts.GetByIdAsync(postId);
        if (post != null)
        {
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
            _unitOfWork.CommunityPosts.Update(post);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<PostCommentDto>> GetCommentsAsync(Guid postId)
    {
        var comments = await _unitOfWork.PostComments.Query()
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToCommentDto).ToList();
    }

    public async Task<PostCommentDto> AddCommentAsync(Guid postId, Guid userId, CreateCommentRequest request)
    {
        var comment = new PostComment
        {
            PostId = postId,
            UserId = userId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };

        await _unitOfWork.PostComments.AddAsync(comment);

        var post = await _unitOfWork.CommunityPosts.GetByIdAsync(postId);
        if (post != null)
        {
            post.CommentCount++;
            _unitOfWork.CommunityPosts.Update(post);
        }

        await _unitOfWork.SaveChangesAsync();

        comment.User = await _unitOfWork.Users.GetByIdAsync(userId) ?? comment.User;
        return MapToCommentDto(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _unitOfWork.PostComments.FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);
        if (comment == null) return false;

        comment.IsDeleted = true;
        _unitOfWork.PostComments.Update(comment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<CreatorStationDto?> GetStationByIdAsync(Guid stationId, Guid? currentUserId)
    {
        var station = await _unitOfWork.CreatorStations.Query()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StationId == stationId);

        if (station == null) return null;

        var isFollowing = currentUserId.HasValue && await _unitOfWork.StationFollowers
            .AnyAsync(f => f.StationId == stationId && f.FollowerUserId == currentUserId.Value);

        return MapToStationDto(station, isFollowing);
    }

    public async Task<CreatorStationDto?> GetStationBySlugAsync(string slug, Guid? currentUserId)
    {
        var station = await _unitOfWork.CreatorStations.Query()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StationSlug == slug);

        if (station == null) return null;

        var isFollowing = currentUserId.HasValue && await _unitOfWork.StationFollowers
            .AnyAsync(f => f.StationId == station.StationId && f.FollowerUserId == currentUserId.Value);

        return MapToStationDto(station, isFollowing);
    }

    public async Task<CreatorStationDto> CreateStationAsync(Guid userId, CreateStationRequest request)
    {
        // Check if user already has a station
        var existingStation = await _unitOfWork.CreatorStations.FirstOrDefaultAsync(s => s.UserId == userId);
        if (existingStation != null)
            throw new InvalidOperationException("User already has a station");

        // Check if slug is unique
        var slugExists = await _unitOfWork.CreatorStations.AnyAsync(s => s.StationSlug == request.StationSlug);
        if (slugExists)
            throw new InvalidOperationException("Station slug already taken");

        var station = new CreatorStation
        {
            UserId = userId,
            StationName = request.StationName,
            StationSlug = request.StationSlug.ToLower().Replace(" ", "-"),
            Bio = request.Bio,
            Description = request.Description,
            Location = request.Location,
            Specialties = request.Specialties != null ? string.Join(",", request.Specialties) : null,
            Theme = request.Theme ?? "default",
            IsPublished = true
        };

        await _unitOfWork.CreatorStations.AddAsync(station);

        // Update user to be a creator
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.IsCreator = true;
            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToStationDto(station, false);
    }

    public async Task<CreatorStationDto> UpdateStationAsync(Guid stationId, Guid userId, UpdateStationRequest request, IFileStorageService? fileStorage = null)
    {
        var station = await _unitOfWork.CreatorStations.FirstOrDefaultAsync(s => s.StationId == stationId && s.UserId == userId);
        if (station == null)
            throw new KeyNotFoundException("Station not found");

        if (request.StationName != null) station.StationName = request.StationName;
        if (request.Bio != null) station.Bio = request.Bio;
        if (request.Description != null) station.Description = request.Description;
        if (request.Location != null) station.Location = request.Location;
        if (request.InstagramUrl != null) station.InstagramUrl = request.InstagramUrl;
        if (request.YoutubeUrl != null) station.YoutubeUrl = request.YoutubeUrl;
        if (request.TikTokUrl != null) station.TikTokUrl = request.TikTokUrl;
        if (request.TwitterUrl != null) station.TwitterUrl = request.TwitterUrl;
        if (request.WebsiteUrl != null) station.WebsiteUrl = request.WebsiteUrl;
        if (request.Email != null) station.Email = request.Email;
        if (request.Specialties != null) station.Specialties = string.Join(",", request.Specialties);
        if (request.Certifications != null) station.Certifications = string.Join(",", request.Certifications);
        if (request.Experience != null) station.Experience = request.Experience;
        if (request.ContentFrequency != null) station.ContentFrequency = request.ContentFrequency;
        if (request.Theme != null) station.Theme = request.Theme;
        if (request.IsPublished.HasValue) station.IsPublished = request.IsPublished.Value;
        if (request.AllowComments.HasValue) station.AllowComments = request.AllowComments.Value;
        if (request.AllowMessages.HasValue) station.AllowMessages = request.AllowMessages.Value;

        station.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CreatorStations.Update(station);
        await _unitOfWork.SaveChangesAsync();

        return MapToStationDto(station, false);
    }

    public async Task<bool> FollowStationAsync(Guid stationId, Guid userId)
    {
        var exists = await _unitOfWork.StationFollowers.AnyAsync(f => f.StationId == stationId && f.FollowerUserId == userId);
        if (exists) return false;

        await _unitOfWork.StationFollowers.AddAsync(new StationFollower
        {
            StationId = stationId,
            FollowerUserId = userId
        });

        var station = await _unitOfWork.CreatorStations.GetByIdAsync(stationId);
        if (station != null)
        {
            station.FollowersCount++;
            _unitOfWork.CreatorStations.Update(station);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnfollowStationAsync(Guid stationId, Guid userId)
    {
        var follow = await _unitOfWork.StationFollowers.FirstOrDefaultAsync(f => f.StationId == stationId && f.FollowerUserId == userId);
        if (follow == null) return false;

        _unitOfWork.StationFollowers.Remove(follow);

        var station = await _unitOfWork.CreatorStations.GetByIdAsync(stationId);
        if (station != null)
        {
            station.FollowersCount = Math.Max(0, station.FollowersCount - 1);
            _unitOfWork.CreatorStations.Update(station);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<InfluencerProfileDto>> GetTopInfluencersAsync(int limit = 10)
    {
        var stations = await _unitOfWork.CreatorStations.Query()
            .Include(s => s.User)
            .Where(s => s.IsPublished)
            .OrderByDescending(s => s.FollowersCount)
            .Take(limit)
            .ToListAsync();

        var rank = 1;
        return stations.Select(s => new InfluencerProfileDto(
            s.UserId,
            s.User.FullName,
            $"@{s.StationSlug}",
            s.ProfileImageUrl ?? s.User.ProfileImageUrl,
            s.User.IsVerified,
            s.Bio,
            s.FollowersCount,
            0, // Following count not tracked
            s.TotalPosts,
            s.TotalLikes,
            s.Specialties?.Split(','),
            s.CreatedAt,
            rank++,
            null,
            s.StationId
        )).ToList();
    }

    public async Task<List<BrandCampaignDto>> GetActiveCampaignsAsync(Guid? currentUserId)
    {
        var campaigns = await _unitOfWork.BrandCampaigns.Query()
            .Include(c => c.Brand)
            .Where(c => c.IsActive && c.EndDate > DateTime.UtcNow)
            .OrderByDescending(c => c.Featured)
            .ThenByDescending(c => c.ParticipantCount)
            .ToListAsync();

        var participatingCampaignIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            participatingCampaignIds = (await _unitOfWork.CampaignParticipants.Query()
                .Where(p => p.UserId == currentUserId.Value)
                .Select(p => p.CampaignId)
                .ToListAsync()).ToHashSet();
        }

        return campaigns.Select(c => MapToCampaignDto(c, participatingCampaignIds.Contains(c.CampaignId))).ToList();
    }

    public async Task<BrandCampaignDto?> GetCampaignByIdAsync(Guid campaignId, Guid? currentUserId)
    {
        var campaign = await _unitOfWork.BrandCampaigns.Query()
            .Include(c => c.Brand)
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId);

        if (campaign == null) return null;

        var isParticipating = currentUserId.HasValue && await _unitOfWork.CampaignParticipants
            .AnyAsync(p => p.CampaignId == campaignId && p.UserId == currentUserId.Value);

        return MapToCampaignDto(campaign, isParticipating);
    }

    public async Task<bool> JoinCampaignAsync(Guid campaignId, Guid userId)
    {
        var exists = await _unitOfWork.CampaignParticipants.AnyAsync(p => p.CampaignId == campaignId && p.UserId == userId);
        if (exists) return false;

        await _unitOfWork.CampaignParticipants.AddAsync(new CampaignParticipant
        {
            CampaignId = campaignId,
            UserId = userId
        });

        var campaign = await _unitOfWork.BrandCampaigns.GetByIdAsync(campaignId);
        if (campaign != null)
        {
            campaign.ParticipantCount++;
            _unitOfWork.BrandCampaigns.Update(campaign);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<CommunityStatsDto> GetCommunityStatsAsync()
    {
        var totalMembers = await _unitOfWork.Users.CountAsync(u => !u.IsDeleted);
        var today = DateTime.UtcNow.Date;
        var activeToday = await _unitOfWork.Users.CountAsync(u => u.LastLoginAt != null && u.LastLoginAt >= today);
        var postsToday = await _unitOfWork.CommunityPosts.CountAsync(p => p.CreatedAt >= today);

        // Get top hashtags - simplified for now
        var topHashtags = new[] { "#skincare", "#glowup", "#skinroutine", "#beauty", "#selfcare" };

        return new CommunityStatsDto(totalMembers, activeToday, postsToday, topHashtags);
    }

    private CommunityPostDto MapToPostDto(CommunityPost post, bool isLiked)
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
            post.MediaUrls?.Split(',', StringSplitOptions.RemoveEmptyEntries),
            post.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries),
            post.Hashtags?.Split(',', StringSplitOptions.RemoveEmptyEntries),
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

    private PostCommentDto MapToCommentDto(PostComment comment)
    {
        return new PostCommentDto(
            comment.CommentId,
            comment.PostId,
            comment.UserId,
            comment.User?.FullName,
            comment.User?.ProfileImageUrl,
            comment.ParentCommentId,
            comment.Content,
            comment.LikeCount,
            comment.CreatedAt,
            comment.Replies?.Where(r => !r.IsDeleted).Select(MapToCommentDto).ToList()
        );
    }

    private CreatorStationDto MapToStationDto(CreatorStation station, bool isFollowing)
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
            station.Specialties?.Split(',', StringSplitOptions.RemoveEmptyEntries),
            station.Certifications?.Split(',', StringSplitOptions.RemoveEmptyEntries),
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

    private BrandCampaignDto MapToCampaignDto(BrandCampaign campaign, bool isParticipating)
    {
        return new BrandCampaignDto(
            campaign.CampaignId,
            new BrandDto(
                campaign.Brand.BrandId,
                campaign.Brand.BrandName,
                campaign.Brand.LogoUrl,
                campaign.Brand.Description,
                campaign.Brand.Website,
                campaign.Brand.IsVerified,
                campaign.Brand.IsPartner
            ),
            campaign.Title,
            campaign.Description,
            campaign.CampaignImageUrl,
            campaign.CampaignVideoUrl,
            campaign.StartDate,
            campaign.EndDate,
            campaign.ParticipantCount,
            campaign.TotalEngagement,
            campaign.Hashtag,
            campaign.Prize,
            campaign.Requirements != null ? JsonSerializer.Deserialize<string[]>(campaign.Requirements) : null,
            campaign.IsActive,
            campaign.Featured,
            isParticipating
        );
    }
}
