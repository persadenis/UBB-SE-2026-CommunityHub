using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class EventStatisticsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IEventStatisticsService _statisticsService;

    public EventStatisticsController(IEventService eventService, IEventStatisticsService statisticsService)
    {
        _eventService = eventService;
        _statisticsService = statisticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        var selectedEvent = await _eventService.GetEventByIdAsync(eventId);
        if (selectedEvent == null)
        {
            return NotFound();
        }

        var viewModel = new EventStatisticsViewModel
        {
            EventId = selectedEvent.EventId,
            EventName = selectedEvent.Name
        };

        try
        {
            var overviewTask = _statisticsService.GetParticipantOverviewAsync(eventId);
            var engagementTask = _statisticsService.GetEngagementBreakdownAsync(eventId);
            var leaderboardTask = _statisticsService.GetLeaderboardAsync(eventId);
            var questAnalyticsTask = _statisticsService.GetQuestAnalyticsAsync(eventId);

            await Task.WhenAll(overviewTask, engagementTask, leaderboardTask, questAnalyticsTask);

            viewModel.ParticipantOverview = overviewTask.Result;
            viewModel.EngagementBreakdown = engagementTask.Result;
            viewModel.Leaderboard = leaderboardTask.Result;
            viewModel.QuestAnalytics = questAnalyticsTask.Result;
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }

        return View(viewModel);
    }
}
