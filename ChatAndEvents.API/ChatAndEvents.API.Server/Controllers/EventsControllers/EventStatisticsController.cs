using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class EventStatisticsController : ControllerBase
{
    private readonly IEventStatisticsService _eventStatisticsService;

    public EventStatisticsController(IEventStatisticsService eventStatisticsService)
    {
        _eventStatisticsService = eventStatisticsService;
    }

    [HttpGet("{eventId}/participants")]
    public async Task<IActionResult> GetParticipantOverview(int eventId)
    {
        var overview = await _eventStatisticsService
            .GetParticipantOverviewAsync(eventId);

        return Ok(overview);
    }

    [HttpGet("{eventId}/engagement")]
    public async Task<IActionResult> GetEngagementBreakdown(int eventId)
    {
        var breakdown = await _eventStatisticsService
            .GetEngagementBreakdownAsync(eventId);

        return Ok(breakdown);
    }

    [HttpGet("{eventId}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(int eventId)
    {
        var leaderboard = await _eventStatisticsService
            .GetLeaderboardAsync(eventId);

        return Ok(leaderboard);
    }

    [HttpGet("{eventId}/quests")]
    public async Task<IActionResult> GetQuestAnalytics(int eventId)
    {
        var analytics = await _eventStatisticsService
            .GetQuestAnalyticsAsync(eventId);

        return Ok(analytics);
    }
}