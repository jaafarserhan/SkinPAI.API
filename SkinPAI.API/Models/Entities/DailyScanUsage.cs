using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class DailyScanUsage
{
    [Key]
    public long UsageId { get; set; }

    public Guid UserId { get; set; }

    public DateOnly ScanDate { get; set; }

    public int ScanCount { get; set; } = 0;

    public DateTime LastScanAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
