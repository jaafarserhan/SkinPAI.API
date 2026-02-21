using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class UserAchievement
{
    [Key]
    public long UserAchievementId { get; set; }

    public Guid UserId { get; set; }

    public Guid AchievementId { get; set; }

    public int CurrentProgress { get; set; } = 0;

    // Alias for service compatibility
    public int Progress { get => CurrentProgress; set => CurrentProgress = value; }

    public bool IsEarned { get; set; } = false;

    public DateTime? EarnedDate { get; set; }

    public DateTime UnlockedAt { get => EarnedDate ?? DateTime.MinValue; set => EarnedDate = value; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(AchievementId))]
    public virtual Achievement Achievement { get; set; } = null!;
}
