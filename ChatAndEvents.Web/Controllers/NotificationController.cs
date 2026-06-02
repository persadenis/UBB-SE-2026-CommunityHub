using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;

    public NotificationController(INotificationService notificationService, IUserService userService)
    {
        _notificationService = notificationService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new NotificationViewModel();
        try
        {
            var currentUser = await _userService.GetCurrentUser();
            var notifications = await _notificationService.GetNotificationsAsync(currentUser.UserId);
            viewModel.Notifications = notifications ?? new List<Notification>();
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Could not load notifications: {ex.Message}";
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _notificationService.DeleteAsync(id);
        }
        catch (Exception)
        {
            // If it fails, we just redirect back to the page anyway
            TempData["ErrorMessage"] = "Failed to delete notification.";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var currentUser = await _userService.GetCurrentUser();
        await _notificationService.MarkAsReadAsync(id, currentUser.UserId);
        return RedirectToAction(nameof(Index));
    }
}
