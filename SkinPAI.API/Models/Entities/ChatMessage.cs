using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinPAI.API.Models.Entities;

public class ChatMessage
{
    [Key]
    public Guid MessageId { get; set; } = Guid.NewGuid();

    public Guid SenderUserId { get; set; }

    public Guid ReceiverUserId { get; set; }

    // Aliases for service compatibility
    public Guid SenderId { get => SenderUserId; set => SenderUserId = value; }
    public Guid ReceiverId { get => ReceiverUserId; set => ReceiverUserId = value; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(50)]
    public string MessageType { get; set; } = "Text"; // Text, Image, File

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    [MaxLength(500)]
    public string? MediaUrl { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime SentAt { get => CreatedAt; set => CreatedAt = value; }

    [ForeignKey(nameof(SenderUserId))]
    public virtual User Sender { get; set; } = null!;

    [ForeignKey(nameof(ReceiverUserId))]
    public virtual User Receiver { get; set; } = null!;
}
