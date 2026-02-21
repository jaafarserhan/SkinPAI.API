using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class CampaignParticipant
{
    [Key]
    public long ParticipantId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Completed, Withdrawn

    public string? SubmissionUrls { get; set; } // JSON array of submitted content URLs

    public int EngagementScore { get; set; } = 0;

    [ForeignKey(nameof(CampaignId))]
    public virtual BrandCampaign Campaign { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
