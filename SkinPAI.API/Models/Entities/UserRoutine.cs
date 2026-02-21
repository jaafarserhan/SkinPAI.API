using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class UserRoutine
{
    [Key]
    public Guid RoutineId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, MaxLength(200)]
    public string RoutineName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string RoutineType { get; set; } = "Morning"; // Morning, Evening

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<RoutineStep> RoutineSteps { get; set; } = new List<RoutineStep>();
    public virtual ICollection<RoutineCompletion> Completions { get; set; } = new List<RoutineCompletion>();
    public virtual ICollection<RoutineReminder> Reminders { get; set; } = new List<RoutineReminder>();
}
