using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MyEventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IAttendedEventService _attendedEventService;
    private readonly CurrentUserContext _currentUserContext;

    public MyEventsController(IEventService eventService, IAttendedEventService attendedEventService, CurrentUserContext currentUserContext)
    {
        _eventService = eventService;
        _attendedEventService = attendedEventService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new MyEventsViewModel();

        try
        {
            var userId = _currentUserContext.UserId;

            var adminEvents = await _eventService.GetMyEventsAsync(userId);
            var attendedEvents = await _attendedEventService.GetAttendedEventsAsync(userId);
            var joinedEvents = attendedEvents.Select(ae => ae.Event).ToList();

            viewModel.Events = adminEvents
                .Union(joinedEvents, new EventIdComparer())
                .OrderBy(e => e.StartDateTime)
                .ToList();
        
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Failed to load events: {ex.Message}";
        }

        return View(viewModel);
    }

    private class EventIdComparer : IEqualityComparer<ChatAndEvents.Data.EventsData.Models.Event>
    {
        public bool Equals(ChatAndEvents.Data.EventsData.Models.Event? x, ChatAndEvents.Data.EventsData.Models.Event? y) => x?.EventId == y?.EventId;
        public int GetHashCode(ChatAndEvents.Data.EventsData.Models.Event obj) => obj.EventId.GetHashCode();
    }
}