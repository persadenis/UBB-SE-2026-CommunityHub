using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class MemoriesController : ControllerBase
{
    private readonly IMemoryService _memoryService;

    public MemoriesController(IMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetByEvent(int eventId, [FromBody] User currentUser)
    {
        var ev = new Event { EventId = eventId };
        var memories = await _memoryService.GetByEventAsync(ev, currentUser);
        return Ok(memories);
    }

    [HttpGet("{eventId}/photos")]
    public async Task<IActionResult> GetOnlyPhotos(int eventId)
    {
        var ev = new Event { EventId = eventId };
        var photos = await _memoryService.GetOnlyPhotosAsync(ev);
        return Ok(photos);
    }

    [HttpGet("{eventId}/mine")]
    public async Task<IActionResult> FilterByMyMemories(int eventId, [FromBody] User currentUser)
    {
        var ev = new Event { EventId = eventId };
        var memories = await _memoryService.FilterByMyMemoriesAsync(ev, currentUser);
        return Ok(memories);
    }

    [HttpGet("{eventId}/ordered")]
    public async Task<IActionResult> OrderByDate(int eventId, [FromBody] User currentUser, [FromQuery] bool ascending = true)
    {
        var ev = new Event { EventId = eventId };
        var memories = await _memoryService.OrderByDateAsync(ev, currentUser, ascending);
        return Ok(memories);
    }

    [HttpPost]
    public async Task<IActionResult> AddMemory([FromBody] User author, [FromQuery] int eventId, [FromQuery] string? photoPath, [FromQuery] string? text)
    {
        var ev = new Event { EventId = eventId };
        await _memoryService.AddAsync(ev, author, photoPath, text);
        return NoContent();
    }

    [HttpDelete("{memoryId}")]
    public async Task<IActionResult> DeleteMemory(int memoryId, [FromBody] User requestingUser)
    {
        var memory = new Memory { MemoryId = memoryId };
        await _memoryService.DeleteAsync(memory, requestingUser);
        return NoContent();
    }

    [HttpPost("{memoryId}/like")]
    public async Task<IActionResult> ToggleLike(int memoryId, [FromBody] User currentUser)
    {
        var memory = new Memory { MemoryId = memoryId };
        await _memoryService.ToggleLikeAsync(memory, currentUser);
        return NoContent();
    }

    [HttpGet("{memoryId}/likes")]
    public async Task<IActionResult> GetLikesCount(int memoryId)
    {
        var count = await _memoryService.GetLikesCountAsync(memoryId);
        return Ok(count);
    }
}