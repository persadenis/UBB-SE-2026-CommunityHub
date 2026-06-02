using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class FriendRequestController : ControllerBase
{
    private readonly IFriendRequestService _friendRequestService;

    public FriendRequestController(IFriendRequestService friendRequestService)
    {
        _friendRequestService = friendRequestService;
    }

    [HttpPost]
    public async Task<IActionResult> SendRequest(
        [FromQuery] Guid senderUserId,
        [FromQuery] Guid receiverUserId)
    {
        await _friendRequestService.SendFriendRequestAsync(
            senderUserId,
            receiverUserId);

        return NoContent();
    }

    [HttpPost("username")]
    public async Task<IActionResult> SendRequestByUsername(
        [FromQuery] Guid senderUserId,
        [FromQuery] string username)
    {
        var result = await _friendRequestService
            .SendFriendRequestByUsernameAsync(senderUserId, username);

        return Ok(result);
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptRequest(
        [FromQuery] Guid currentUserId,
        [FromQuery] Guid requesterUserId)
    {
        await _friendRequestService.AcceptFriendRequestAsync(
            currentUserId,
            requesterUserId);

        return NoContent();
    }

    [HttpPost("decline")]
    public async Task<IActionResult> DeclineRequest(
        [FromQuery] Guid currentUserId,
        [FromQuery] Guid requesterUserId)
    {
        await _friendRequestService.DeclineFriendRequestAsync(
            currentUserId,
            requesterUserId);

        return NoContent();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetIncomingRequests(Guid userId)
    {
        var requests = await _friendRequestService
            .GetIncomingRequestsAsync(userId);

        return Ok(requests);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetRelationshipStatus(
        [FromQuery] Guid firstUserId,
        [FromQuery] Guid secondUserId)
    {
        var status = await _friendRequestService
            .GetRelationshipStatusAsync(firstUserId, secondUserId);

        return Ok(status);
    }
}