using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class FriendListController : ControllerBase
{
    private readonly IFriendListService _friendListService;

    public FriendListController(IFriendListService friendListService)
    {
        _friendListService = friendListService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetFriends(Guid userId)
    {
        var friends = await _friendListService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveFriend(
        [FromQuery] Guid currentUserId,
        [FromQuery] Guid targetFriendId)
    {
        await _friendListService.RemoveFriendAsync(
            currentUserId,
            targetFriendId);

        return NoContent();
    }
}