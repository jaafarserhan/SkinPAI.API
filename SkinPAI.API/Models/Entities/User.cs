using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class User
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; } = false;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? SecurityStamp { get; set; }

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; } = false;

    public bool TwoFactorEnabled { get; set; } = false;

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; } = false;

    public int AccessFailedCount { get; set; } = 0;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [Required, MaxLength(20)]
    public string MembershipType { get; set; } = "Guest"; // Guest, Member, Pro

    [Required, MaxLength(20)]
    public string MembershipStatus { get; set; } = "Active"; // Active, Suspended, Cancelled

    public DateTime? MembershipStartDate { get; set; }

    public DateTime? MembershipEndDate { get; set; }

    public bool IsCreator { get; set; } = false;

    public bool IsVerified { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    public decimal WalletBalance { get; set; } = 0;

    public int TotalScansUsed { get; set; } = 0;

    public DateTime? LastScanDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Social Login / OAuth
    [MaxLength(50)]
    public string? AuthProvider { get; set; } // "email", "google", "apple", "facebook"

    [MaxLength(256)]
    public string? AuthProviderId { get; set; } // External provider's user ID

    // Questionnaire completion tracking
    public bool QuestionnaireCompleted { get; set; } = false;

    public DateTime? QuestionnaireCompletedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<SkinScan> SkinScans { get; set; } = new List<SkinScan>();
    public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    public virtual ICollection<UserRoutine> Routines { get; set; } = new List<UserRoutine>();
    public virtual ICollection<UserProductFavorite> ProductFavorites { get; set; } = new List<UserProductFavorite>();
    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual CreatorStation? CreatorStation { get; set; }
    public virtual SkinProfile? SkinProfile { get; set; }
    public virtual ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
    public virtual ICollection<StationFollower> FollowingStations { get; set; } = new List<StationFollower>();
    public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
