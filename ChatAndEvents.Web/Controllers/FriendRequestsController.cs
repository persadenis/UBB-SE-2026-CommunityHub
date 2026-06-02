using System;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class FriendRequestsController : Controller
{
    private readonly IFriendRequestService _friendRequestService;
    private readonly CurrentUserContext _currentUserContext;

    public FriendRequestsController(
        IFriendRequestService friendRequestService,
        CurrentUserContext currentUserContext)
    {
        _friendRequestService = friendRequestService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Route("FriendRequests")]
    [Route("FriendRequestsView")]
    public async Task<IActionResult> Index()
    {
        var incomingRequests = await _friendRequestService.GetIncomingRequestsAsync(_currentUserContext.UserId);

        var viewModel = new FriendRequestsViewModel
        {
            IncomingRequests = incomingRequests
                .Select(request => new FriendListItemViewModel(request))
                .ToList(),
            RequestActionMessage = TempData["RequestActionMessage"] as string,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid requesterId)
    {
        if (requesterId != Guid.Empty)
        {
            await _friendRequestService.AcceptFriendRequestAsync(_currentUserContext.UserId, requesterId);
            TempData["RequestActionMessage"] = "Friend request accepted.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(Guid requesterId)
    {
        if (requesterId != Guid.Empty)
        {
            await _friendRequestService.DeclineFriendRequestAsync(_currentUserContext.UserId, requesterId);
            TempData["RequestActionMessage"] = "Friend request declined.";
        }

        return RedirectToAction(nameof(Index));
    }
}
