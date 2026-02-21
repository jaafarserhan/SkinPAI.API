using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class RoutineStep
{
    [Key]
    public long StepId { get; set; }

    public Guid RoutineId { get; set; }

    public Guid? ProductId { get; set; }

    public int StepOrder { get; set; }

    [Required, MaxLength(200)]
    public string StepName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsCompleted { get; set; } = false;

    [ForeignKey(nameof(RoutineId))]
    public virtual UserRoutine Routine { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}
