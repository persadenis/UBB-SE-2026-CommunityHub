using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MemoryController : Controller
{
    private readonly IMemoryService _memoryService;
    private readonly IUserService _userService;

    public MemoryController(IMemoryService memoryService, IUserService userService)
    {
        _memoryService = memoryService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId, bool showOnlyMine = false,
        bool sortAscending = false, bool showGallery = false)
    {
        var currentUser = await _userService.GetCurrentUser();
        var ev = new Event { EventId = eventId };

        var memories = showOnlyMine
            ? await _memoryService.FilterByMyMemoriesAsync(ev, currentUser)
            : await _memoryService.OrderByDateAsync(ev, currentUser, sortAscending);

        var galleryPhotos = showGallery
            ? await _memoryService.GetOnlyPhotosAsync(ev)
            : new System.Collections.Generic.List<string>();

        var vm = new MemoryViewModel
        {
            EventId = eventId,
            ShowOnlyMine = showOnlyMine,
            SortAscending = sortAscending,
            ShowGallery = showGallery,
            GalleryPhotos = galleryPhotos,
            Memories = memories.Select(m => new MemoryItemWebViewModel
            {
                Memory = m,
                CanDelete = _memoryService.CanDelete(m, currentUser),
                CanLike = _memoryService.CanLike(m, currentUser)
            }).ToList()
        };

        if (TempData["ErrorMessage"] is string err)
            vm.ErrorMessage = err;

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int eventId, string? text, bool showOnlyMine, bool sortAscending)
    {
        var currentUser = await _userService.GetCurrentUser();
        var ev = new Event { EventId = eventId };
        try
        {
            await _memoryService.AddAsync(ev, currentUser, null, text);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, showOnlyMine, sortAscending });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int memoryId, int eventId, bool showOnlyMine, bool sortAscending)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _memoryService.DeleteAsync(new Memory { MemoryId = memoryId }, currentUser);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, showOnlyMine, sortAscending });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(int memoryId, int eventId, bool showOnlyMine, bool sortAscending)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _memoryService.ToggleLikeAsync(new Memory { MemoryId = memoryId }, currentUser);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, showOnlyMine, sortAscending });
    }

    [HttpGet]
    public async Task<IActionResult> Gallery(int eventId)
    {
        return RedirectToAction(nameof(Index), new { eventId, showGallery = true });
    }
}