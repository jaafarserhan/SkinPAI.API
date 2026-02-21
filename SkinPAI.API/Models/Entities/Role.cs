using System.ComponentModel.DataAnnotations;

namespace SkinPAI.API.Models.Entities;

public class Role
{
    [Key]
    public Guid RoleId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
