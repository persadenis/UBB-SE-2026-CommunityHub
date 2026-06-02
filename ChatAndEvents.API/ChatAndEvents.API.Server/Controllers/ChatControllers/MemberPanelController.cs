using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class MemberPanelController : ControllerBase
{
    private readonly IMemberPanelService _memberPanelService;

    public MemberPanelController(IMemberPanelService memberPanelService)
    {
        _memberPanelService = memberPanelService;
    }

    [HttpGet("{conversationId}/members")]
    public async Task<IActionResult> GetMembers(Guid conversationId)
    {
        var members = await _memberPanelService.GetMembersAsync(conversationId);
        return Ok(members);
    }

    [HttpGet("{conversationId}/banned")]
    public async Task<IActionResult> GetBannedMembers(Guid conversationId)
    {
        var members = await _memberPanelService.GetBannedMembersAsync(conversationId);
        return Ok(members);
    }

    [HttpGet("{conversationId}/search")]
    public async Task<IActionResult> SearchUsersToAdd(
        Guid conversationId,
        [FromQuery] string query)
    {
        var users = await _memberPanelService
            .SearchUsersToAddAsync(conversationId, query);

        return Ok(users);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var user = await _memberPanelService.GetUserAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }
}