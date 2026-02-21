using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class RefreshToken
{
    [Key]
    public long RefreshTokenId { get; set; }

    public Guid UserId { get; set; }

    [Required, MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    [MaxLength(50)]
    public string? RevokedByIp { get; set; }

    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsRevoked => RevokedAt != null;

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;
}
