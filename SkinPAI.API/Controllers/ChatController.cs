using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get all conversations
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(typeof(List<ChatConversationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatConversationDto>>> GetConversations()
    {
        var conversations = await _chatService.GetConversationsAsync(GetUserId());
        return Ok(conversations);
    }

    /// <summary>
    /// Get messages with a specific user
    /// </summary>
    [HttpGet("messages/{otherUserId}")]
    [ProducesResponseType(typeof(List<ChatMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _chatService.GetMessagesAsync(GetUserId(), otherUserId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Send a message
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var message = await _chatService.SendMessageAsync(GetUserId(), request);
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    [HttpPut("messages/{otherUserId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> MarkAsRead(Guid otherUserId)
    {
        await _chatService.MarkMessagesAsReadAsync(GetUserId(), otherUserId);
        return Ok(new { message = "Messages marked as read" });
    }

    /// <summary>
    /// Get unread message count
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUnreadCount()
    {
        var count = await _chatService.GetUnreadCountAsync(GetUserId());
        return Ok(new { count });
    }

    /// <summary>
    /// Delete conversation with a user
    /// </summary>
    [HttpDelete("conversations/{otherUserId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConversation(Guid otherUserId)
    {
        var deleted = await _chatService.DeleteConversationAsync(GetUserId(), otherUserId);
        if (!deleted) return NotFound();
        return Ok(new { message = "Conversation deleted" });
    }
}
