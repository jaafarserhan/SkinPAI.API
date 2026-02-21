using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class StationFollower
{
    [Key]
    public long FollowId { get; set; }

    public Guid StationId { get; set; }

    public Guid FollowerUserId { get; set; }

    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

    public bool NotificationsEnabled { get; set; } = true;

    [ForeignKey(nameof(StationId))]
    public virtual CreatorStation Station { get; set; } = null!;

    [ForeignKey(nameof(FollowerUserId))]
    public virtual User Follower { get; set; } = null!;
}
