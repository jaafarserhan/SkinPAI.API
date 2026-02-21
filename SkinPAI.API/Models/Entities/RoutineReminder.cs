using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class RoutineReminder
{
    [Key]
    public Guid ReminderId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid RoutineId { get; set; }

    public TimeOnly ReminderTime { get; set; }

    [Required, MaxLength(50)]
    public string DaysOfWeek { get; set; } = "[0,1,2,3,4,5,6]"; // JSON array

    public bool IsEnabled { get; set; } = true;

    public bool SoundEnabled { get; set; } = true;

    public bool VibrationEnabled { get; set; } = true;

    public DateTime? LastSentAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(RoutineId))]
    public virtual UserRoutine Routine { get; set; } = null!;
}
