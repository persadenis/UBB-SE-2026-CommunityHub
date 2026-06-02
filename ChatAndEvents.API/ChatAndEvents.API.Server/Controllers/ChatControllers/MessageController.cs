using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var messages = await _messageService.GetMessagesAsync(
            conversationId,
            userId,
            skip,
            take);

        return Ok(messages);
    }

    [HttpGet("{conversationId}/cannot-send-reason")]
    public async Task<IActionResult> GetCannotSendReason(
        Guid conversationId,
        [FromQuery] Guid userId)
    {
        var reason = await _messageService.GetCannotSendReasonAsync(
            conversationId,
            userId);

        return Ok(reason);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(
        [FromQuery] Guid conversationId,
        [FromQuery] Guid senderId,
        [FromQuery] string content,
        [FromQuery] Guid? replyToId)
    {
        var message = await _messageService.SendMessageAsync(
            conversationId,
            senderId,
            content,
            replyToId);

        return Ok(message);
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(
        Guid messageId,
        [FromQuery] Guid requesterId,
        [FromQuery] string newContent)
    {
        await _messageService.EditMessageAsync(
            messageId,
            requesterId,
            newContent);

        return NoContent();
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(
        Guid messageId,
        [FromQuery] Guid requesterId)
    {
        await _messageService.DeleteMessageAsync(
            messageId,
            requesterId);

        return NoContent();
    }

    [HttpPut("{conversationId}/nickname")]
    public async Task<IActionResult> SetNickname(
        Guid conversationId,
        [FromQuery] Guid userId,
        [FromQuery] string? nickname)
    {
        await _messageService.SetNicknameAsync(
            conversationId,
            userId,
            nickname);

        return NoContent();
    }
}
