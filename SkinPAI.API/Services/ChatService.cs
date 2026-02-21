using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface IChatService
{
    Task<List<ChatConversationDto>> GetConversationsAsync(Guid userId);
    Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50);
    Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendMessageRequest request);
    Task<bool> MarkMessagesAsReadAsync(Guid userId, Guid otherUserId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> DeleteConversationAsync(Guid userId, Guid otherUserId);
}

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<ChatConversationDto>> GetConversationsAsync(Guid userId)
    {
        // Get all messages where user is sender or receiver
        var messages = await _unitOfWork.ChatMessages.Query()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted)
            .ToListAsync();

        // Group by conversation partner
        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var lastMessage = g.OrderByDescending(m => m.SentAt).First();
                var partnerId = g.Key;
                var partner = lastMessage.SenderId == partnerId ? lastMessage.Sender : lastMessage.Receiver;
                var unreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead);

                return new ChatConversationDto(
                    partnerId,
                    partner?.FullName ?? "Unknown",
                    partner?.ProfileImageUrl,
                    partner?.IsVerified ?? false,
                    lastMessage.Content,
                    lastMessage.SentAt,
                    unreadCount
                );
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return conversations;
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50)
    {
        var messages = await _unitOfWork.ChatMessages.Query()
            .Include(m => m.Sender)
            .Where(m =>
                ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                 (m.SenderId == otherUserId && m.ReceiverId == userId)) &&
                !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages
            .OrderBy(m => m.SentAt) // Return in chronological order
            .Select(m => new ChatMessageDto(
                m.MessageId,
                m.SenderId,
                m.Sender?.FullName,
                m.Sender?.ProfileImageUrl,
                m.ReceiverId,
                m.MessageType,
                m.Content,
                m.MediaUrl,
                m.IsRead,
                m.SentAt,
                m.ReadAt,
                m.SenderId == userId
            )).ToList();
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendMessageRequest request)
    {
        // Check if receiver exists
        var receiver = await _unitOfWork.Users.GetByIdAsync(request.ReceiverId);
        if (receiver == null)
            throw new KeyNotFoundException("Receiver not found");

        var sender = await _unitOfWork.Users.GetByIdAsync(senderId);

        var message = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            MessageType = request.MessageType ?? "Text",
            Content = request.Content,
            MediaUrl = request.MediaUrl
        };

        await _unitOfWork.ChatMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        return new ChatMessageDto(
            message.MessageId,
            message.SenderId,
            sender?.FullName,
            sender?.ProfileImageUrl,
            message.ReceiverId,
            message.MessageType,
            message.Content,
            message.MediaUrl,
            message.IsRead,
            message.SentAt,
            message.ReadAt,
            true
        );
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid userId, Guid otherUserId)
    {
        var unreadMessages = await _unitOfWork.ChatMessages.Query()
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

        if (!unreadMessages.Any()) return false;

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            _unitOfWork.ChatMessages.Update(message);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.ChatMessages.CountAsync(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted);
    }

    public async Task<bool> DeleteConversationAsync(Guid userId, Guid otherUserId)
    {
        var messages = await _unitOfWork.ChatMessages.Query()
            .Where(m =>
                ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                 (m.SenderId == otherUserId && m.ReceiverId == userId)))
            .ToListAsync();

        if (!messages.Any()) return false;

        // Soft delete messages
        foreach (var message in messages)
        {
            message.IsDeleted = true;
            _unitOfWork.ChatMessages.Update(message);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
