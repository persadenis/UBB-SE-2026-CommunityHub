using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class QuestsController : ControllerBase
{
    private readonly IQuestService _questService;

    public QuestsController(IQuestService questService)
    {
        _questService = questService;
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetQuests(int eventId)
    {
        var ev = new Event { EventId = eventId };

        var quests = await _questService.GetQuestsAsync(ev);
        return Ok(quests);
    }

    [HttpGet("preset")]
    public async Task<IActionResult> GetPresetQuests()
    {
        var quests = await _questService.GetPresetQuestsAsync();
        return Ok(quests);
    }

    [HttpPost("{eventId}")]
    public async Task<IActionResult> AddQuest(
        int eventId,
        [FromBody] Quest quest)
    {
        var ev = new Event { EventId = eventId };

        var id = await _questService.AddQuestAsync(ev, quest);
        return Ok(id);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteQuest([FromBody] Quest quest)
    {
        await _questService.DeleteQuestAsync(quest);
        return NoContent();
    }
}