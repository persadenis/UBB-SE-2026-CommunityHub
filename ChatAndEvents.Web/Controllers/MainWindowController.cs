using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services; // For IFriendRequestService
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MainWindowController : Controller
{
    private readonly IUserService _userService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly INotificationService _notificationService;

    public MainWindowController(
        IUserService userService, 
        IFriendRequestService friendRequestService,
        INotificationService notificationService)
    {
        _userService = userService;
        _friendRequestService = friendRequestService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string activeSection = "Conversations")
    {
        var currentUser = await _userService.GetCurrentUser();
        var friendRequests = await _friendRequestService.GetIncomingRequestsAsync(currentUser.UserId);
        var unreadNotifications = await _notificationService.CountUnreadAsync(currentUser.UserId);

        return View(new MainWindowViewModel
        {
            CurrentUserId = currentUser.UserId,
            CurrentUsername = currentUser.Name,
            ActiveSection = activeSection,
            
            UnreadNotificationsCount = unreadNotifications,
            PendingFriendRequestsCount = friendRequests?.Count ?? 0
        });
    }
}
