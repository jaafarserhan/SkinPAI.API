using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class UserSubscription
{
    [Key]
    public Guid SubscriptionId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid PlanId { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Cancelled, Expired, Suspended

    [MaxLength(20)]
    public string BillingCycle { get; set; } = "Monthly";

    public bool IsActive { get => Status == "Active"; set { if (value) Status = "Active"; } }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? NextBillingDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public bool AutoRenew { get; set; } = true;

    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(PlanId))]
    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
