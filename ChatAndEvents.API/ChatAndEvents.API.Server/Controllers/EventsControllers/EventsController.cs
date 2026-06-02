using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPublicActiveEvents()
    {
        var events = await _eventService.GetAllPublicActiveEventsAsync();
        return Ok(events);
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetEventById(int eventId)
    {
        var ev = await _eventService.GetEventByIdAsync(eventId);
        if (ev == null) return NotFound();
        return Ok(ev);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] Event eventEntity)
    {
        var id = await _eventService.CreateEventAsync(eventEntity);
        return Ok(id);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateEvent([FromBody] Event eventEntity)
    {
        await _eventService.UpdateEventAsync(eventEntity);
        return NoContent();
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        await _eventService.DeleteEventAsync(eventId);
        return NoContent();
    }

    [HttpGet("admin/{adminId}")]
    public async Task<IActionResult> GetMyEvents(Guid adminId)
    {
        var events = await _eventService.GetMyEventsAsync(adminId);
        return Ok(events);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchByTitle([FromQuery] string title)
    {
        var events = await _eventService.SearchByTitleAsync(title);
        return Ok(events);
    }

    [HttpGet("filter/category")]
    public async Task<IActionResult> FilterByCategory([FromQuery] string category)
    {
        var events = await _eventService.FilterByCategoryAsync(category);
        return Ok(events);
    }

    [HttpGet("filter/location")]
    public async Task<IActionResult> FilterByLocation([FromQuery] string location)
    {
        var events = await _eventService.FilterByLocationAsync(location);
        return Ok(events);
    }

    [HttpGet("filter/date")]
    public async Task<IActionResult> FilterByDate([FromQuery] DateTime date)
    {
        var events = await _eventService.FilterByDateAsync(date);
        return Ok(events);
    }

    [HttpGet("filter/daterange")]
    public async Task<IActionResult> FilterByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var events = await _eventService.FilterByDateRangeAsync(from, to);
        return Ok(events);
    }
}