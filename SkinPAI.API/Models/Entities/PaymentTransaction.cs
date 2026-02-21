using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class PaymentTransaction
{
    [Key]
    public Guid TransactionId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid? SubscriptionId { get; set; }

    [Required, MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Subscription, WalletTopup, Refund

    [Required, MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty; // Card, Wallet, Stripe

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Refunded

    [MaxLength(255)]
    public string? PaymentGatewayTransactionId { get; set; }

    public string? PaymentGatewayResponse { get; set; } // JSON

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? RefundAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime TransactionDate { get => CreatedAt; set => CreatedAt = value; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(SubscriptionId))]
    public virtual UserSubscription? Subscription { get; set; }
}
