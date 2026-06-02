using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class MessageInteractionController : ControllerBase
{
    private readonly IMessageInteractionService _messageInteractionService;

    public MessageInteractionController(IMessageInteractionService messageInteractionService)
    {
        _messageInteractionService = messageInteractionService;
    }

    [HttpPost("{messageId}/react")]
    public async Task<IActionResult> ReactToMessage(
        Guid messageId,
        [FromQuery] Guid userId,
        [FromQuery] string emoji)
    {
        await _messageInteractionService.ReactToMessageAsync(
            messageId,
            userId,
            emoji);

        return NoContent();
    }

    [HttpDelete("{messageId}/react")]
    public async Task<IActionResult> RemoveReaction(
        Guid messageId,
        [FromQuery] Guid userId)
    {
        await _messageInteractionService.RemoveReactionAsync(messageId, userId);
        return NoContent();
    }

    [HttpGet("{messageId}/reactions")]
    public async Task<IActionResult> GetReactions(Guid messageId)
    {
        var reactions = await _messageInteractionService.GetReactionsAsync(messageId);
        return Ok(reactions);
    }

    [HttpGet("{messageId}/reply-preview")]
    public async Task<IActionResult> BuildReplyPreview(Guid messageId)
    {
        var preview = await _messageInteractionService.BuildReplyPreviewAsync(messageId);
        return Ok(preview);
    }

    [HttpGet("{messageId}/reply-preview-parts")]
    public async Task<IActionResult> BuildReplyPreviewParts(Guid messageId)
    {
        var result = await _messageInteractionService
            .BuildReplyPreviewPartsAsync(messageId);

        return Ok(result);
    }
}