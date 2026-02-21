using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class SubscriptionPlan
{
    [Key]
    public Guid PlanId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string PlanName { get; set; } = string.Empty; // Member, Pro

    [Required, MaxLength(50)]
    public string PlanCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string BillingPeriod { get; set; } = "Monthly"; // Monthly, Yearly

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal PriceAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceMonthly { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceYearly { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public int? DailyScansLimit { get; set; } // NULL = unlimited

    public int ScansPerDay { get => DailyScansLimit ?? 0; set => DailyScansLimit = value; }

    public bool HasAdvancedAnalysis { get; set; } = false;

    public bool HasProductRecommendations { get; set; } = false;

    public bool HasProgressTracking { get; set; } = false;

    public bool HasCommunityAccess { get; set; } = false;

    public bool HasCreatorStudio { get; set; } = false;

    public bool HasPrioritySupport { get; set; } = false;

    public bool AdFree { get; set; } = false;

    public string? Features { get; set; } // JSON array of features

    public string? FeatureListJson { get => Features; set => Features = value; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
