using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class EventDetailController : Controller
{
    private readonly IEventService _eventService;
    private readonly IAttendedEventService _attendedEventService;
    private readonly IUserService _userService;

    public EventDetailController(
        IEventService eventService,
        IAttendedEventService attendedEventService,
        IUserService userService)
    {
        _eventService = eventService;
        _attendedEventService = attendedEventService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        var selectedEvent = await _eventService.GetEventByIdAsync(eventId);
        if (selectedEvent == null)
        {
            return NotFound();
        }

        var currentUser = await _userService.GetCurrentUser();
        var attendedEvent = await _attendedEventService.GetAsync(eventId, currentUser.UserId);

        var viewModel = new EventDetailViewModel
        {
            SelectedEvent = selectedEvent,
            CurrentUserId = currentUser.UserId,
            IsAdmin = selectedEvent.AdminId == currentUser.UserId || selectedEvent.Admin?.UserId == currentUser.UserId,
            IsEnrolled = attendedEvent != null
        };

        if (TempData["EventDetailError"] is string error)
        {
            viewModel.ErrorMessage = error;
        }

        if (TempData["EventDetailSuccess"] is string success)
        {
            viewModel.SuccessMessage = success;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAttendance(int eventId, bool isEnrolled)
    {
        var currentUser = await _userService.GetCurrentUser();

        try
        {
            if (isEnrolled)
            {
                await _attendedEventService.LeaveEventAsync(eventId, currentUser.UserId);
            }
            else
            {
                await _attendedEventService.AttendEventAsync(eventId, currentUser.UserId);
            }
        }
        catch (Exception ex)
        {
            TempData["EventDetailError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
