using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class RoutineCompletion
{
    [Key]
    public long CompletionId { get; set; }

    public Guid RoutineId { get; set; }

    public Guid UserId { get; set; }

    public DateOnly CompletionDate { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(RoutineId))]
    public virtual UserRoutine Routine { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
