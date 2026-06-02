using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.Web.Controllers;

public class AttendedEventsController : Controller
{
    private readonly IAttendedEventService _attendedEventService;
    private readonly CurrentUserContext _currentUserContext;

    public AttendedEventsController(IAttendedEventService attendedEventService, CurrentUserContext currentUserContext)
    {
        _attendedEventService = attendedEventService;
        _currentUserContext = currentUserContext;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, string? sort)
    {
        var viewModel = new AttendedEventsViewModel
        {
            SearchQuery = search ?? string.Empty,
            SelectedCategoryId = categoryId,
            SelectedSort = sort ?? "Default"
        };

        try
        {
            var userId = _currentUserContext.UserId;
            var allEvents = await _attendedEventService.GetAttendedEventsAsync(userId);

            var categories = allEvents
                .Where(ae => ae.Event?.Category != null)
                .Select(ae => ae.Event.Category!)
                .GroupBy(c => c.CategoryId)
                .Select(g => g.First())
                .ToList();

            viewModel.AvailableCategories = categories;

            IEnumerable<AttendedEvent> filtered = allEvents;

            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(ae => ae.Event.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (categoryId.HasValue)
                filtered = filtered.Where(ae => ae.Event.Category?.CategoryId == categoryId);

            var active = filtered.Where(ae => !ae.IsArchived).ToList();
            var archived = filtered.Where(ae => ae.IsArchived).ToList();
            var favourites = allEvents
                .Where(ae => ae.IsFavourite && !ae.IsArchived)
                .Where(ae => string.IsNullOrWhiteSpace(search) || ae.Event.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(ae => !categoryId.HasValue || ae.Event.Category?.CategoryId == categoryId)
                .ToList();

            active = ApplySort(active, viewModel.SelectedSort);
            archived = ApplySort(archived, viewModel.SelectedSort);
            favourites = ApplySort(favourites, viewModel.SelectedSort);

            if (viewModel.SelectedSort == "Default")
                active = active.OrderByDescending(ae => ae.IsFavourite).ToList();

            viewModel.AttendedEvents = active;
            viewModel.ArchivedEvents = archived;
            viewModel.FavouriteEvents = favourites;
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Failed to load events: {ex.Message}";
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Leave(int eventId, string? search, int? categoryId, string? sort)
    {
        await _attendedEventService.LeaveEventAsync(eventId, _currentUserContext.UserId);
        return RedirectToAction(nameof(Index), new { search, categoryId, sort });
    }

    [HttpPost]
    public async Task<IActionResult> SetArchived(int eventId, bool isArchived, string? search, int? categoryId, string? sort)
    {
        await _attendedEventService.SetArchivedAsync(eventId, _currentUserContext.UserId, isArchived);
        return RedirectToAction(nameof(Index), new { search, categoryId, sort });
    }

    [HttpPost]
    public async Task<IActionResult> SetFavourite(int eventId, bool isFavourite, string? search, int? categoryId, string? sort)
    {
        await _attendedEventService.SetFavouriteAsync(eventId, _currentUserContext.UserId, isFavourite);
        return RedirectToAction(nameof(Index), new { search, categoryId, sort });
    }

    [HttpGet]
    public async Task<IActionResult> CommonEvents(Guid friendId)
    {
        var events = await _attendedEventService.GetCommonEventsAsync(_currentUserContext.UserId, friendId);
        var viewModel = new AttendedEventsViewModel { CommonEvents = events };
        return View(viewModel);
    }

    private static List<AttendedEvent> ApplySort(List<AttendedEvent> list, string sort) => sort switch
    {
        "TitleAscending" => list.OrderBy(ae => ae.Event.Name).ToList(),
        "TitleDescending" => list.OrderByDescending(ae => ae.Event.Name).ToList(),
        "CategoryAscending" => list.OrderBy(ae => ae.Event.Category?.Title).ToList(),
        "CategoryDescending" => list.OrderByDescending(ae => ae.Event.Category?.Title).ToList(),
        "StartDateAscending" => list.OrderBy(ae => ae.Event.StartDateTime).ToList(),
        "StartDateDescending" => list.OrderByDescending(ae => ae.Event.StartDateTime).ToList(),
        "EndDateAscending" => list.OrderBy(ae => ae.Event.EndDateTime).ToList(),
        "EndDateDescending" => list.OrderByDescending(ae => ae.Event.EndDateTime).ToList(),
        _ => list
    };
}