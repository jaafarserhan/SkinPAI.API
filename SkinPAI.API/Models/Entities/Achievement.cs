using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class Achievement
{
    [Key]
    public Guid AchievementId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // Alias for service compatibility
    public string AchievementName { get => Title; set => Title = value; }

    [Required, MaxLength(50)]
    public string AchievementCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [MaxLength(500)]
    public string? IconUrl { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? Rarity { get; set; }

    [MaxLength(500)]
    public string? UnlockCriteria { get; set; }

    [Required, MaxLength(50)]
    public string AchievementType { get; set; } = string.Empty; // Scans, Routines, Community, etc.

    public int RequiredProgress { get; set; }

    public int Points { get; set; } = 0;

    // Alias for service compatibility
    public int PointsValue { get => Points; set => Points = value; }

    public int TotalUnlocked { get; set; } = 0;

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
