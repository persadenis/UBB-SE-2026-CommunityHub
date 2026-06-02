using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class AttendedEventsController : ControllerBase
{
    private readonly IAttendedEventService _attendedEventService;

    public AttendedEventsController(IAttendedEventService attendedEventService)
    {
        _attendedEventService = attendedEventService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAttendedEvents(Guid userId)
    {
        var events = await _attendedEventService.GetAttendedEventsAsync(userId);
        return Ok(events);
    }

    [HttpGet("{userId}/archived")]
    public async Task<IActionResult> GetEventsByArchiveStatus(Guid userId, [FromQuery] bool isArchived)
    {
        var events = await _attendedEventService.GetEventsByArchiveStatusAsync(userId, isArchived);
        return Ok(events);
    }

    [HttpGet("{eventId}/{userId}")]
    public async Task<IActionResult> GetAttendedEvent(int eventId, Guid userId)
    {
        var attendedEvent = await _attendedEventService.GetAsync(eventId, userId);
        if (attendedEvent == null) return NotFound();
        return Ok(attendedEvent);
    }

    [HttpPost("{eventId}/{userId}/attend")]
    public async Task<IActionResult> AttendEvent(int eventId, Guid userId)
    {
        await _attendedEventService.AttendEventAsync(eventId, userId);
        return NoContent();
    }

    [HttpDelete("{eventId}/{userId}/leave")]
    public async Task<IActionResult> LeaveEvent(int eventId, Guid userId)
    {
        await _attendedEventService.LeaveEventAsync(eventId, userId);
        return NoContent();
    }

    [HttpPut("{eventId}/{userId}/archive")]
    public async Task<IActionResult> SetArchived(int eventId, Guid userId, [FromQuery] bool isArchived)
    {
        await _attendedEventService.SetArchivedAsync(eventId, userId, isArchived);
        return NoContent();
    }

    [HttpPut("{eventId}/{userId}/favourite")]
    public async Task<IActionResult> SetFavourite(int eventId, Guid userId, [FromQuery] bool isFavourite)
    {
        await _attendedEventService.SetFavouriteAsync(eventId, userId, isFavourite);
        return NoContent();
    }

    [HttpGet("{userId}/common/{friendId}")]
    public async Task<IActionResult> GetCommonEvents(Guid userId, Guid friendId)
    {
        var events = await _attendedEventService.GetCommonEventsAsync(userId, friendId);
        return Ok(events);
    }
}