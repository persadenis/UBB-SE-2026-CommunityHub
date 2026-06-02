using ChatAndEvents.Data.EventsData.Services.discussionService;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class DiscussionController : ControllerBase
{
    private readonly IDiscussionService _discussionService;

    public DiscussionController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetMessages(int eventId, [FromQuery] Guid userId)
    {
        var messages = await _discussionService.GetMessagesAsync(eventId, userId);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(
        [FromQuery] string? text,
        [FromQuery] string? mediaPath,
        [FromQuery] int eventId,
        [FromQuery] Guid userId,
        [FromQuery] int? replyToId)
    {
        await _discussionService.CreateMessageAsync(text, mediaPath, eventId, userId, replyToId);
        return NoContent();
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId, [FromQuery] Guid userId, [FromQuery] int eventId)
    {
        await _discussionService.DeleteMessageAsync(messageId, userId, eventId);
        return NoContent();
    }

    [HttpPost("{messageId}/react")]
    public async Task<IActionResult> React(int messageId, [FromQuery] Guid userId, [FromQuery] string emoji)
    {
        await _discussionService.ReactAsync(messageId, userId, emoji);
        return NoContent();
    }

    [HttpDelete("{messageId}/react")]
    public async Task<IActionResult> RemoveReaction(int messageId, [FromQuery] Guid userId)
    {
        await _discussionService.RemoveReactionAsync(messageId, userId);
        return NoContent();
    }

    [HttpPost("{eventId}/mute")]
    public async Task<IActionResult> MuteUser(int eventId, [FromQuery] Guid targetUserId, [FromQuery] DateTime? muteUntil, [FromQuery] Guid adminUserId)
    {
        await _discussionService.MuteUserAsync(eventId, targetUserId, muteUntil, adminUserId);
        return NoContent();
    }

    [HttpPost("{eventId}/unmute")]
    public async Task<IActionResult> UnmuteUser(int eventId, [FromQuery] Guid targetUserId, [FromQuery] Guid adminUserId)
    {
        await _discussionService.UnmuteUserAsync(eventId, targetUserId, adminUserId);
        return NoContent();
    }

    [HttpPost("{eventId}/slowmode")]
    public async Task<IActionResult> SetSlowMode(int eventId, [FromQuery] int? seconds, [FromQuery] Guid adminUserId)
    {
        await _discussionService.SetSlowModeAsync(eventId, seconds, adminUserId);
        return NoContent();
    }

    [HttpGet("{eventId}/slowmode")]
    public async Task<IActionResult> GetSlowMode(int eventId)
    {
        var seconds = await _discussionService.GetSlowModeSecondsAsync(eventId);
        return Ok(seconds);
    }

    [HttpGet("{eventId}/participants")]
    public async Task<IActionResult> GetParticipants(int eventId)
    {
        var participants = await _discussionService.GetEventParticipantsAsync(eventId);
        return Ok(participants);
    }
}