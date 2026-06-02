using ChatAndEvents.Data.ChatData.services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class DirectMessageController : ControllerBase
{
    private readonly IDirectMessageService _directMessageService;

    public DirectMessageController(IDirectMessageService directMessageService)
    {
        _directMessageService = directMessageService;
    }

    [HttpPost("conversation")]
    public async Task<IActionResult> GetOrCreateConversation(
        [FromQuery] Guid userId1,
        [FromQuery] Guid userId2)
    {
        var conversation = await _directMessageService
            .GetOrCreateAsync(userId1, userId2);

        return Ok(conversation);
    }

    [HttpGet("{conversationId}/other-user")]
    public async Task<IActionResult> GetOtherUser(
        Guid conversationId,
        [FromQuery] Guid viewerUserId)
    {
        var user = await _directMessageService
            .GetOtherUserAsync(conversationId, viewerUserId);

        return Ok(user);
    }

    [HttpGet("{conversationId}/blocked")]
    public async Task<IActionResult> IsBlocked(
        Guid conversationId,
        [FromQuery] Guid viewerUserId)
    {
        var blocked = await _directMessageService
            .IsBlockedAsync(conversationId, viewerUserId);

        return Ok(blocked);
    }
}