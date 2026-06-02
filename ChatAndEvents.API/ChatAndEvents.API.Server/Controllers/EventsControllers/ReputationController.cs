using ChatAndEvents.Data.EventsData.Services.reputationService;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class ReputationController : ControllerBase
{
    private readonly IReputationService _reputationService;

    public ReputationController(IReputationService reputationService)
    {
        _reputationService = reputationService;
    }

    [HttpGet("{userId}/score")]
    public async Task<IActionResult> GetReputationScore(Guid userId)
    {
        var score = await _reputationService.GetReputationScoreAsync(userId);
        return Ok(score);
    }

    [HttpGet("{userId}/points")]
    public async Task<IActionResult> GetReputationPoints(Guid userId)
    {
        var points = await _reputationService.GetReputationPointsAsync(userId);
        return Ok(points);
    }

    [HttpGet("{userId}/tier")]
    public async Task<IActionResult> GetTier(Guid userId)
    {
        var tier = await _reputationService.GetTierAsync(userId);
        return Ok(tier);
    }

    [HttpGet("{userId}/achievements")]
    public async Task<IActionResult> GetAchievements(Guid userId)
    {
        var achievements = await _reputationService.GetAchievementsAsync(userId);
        return Ok(achievements);
    }

    [HttpGet("{userId}/can-post-memories")]
    public async Task<IActionResult> CanPostMemories(Guid userId)
    {
        var result = await _reputationService.CanPostMemoriesAsync(userId);
        return Ok(result);
    }

    [HttpGet("{userId}/can-post-messages")]
    public async Task<IActionResult> CanPostMessages(Guid userId)
    {
        var result = await _reputationService.CanPostMessagesAsync(userId);
        return Ok(result);
    }

    [HttpGet("{userId}/can-create-events")]
    public async Task<IActionResult> CanCreateEvents(Guid userId)
    {
        var result = await _reputationService.CanCreateEventsAsync(userId);
        return Ok(result);
    }

    [HttpGet("{userId}/can-attend-events")]
    public async Task<IActionResult> CanAttendEvents(Guid userId)
    {
        var result = await _reputationService.CanAttendEventsAsync(userId);
        return Ok(result);
    }
}