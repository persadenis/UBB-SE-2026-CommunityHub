using ChatAndEvents.Data.ChatData.services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> SearchMessages(
        [FromQuery] Guid conversationId,
        [FromQuery] Guid userId,
        [FromQuery] string query)
    {
        var messages = await _searchService.SearchMessagesAsync(
            conversationId,
            userId,
            query);

        return Ok(messages);
    }

    [HttpGet("users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        var users = await _searchService.SearchUsersAsync(query);
        return Ok(users);
    }

    [HttpGet("members")]
    public async Task<IActionResult> SearchUsersForAddMember(
        [FromQuery] Guid conversationId,
        [FromQuery] string query)
    {
        var users = await _searchService.SearchUsersForAddMemberAsync(
            conversationId,
            query);

        return Ok(users);
    }
}