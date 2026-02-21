using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class Notification
{
    [Key]
    public Guid NotificationId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(50)]
    public string NotificationType { get; set; } = string.Empty; // Scan, Routine, Community, Achievement, System

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Message { get; set; }

    // Alias for service compatibility
    public string? Body { get => Message; set => Message = value; }

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }

    [MaxLength(500)]
    public string? IconUrl { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
