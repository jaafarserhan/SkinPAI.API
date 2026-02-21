using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class CommunityPost
{
    [Key]
    public Guid PostId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid? StationId { get; set; }

    [Required, MaxLength(50)]
    public string PostType { get; set; } = "Post"; // Post, Article, Video, Tutorial, Tip

    [MaxLength(300)]
    public string? Title { get; set; }

    public string? Content { get; set; } // HTML or Markdown

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public string? MediaUrls { get; set; } // JSON array of image/video URLs

    public string? Tags { get; set; } // JSON array

    public string? Hashtags { get; set; } // JSON array

    public int? ReadTimeMinutes { get; set; }

    // Engagement
    public int ViewCount { get; set; } = 0;

    public int LikeCount { get; set; } = 0;

    public int CommentCount { get; set; } = 0;

    public int ShareCount { get; set; } = 0;

    // Publishing
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Published"; // Draft, Published, Scheduled, Archived

    public DateTime? PublishedAt { get; set; }

    public DateTime? ScheduledFor { get; set; }

    // Moderation
    public bool IsFlagged { get; set; } = false;

    [MaxLength(500)]
    public string? FlagReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(StationId))]
    public virtual CreatorStation? Station { get; set; }

    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
}
