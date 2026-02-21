using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class SkinProfile
{
    [Key]
    public Guid SkinProfileId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string? SkinType { get; set; } // Oily, Dry, Combination, Normal, Sensitive

    [MaxLength(500)]
    public string? SkinConcerns { get; set; } // JSON array

    [MaxLength(500)]
    public string? CurrentRoutine { get; set; }

    [MaxLength(100)]
    public string? SunExposure { get; set; }

    [MaxLength(500)]
    public string? Lifestyle { get; set; }

    [MaxLength(500)]
    public string? Allergies { get; set; }

    [MaxLength(500)]
    public string? MedicationsUsed { get; set; }

    public bool QuestionnaireCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
