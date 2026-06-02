using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _moderationService;

    public ModerationController(IModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpPost("{conversationId}/ban")]
    public async Task<IActionResult> BanMember(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid targetId)
    {
        await _moderationService.BanMemberAsync(conversationId, adminId, targetId);
        return NoContent();
    }

    [HttpPost("{conversationId}/unban")]
    public async Task<IActionResult> UnbanMember(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid targetId)
    {
        await _moderationService.UnbanMemberAsync(conversationId, adminId, targetId);
        return NoContent();
    }

    [HttpPost("{conversationId}/timeout")]
    public async Task<IActionResult> TimeoutMember(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid targetId,
        [FromQuery] int durationMinutes)
    {
        await _moderationService.TimeoutMemberAsync(
            conversationId,
            adminId,
            targetId,
            TimeSpan.FromMinutes(durationMinutes));

        return NoContent();
    }

    [HttpPost("{conversationId}/untimeout")]
    public async Task<IActionResult> RemoveTimeout(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid targetId)
    {
        await _moderationService.RemoveTimeoutAsync(conversationId, adminId, targetId);
        return NoContent();
    }

    [HttpPost("{conversationId}/promote")]
    public async Task<IActionResult> PromoteMember(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid targetId)
    {
        await _moderationService.PromoteMemberAsync(conversationId, adminId, targetId);
        return NoContent();
    }

    [HttpPost("{conversationId}/demote")]
    public async Task<IActionResult> DemoteMember(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid targetId)
    {
        await _moderationService.DemoteMemberAsync(conversationId, adminId, targetId);
        return NoContent();
    }

    [HttpPost("{conversationId}/add-member")]
    public async Task<IActionResult> AddMember(
        Guid conversationId,
        [FromQuery] Guid adminId,
        [FromQuery] Guid newUserId)
    {
        await _moderationService.AddMemberAsync(conversationId, adminId, newUserId);
        return NoContent();
    }
}