using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class EventListingController : Controller
{
    private readonly IEventService _eventService;

    public EventListingController(IEventService eventService)
    {
        _eventService = eventService;
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound();

            return View(ev);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Index", new { errorMessage = ex.Message });
        }
    }
    [HttpGet]
    public async Task<IActionResult> Index(string? searchQuery, string? locationFilter)
    {
        var viewModel = new EventListingViewModel
        {
            SearchQuery = searchQuery,
            LocationFilter = locationFilter
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                viewModel.Events = await _eventService.SearchByTitleAsync(searchQuery);
            }
            else if (!string.IsNullOrWhiteSpace(locationFilter))
            {
                viewModel.Events = await _eventService.FilterByLocationAsync(locationFilter);
            }
            else
            {
                viewModel.Events = await _eventService.GetAllPublicActiveEventsAsync();
            }
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }

        return View(viewModel);
    }
}
