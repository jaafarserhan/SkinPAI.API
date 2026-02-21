using System.ComponentModel.DataAnnotations;

namespace SkinPAI.API.Models.Entities;

public class Brand
{
    [Key]
    public Guid BrandId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string BrandName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    public bool IsVerified { get; set; } = false;

    public bool IsPartner { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<BrandDistributor> BrandDistributors { get; set; } = new List<BrandDistributor>();
    public virtual ICollection<BrandCampaign> Campaigns { get; set; } = new List<BrandCampaign>();
}
