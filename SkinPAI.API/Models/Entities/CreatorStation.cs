using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class CreatorStation
{
    [Key]
    public Guid StationId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(200)]
    public string StationName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string StationSlug { get; set; } = string.Empty; // URL-friendly name

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? BannerImageUrl { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    // Social Links
    [MaxLength(500)]
    public string? InstagramUrl { get; set; }

    [MaxLength(500)]
    public string? YoutubeUrl { get; set; }

    [MaxLength(500)]
    public string? TikTokUrl { get; set; }

    [MaxLength(500)]
    public string? TwitterUrl { get; set; }

    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    // Specialties and certifications
    public string? Specialties { get; set; } // JSON array

    public string? Certifications { get; set; } // JSON array

    [MaxLength(100)]
    public string? Experience { get; set; }

    [MaxLength(100)]
    public string? ContentFrequency { get; set; }

    [MaxLength(50)]
    public string Theme { get; set; } = "skincare"; // skincare, makeup, wellness, lifestyle

    // Stats
    public int FollowersCount { get; set; } = 0;

    public int TotalPosts { get; set; } = 0;

    public long TotalViews { get; set; } = 0;

    public int TotalLikes { get; set; } = 0;

    // Settings
    public bool IsPublished { get; set; } = false;

    public bool AllowComments { get; set; } = true;

    public bool AllowMessages { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<StationFollower> Followers { get; set; } = new List<StationFollower>();
    public virtual ICollection<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
}
