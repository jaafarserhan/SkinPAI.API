using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class BrandCampaign
{
    [Key]
    public Guid CampaignId { get; set; } = Guid.NewGuid();

    public Guid BrandId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CampaignImageUrl { get; set; }

    [MaxLength(500)]
    public string? CampaignVideoUrl { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int ParticipantCount { get; set; } = 0;

    public int TotalEngagement { get; set; } = 0;

    [MaxLength(100)]
    public string? Hashtag { get; set; }

    [MaxLength(500)]
    public string? Prize { get; set; }

    public string? Requirements { get; set; } // JSON array

    public bool IsActive { get; set; } = true;

    public bool Featured { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BrandId))]
    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<CampaignParticipant> Participants { get; set; } = new List<CampaignParticipant>();
}
