using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class BrandDistributor
{
    [Key]
    public long BrandDistributorId { get; set; }

    public Guid BrandId { get; set; }

    public Guid DistributorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BrandId))]
    public virtual Brand Brand { get; set; } = null!;

    [ForeignKey(nameof(DistributorId))]
    public virtual Distributor Distributor { get; set; } = null!;
}
