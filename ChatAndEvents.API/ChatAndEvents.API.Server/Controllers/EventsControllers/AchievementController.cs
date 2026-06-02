using ChatAndEvents.Data.EventsData.Services.achievementServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserAchievements(Guid userId)
    {
        var achievements = await _achievementService
            .GetUserAchievementsAsync(userId);

        return Ok(achievements);
    }

    [HttpPost("{userId}/check")]
    public async Task<IActionResult> CheckAndAwardAchievements(Guid userId)
    {
        await _achievementService.CheckAndAwardAchievementsAsync(userId);
        return NoContent();
    }
}