using System;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class AnnouncementController : Controller
{
    private readonly IAnnouncementService _announcementService;
    private readonly IUserService _userService;

    public AnnouncementController(IAnnouncementService announcementService, IUserService userService)
    {
        _announcementService = announcementService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId, int? editAnnouncementId = null)
    {
        var currentUser = await _userService.GetCurrentUser();
        var isAdmin = _userService.IsAdmin(new Event { EventId = eventId });
        var announcements = await _announcementService.GetAnnouncementsAsync(eventId, currentUser.UserId);
        var unreadCounts = await _announcementService.GetUnreadCountsForUserAsync(currentUser.UserId);

        var orderedAnnouncements = announcements
            .OrderByDescending(announcement => announcement.IsPinned)
            .ThenByDescending(announcement => announcement.Date)
            .ToList();

        var items = orderedAnnouncements
            .Select(announcement => new AnnouncementItemViewModel(announcement, currentUser.UserId, isAdmin))
            .ToList();

        var viewModel = new AnnouncementViewModel
        {
            EventId = eventId,
            CurrentUserName = currentUser.Name,
            IsEventAdmin = isAdmin,
            Announcements = items,
            UnreadCount = unreadCounts.TryGetValue(eventId, out var unreadCount) ? unreadCount : 0,
        };

        if (editAnnouncementId.HasValue)
        {
            viewModel.EditingAnnouncement = items.FirstOrDefault(item => item.Model.Id == editAnnouncementId.Value);
            viewModel.NewMessage = viewModel.EditingAnnouncement?.Model.Message ?? string.Empty;
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(int eventId, string newMessage, int? announcementId)
    {
        var currentUser = await _userService.GetCurrentUser();
        var message = (newMessage ?? string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(message))
        {
            if (announcementId.HasValue)
            {
                await _announcementService.UpdateAnnouncementAsync(
                    announcementId.Value,
                    message,
                    currentUser.UserId,
                    eventId);
            }
            else
            {
                await _announcementService.CreateAnnouncementAsync(
                    message,
                    eventId,
                    currentUser.UserId);
            }
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int eventId, int announcementId)
    {
        var currentUser = await _userService.GetCurrentUser();
        await _announcementService.DeleteAnnouncementAsync(announcementId, currentUser.UserId, eventId);

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Pin(int eventId, int announcementId)
    {
        var currentUser = await _userService.GetCurrentUser();
        await _announcementService.PinAnnouncementAsync(announcementId, eventId, currentUser.UserId);

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> React(int eventId, int announcementId, string emoji)
    {
        var currentUser = await _userService.GetCurrentUser();

        if (!string.IsNullOrWhiteSpace(emoji))
        {
            await _announcementService.ToggleReactionAsync(announcementId, currentUser.UserId, emoji);
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
