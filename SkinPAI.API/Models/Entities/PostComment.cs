using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class PostComment
{
    [Key]
    public Guid CommentId { get; set; } = Guid.NewGuid();

    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public int LikeCount { get; set; } = 0;

    public bool IsFlagged { get; set; } = false;

    [MaxLength(500)]
    public string? FlagReason { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PostId))]
    public virtual CommunityPost Post { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(ParentCommentId))]
    public virtual PostComment? ParentComment { get; set; }

    public virtual ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
}
