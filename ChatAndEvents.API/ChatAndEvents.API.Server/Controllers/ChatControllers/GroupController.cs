using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup(
        [FromQuery] Guid creatorId,
        [FromQuery] string title,
        [FromQuery] string? iconUrl,
        [FromBody] List<Guid> memberIds)
    {
        var group = await _groupService.CreateGroupAsync(
            creatorId,
            title,
            iconUrl,
            memberIds);

        return Ok(group);
    }

    [HttpPut("{conversationId}")]
    public async Task<IActionResult> UpdateGroupInfo(
        Guid conversationId,
        [FromQuery] Guid requesterId,
        [FromQuery] string? newTitle,
        [FromQuery] string? newIconUrl)
    {
        await _groupService.UpdateGroupInfoAsync(
            conversationId,
            requesterId,
            newTitle,
            newIconUrl);

        return NoContent();
    }

    [HttpPost("{conversationId}/leave")]
    public async Task<IActionResult> LeaveGroup(
        Guid conversationId,
        [FromQuery] Guid userId)
    {
        await _groupService.LeaveGroupAsync(conversationId, userId);
        return NoContent();
    }

    [HttpPost("{conversationId}/pin")]
    public async Task<IActionResult> PinMessage(
        Guid conversationId,
        [FromQuery] Guid requesterId,
        [FromQuery] Guid messageId)
    {
        await _groupService.PinMessageAsync(conversationId, requesterId, messageId);
        return NoContent();
    }

    [HttpPost("{conversationId}/unpin")]
    public async Task<IActionResult> UnpinMessage(
        Guid conversationId,
        [FromQuery] Guid requesterId)
    {
        await _groupService.UnpinMessageAsync(conversationId, requesterId);
        return NoContent();
    }

    [HttpPost("{conversationId}/event-notice")]
    public async Task<IActionResult> PostEventNotice(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] string eventTitle,
        [FromQuery] DateTime eventDate)
    {
        await _groupService.PostEventNoticeAsync(
            conversationId,
            adminId,
            eventTitle,
            eventDate);

        return NoContent();
    }
}