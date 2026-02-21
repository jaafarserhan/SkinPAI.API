using System.ComponentModel.DataAnnotations;

namespace SkinPAI.API.Models.Entities;

public class Distributor
{
    [Key]
    public Guid DistributorId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public bool IsPartner { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<BrandDistributor> BrandDistributors { get; set; } = new List<BrandDistributor>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
