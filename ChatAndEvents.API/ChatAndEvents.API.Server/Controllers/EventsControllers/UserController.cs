using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEventService _eventService;

    public UserController(IUserService userService, IEventService eventService)
    {
        _userService = userService;
        _eventService = eventService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("A userId query parameter is required.");
        }

        var user = await _userService.GetUserById(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _userService.GetUserById(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{userId}/friends")]
    public IActionResult GetFriends(Guid userId)
    {
        var friends = _userService.GetFriends(userId);
        return Ok(friends);
    }

    [HttpGet("{userId}/search-friends")]
    public IActionResult SearchFriends(
        Guid userId,
        [FromQuery] string name)
    {
        var friends = _userService.SearchFriends(userId, name);
        return Ok(friends);
    }

    [HttpGet("{eventId}/attending")]
    public async Task<IActionResult> IsAttending(
        int eventId,
        [FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("A userId query parameter is required.");
        }

        if (_userService is ChatUserService chatUserService)
        {
            chatUserService.SetCurrentUserId(userId);
        }

        var ev = new Event { EventId = eventId };
        var result = await _userService.IsAttending(ev);

        return Ok(result);
    }

    [HttpGet("{eventId}/admin")]
    public async Task<IActionResult> IsAdmin(
        int eventId,
        [FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("A userId query parameter is required.");
        }

        var ev = await _eventService.GetEventByIdAsync(eventId);
        if (ev == null)
        {
            return NotFound();
        }

        var result = ev.Admin?.UserId == userId;

        return Ok(result);
    }
}
