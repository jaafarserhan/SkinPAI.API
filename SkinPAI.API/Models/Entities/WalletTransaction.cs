using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class WalletTransaction
{
    [Key]
    public long WalletTransactionId { get; set; }

    public Guid TransactionId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Credit, Debit, Refund

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }

    // Alias for service compatibility
    public decimal Balance { get => BalanceAfter; set => BalanceAfter = value; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ReferenceId { get; set; } // Link to payment/subscription

    [MaxLength(500)]
    public string? RelatedReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime TransactionDate { get => CreatedAt; set => CreatedAt = value; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
