using ChatAndEvents.Data.ChatData.services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class ConversationListController : ControllerBase
{
    private readonly IConversationListService _conversationListService;

    public ConversationListController(IConversationListService conversationListService)
    {
        _conversationListService = conversationListService;
    }

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetById(Guid conversationId)
    {
        var conversation = await _conversationListService.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            return NotFound();
        }

        return Ok(conversation);
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAll(Guid userId)
    {
        var conversations = await _conversationListService.GetAllAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{userId}/dms")]
    public async Task<IActionResult> GetDms(Guid userId)
    {
        var conversations = await _conversationListService.GetDmsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{userId}/groups")]
    public async Task<IActionResult> GetGroups(Guid userId)
    {
        var conversations = await _conversationListService.GetGroupsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{userId}/favourites")]
    public async Task<IActionResult> GetFavourites(Guid userId)
    {
        var conversations = await _conversationListService.GetFavouritesAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{userId}/unread")]
    public async Task<IActionResult> GetUnread(Guid userId)
    {
        var conversations = await _conversationListService.GetUnreadAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{userId}/search")]
    public async Task<IActionResult> Search(Guid userId, [FromQuery] string query)
    {
        var conversations = await _conversationListService.SearchAsync(userId, query);
        return Ok(conversations);
    }

    [HttpGet("{conversationId}/last-message")]
    public async Task<IActionResult> GetLastMessage(Guid conversationId)
    {
        var message = await _conversationListService.GetLastMessageAsync(conversationId);
        return Ok(message);
    }

    [HttpGet("{conversationId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(
        Guid conversationId,
        [FromQuery] Guid userId)
    {
        var count = await _conversationListService.GetUnreadCountAsync(conversationId, userId);
        return Ok(count);
    }

    [HttpPut("{conversationId}/favourite")]
    public async Task<IActionResult> SetFavourite(
        Guid conversationId,
        [FromQuery] Guid userId,
        [FromQuery] bool isFavourite)
    {
        await _conversationListService.SetFavouriteAsync(conversationId, userId, isFavourite);
        return NoContent();
    }
}

