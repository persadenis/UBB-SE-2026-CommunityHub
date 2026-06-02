using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class MentionController : ControllerBase
{
    private readonly IMentionService _mentionService;

    public MentionController(IMentionService mentionService)
    {
        _mentionService = mentionService;
    }

    [HttpGet("{conversationId}/candidates")]
    public async Task<IActionResult> GetCandidates(
        Guid conversationId,
        [FromQuery] string query)
    {
        var users = await _mentionService.GetCandidatesAsync(
            conversationId,
            query);

        return Ok(users);
    }

    [HttpPost("{conversationId}/extract")]
    public async Task<IActionResult> ExtractMentionedUserIds(
        Guid conversationId,
        [FromBody] string content)
    {
        var userIds = await _mentionService
            .ExtractMentionedUserIdsAsync(conversationId, content);

        return Ok(userIds);
    }
}